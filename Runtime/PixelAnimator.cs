using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.AnimationData;
using binc.PixelAnimator.DataManipulations;

namespace binc.PixelAnimator
{
    /// <summary>
    /// Runtime player for <see cref="PixelAnimation"/> assets.
    /// 
    /// Swaps sprites at a fixed FPS, invokes per–frame user methods,
    /// and manages collision/trigger box groups (e.g., hitboxes/hurtboxes).
    ///
    /// <para>
    /// <b>Usage</b>:<br/>
    /// 1) Add this component to a GameObject (requires a SpriteRenderer).<br/>
    /// 2) Assign animations to <see cref="animationController"/>.<br/>
    /// 3) Call <see cref="Play(PixelAnimation)"/> to start playback.<br/>
    /// </para>
    ///
    /// <remarks>
    /// - Per–frame function calls are compiled once via <c>MethodStorage</c>
    ///   to avoid runtime reflection on the hot path.<br/>
    /// - Box groups (<see cref="BoxGroup"/>) are represented as child GameObjects
    ///   with <see cref="BoxCollider2D"/> components and kept in sync each frame.
    /// </remarks>
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public class PixelAnimator : MonoBehaviour
    {
        /// <summary>
        /// Fired when the current frame index changes.
        /// The argument is the new <see cref="FrameIndex"/>.
        /// </summary>
        public event Action<int> OnFrameChanged;

        private static PixelAnimationPreferences preferences;

        [Header("References")]
        [Tooltip("Target SpriteRenderer used to swap frame sprites.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("Holds the available PixelAnimation clips for this animator.")]
        [SerializeField] private PixelAnimationController animationController;

        /// <summary>Currently playing animation (null means nothing is playing).</summary>
        public PixelAnimation PlayingAnimation => playingAnimation;

        [Header("State (Read-Only)")]
        [SerializeField, ReadOnly]
        private PixelAnimation playingAnimation;

        /// <summary>Optional next animation (not used yet).</summary>
        private PixelAnimation nextAnim;

        /// <summary>Current 0-based frame index.</summary>
        public int FrameIndex { get; private set; }

        private float elapsedTime;

        /// <summary>Whether the animator is currently playing.</summary>
        public bool IsPlaying { get; private set; }

        private bool isLastFrame;
        private GameObject titleObject;

        // Keeps collider GameObjects by their logical group name.
        private readonly Dictionary<string, GameObject> colliderObjects = new();

        // Store the handler so we can properly unsubscribe (anonymous lambdas cannot be removed reliably).
        private Action<int> applyFrameHandler;

        #region Unity Lifecycle

        private void OnEnable()
        {
            applyFrameHandler ??= _ => ApplyFrame();
            OnFrameChanged += applyFrameHandler;
        }

        private void OnDisable()
        {
            if (applyFrameHandler != null)
                OnFrameChanged -= applyFrameHandler;
        }

        private void OnDestroy()
        {
            if (applyFrameHandler != null)
                OnFrameChanged -= applyFrameHandler;
        }

        private void Awake()
        {
            if (!spriteRenderer)
                spriteRenderer = GetComponent<SpriteRenderer>();

            TryLoadPreferences();
            CreateTitle();
            CompileFunctions();
        }

        private void Update()
        {
            NextFrame();
        }

        #endregion

        #region Bootstrapping / Preferences

        /// <summary>
        /// Loads the first <see cref="PixelAnimationPreferences"/> asset (static cached).
        /// Prevents repeated asset lookups across scenes.
        /// </summary>
        private static void TryLoadPreferences()
        {
            if (preferences != null) return;
            preferences = PixelAnimatorUtility.LoadFirstAssetOfType<PixelAnimationPreferences>();
        }

        /// <summary>
        /// Creates a parent GameObject to hold collider child objects.
        /// </summary>
        private void CreateTitle()
        {
            titleObject = new GameObject("---PixelAnimator Colliders---")
            {
                transform =
                {
                    parent = transform,
                    localPosition = Vector3.zero
                }
            };
        }

        /// <summary>
        /// Compiles all MethodStorage entries for every animation exactly once.
        /// This boosts per-frame invoke performance and avoids runtime reflection.
        /// </summary>
        private void CompileFunctions()
        {
            foreach (var pixelAnimation in animationController.Animations)
            {
                // Per-sprite user methods:
                foreach (var methodStorage in pixelAnimation.PixelSprites.Select(ps => ps.methodStorage))
                    methodStorage.CompileAllFunctions(gameObject);

                // Box group enter/stay/exit methods:
                foreach (var boxGroup in pixelAnimation.BoxGroups)
                {
                    var isTrigger = boxGroup.collisionTypes == CollisionTypes.Trigger;

                    foreach (var box in boxGroup.boxes)
                    {
                        foreach (var frame in box.frames)
                        {
                            try
                            {
                                if (isTrigger)
                                {
                                    ((MethodStorage<Collider2D>)frame.enterMethodStorage).CompileAllFunctions(gameObject);
                                    ((MethodStorage<Collider2D>)frame.stayMethodStorage).CompileAllFunctions(gameObject);
                                    ((MethodStorage<Collider2D>)frame.exitMethodStorage).CompileAllFunctions(gameObject);
                                }
                                else
                                {
                                    ((MethodStorage<Collision2D>)frame.enterMethodStorage).CompileAllFunctions(gameObject);
                                    ((MethodStorage<Collision2D>)frame.stayMethodStorage).CompileAllFunctions(gameObject);
                                    ((MethodStorage<Collision2D>)frame.exitMethodStorage).CompileAllFunctions(gameObject);
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                                Debug.LogError($"Animation: {pixelAnimation.name} Group: {pixelAnimation.BoxGroups.IndexOf(boxGroup)} Box: {boxGroup.boxes.IndexOf(box)} Frame: {box.frames.IndexOf(frame)}");
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Playback

        /// <summary>
        /// Advances playback time and jumps over frames if needed (catch-up).
        /// Called from <see cref="Update"/>.
        /// </summary>
        private void NextFrame()
        {
            if (!IsPlaying || PlayingAnimation == null) return;

            elapsedTime += Time.deltaTime;

            var secondsPerFrame = 1f / (float)PlayingAnimation.fps;
            var spriteCount = PlayingAnimation.PixelSprites.Count;

            // Use a loop to catch up multiple frames if deltaTime is large.
            while (elapsedTime >= secondsPerFrame)
            {
                // If looping or not on the last frame, just advance.
                if (FrameIndex != spriteCount - 1 || PlayingAnimation.loop)
                {
                    FrameIndex = (FrameIndex + 1) % spriteCount;
                    OnFrameChanged?.Invoke(FrameIndex);
                    elapsedTime -= secondsPerFrame;
                    continue;
                }

                // Non-looping: allow one extra wait on the last frame, then stop.
                if (FrameIndex == spriteCount - 1 && !PlayingAnimation.loop && !isLastFrame)
                {
                    isLastFrame = true;
                    elapsedTime -= secondsPerFrame;
                    continue;
                }

                // Playback finished.
                elapsedTime -= secondsPerFrame;
                IsPlaying = false;
                break;
            }
        }

        /// <summary>
        /// Applies the current frame: swaps the sprite, invokes user methods, and updates box sizes.
        /// Triggered by <see cref="OnFrameChanged"/>.
        /// </summary>
        private void ApplyFrame()
        {
            spriteRenderer.sprite = PlayingAnimation.PixelSprites[FrameIndex].sprite;
            CallEvent();
            SetBoxSize();
        }

        /// <summary>
        /// Invokes user-defined UnityEvents/methods for the current frame.
        /// </summary>
        private void CallEvent()
        {
            var unityEvent = PlayingAnimation.PixelSprites[FrameIndex].methodStorage.methods;
            unityEvent?.Invoke();
        }

        /// <summary>
        /// Updates each <see cref="BoxCollider2D"/> offset/size according to the current frame.
        /// </summary>
        private void SetBoxSize()
        {
            try
            {
                var groups = PlayingAnimation.BoxGroups;

                for (var g = 0; g < groups.Count; g++)
                {
                    var group = groups[g];
                    var groupObj = titleObject.transform.GetChild(g);
                    var cols = groupObj.GetComponents<BoxCollider2D>();

                    if (cols.Length != group.boxes.Count) return;

                    for (var l = 0; l < group.boxes.Count; l++)
                    {
                        var layer = group.boxes[l];
                        var box = GetAdjustedRect(layer, FrameIndex);
                        cols[l].offset = new Vector2(box.x, box.y);
                        cols[l].size = new Vector2(box.width, box.height);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Returns the mapped and (if needed) flip-adjusted rect for a box layer at a given frame.
        /// </summary>
        /// <param name="box">Source box layer.</param>
        /// <param name="index">Frame index (−1 treated as 0).</param>
        private Rect GetAdjustedRect(BoxLayer box, int index)
        {
            var f = index == -1 ? 0 : index;
            var frame = box.GetFrame(index);
            var rect = frame == null
                ? Rect.zero
                : PixelAnimatorUtility.MapBoxRectToTransform(frame.boxRect, PlayingAnimation.PixelSprites[f].sprite);

            // Mirror on X when flipX is enabled.
            rect.x *= spriteRenderer.flipX ? -1 : 1;
            return rect;
        }

        #endregion

        #region Collider Object Management

        /// <summary>
        /// Creates a child GameObject for the given <see cref="BoxData"/> and assigns layer/scale.
        /// </summary>
        private GameObject CreateColliderObject(BoxData boxData)
        {
            var gameObj = new GameObject(boxData.boxName)
            {
                transform =
                {
                    parent = titleObject.transform,
                    localPosition = Vector3.zero,
                    localScale = transform.localScale
                },
                layer = boxData.layer
            };
            return gameObj;
        }

        /// <summary>
        /// Starts playing a new animation. Resets frame/time, refreshes collider objects, and applies boxes.
        /// </summary>
        /// <param name="nextAnimation">The animation to play.</param>
        public void Play(PixelAnimation nextAnimation) // TODO: improve queue/transition support
        {
            if (nextAnimation == null)
            {
                Debug.LogError("Play failed: animation is null.");
                return;
            }

            if (animationController == null || !animationController.Animations.Contains(nextAnimation))
            {
                Debug.LogError("Animation not found in the animation controller");
                return;
            }

            FrameIndex = 0;
            elapsedTime = 0;
            IsPlaying = true;
            isLastFrame = false;
            playingAnimation = nextAnimation;

            RefreshColliderObjects();
            SetBoxes(PlayingAnimation.BoxGroups);

            OnFrameChanged?.Invoke(0);
        }

        /// <summary>
        /// Removes collider GameObjects no longer present in <see cref="PlayingAnimation"/>.
        /// </summary>
        private void RefreshColliderObjects()
        {
            if (PlayingAnimation == null) return;

            var names = PlayingAnimation.GetBoxGroupsName(preferences);
            var keysToRemove = colliderObjects.Keys.Where(k => !names.Contains(k)).ToList();

            foreach (var key in keysToRemove)
            {
                var go = colliderObjects[key];
                colliderObjects.Remove(key);
                if (go) Destroy(go);
            }
        }

        /// <summary>
        /// Ensures collider objects/components exist for all box groups and are configured.
        /// </summary>
        private void SetBoxes(List<BoxGroup> boxGroups)
        {
            foreach (var boxGroup in boxGroups)
                SetColliderObject(boxGroup);
        }

        /// <summary>
        /// Finds or creates the collider GameObject for a group, matches collider count to layer count,
        /// and assigns component settings.
        /// </summary>
        private GameObject SetColliderObject(BoxGroup boxGroup)
        {
            var boxData = preferences.GetBoxData(boxGroup.BoxDataGuid);
            var exists = colliderObjects.TryGetValue(boxData.boxName, out var colliderObj);

            if (exists)
            {
                // Remove any stray components.
                foreach (var component in colliderObj.GetComponents<Component>())
                {
                    if (component is not Transform && component is not BoxCollider2D)
                        Destroy(component);
                }

                // Match collider count to box layer count.
                var cols = colliderObj.GetComponents<BoxCollider2D>();
                if (cols.Length < boxGroup.boxes.Count)
                {
                    for (var i = cols.Length; i < boxGroup.boxes.Count; i++)
                        colliderObj.AddComponent<BoxCollider2D>();
                }
                else if (cols.Length > boxGroup.boxes.Count)
                {
                    for (var i = boxGroup.boxes.Count; i < cols.Length; i++)
                        Destroy(cols[i]);
                }
            }
            else
            {
                colliderObj = CreateColliderObject(boxData);
                colliderObjects.Add(colliderObj.name, colliderObj);

                // Initial collider set.
                foreach (var _ in boxGroup.boxes)
                    colliderObj.AddComponent<BoxCollider2D>();
            }

            // Configure each collider for its corresponding layer.
            for (var i = 0; i < boxGroup.boxes.Count; i++)
            {
                var box = boxGroup.boxes[i];
                var col = colliderObj.GetComponents<BoxCollider2D>()[i];
                SetColliderComponents(colliderObj, box, boxGroup, col);
            }

            return colliderObj;
        }

        /// <summary>
        /// Configures a single <see cref="BoxCollider2D"/> (trigger, offset/size) and
        /// attaches info components that handle enter/stay/exit dispatch.
        /// </summary>
        private void SetColliderComponents(GameObject colliderObj, BoxLayer box, BoxGroup boxGroup, BoxCollider2D col)
        {
            var rect = GetAdjustedRect(box, FrameIndex);

            col.isTrigger = boxGroup.collisionTypes == CollisionTypes.Trigger;
            col.size = rect.size;
            col.offset = new Vector2(rect.position.x, rect.position.y);

            if (col.isTrigger)
                ColliderInfo.Create(colliderObj, this, box);
            else
                CollisionInfo.Create(colliderObj, this, box);
        }

        #endregion
    }
}
