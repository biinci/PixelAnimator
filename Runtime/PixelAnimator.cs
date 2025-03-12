using System.Collections.Generic;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.AnimationData;
using UnityEngine;
using System;
using System.Linq;
using binc.PixelAnimator.DataManipulations;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace binc.PixelAnimator{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PixelAnimator : MonoBehaviour{
        public event Action<int> OnFrameChanged;

        private PixelAnimationPreferences preferences;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private PixelAnimationController animationController;
        public PixelAnimation PlayingAnimation { get; private set; }
        private PixelAnimation nextAnim;
        public int FrameIndex { get; private set; }
        private float elapsedTime;
        private bool isPlaying;

        private GameObject titleObject;
        
        private readonly Dictionary<string, GameObject> colliderObjects = new();

        private void OnEnable()
        {
            OnFrameChanged += _ => ApplyFrame();
        }

        private void OnDestroy()
        {
            OnFrameChanged -= _ => ApplyFrame();
        }

        private void Awake(){
            preferences = Resources.Load<PixelAnimationPreferences>("Animation Preferences");
            CreateTitle();
            CompileFunctions();
        }

        private void CompileFunctions()
        {
            foreach (var pixelAnimation in animationController.Animations)
            {
                foreach (var methodStorage in pixelAnimation.PixelSprites.Select(pixelSprite => pixelSprite.methodStorage))
                {
                    methodStorage.CompileAllFunctions(gameObject);
                }

                foreach (var boxGroup in from boxGroup in pixelAnimation.BoxGroups select boxGroup)
                { 
                    var isTrigger = boxGroup.collisionTypes == CollisionTypes.Trigger;
                    foreach (var frame in from box in boxGroup.boxes from frame in box.frames select frame)
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
                }
            }
        }
        private void CreateTitle(){
            titleObject = new GameObject("---PixelAnimator Colliders---"){ transform ={
                    parent = transform,
                    localPosition = Vector3.zero
                }
            };
        }
        
        private void Update(){
            NextFrame();
        }
        
        private void NextFrame(){
            if (!isPlaying) return;
            elapsedTime += Time.deltaTime;
            var secondsPerFrame = 1 / (float)PlayingAnimation.fps;
            var spriteCount = PlayingAnimation.PixelSprites.Count;
            
            while (elapsedTime >= secondsPerFrame){
                
                FrameIndex = (FrameIndex + 1) % spriteCount;
                OnFrameChanged?.Invoke(FrameIndex);
                elapsedTime -= secondsPerFrame;
                if (FrameIndex != spriteCount - 1 || PlayingAnimation.loop)
                    continue;
                isPlaying = false;
                break;
            }
        }

        
        private void ApplyFrame()
        {
            spriteRenderer.sprite = PlayingAnimation.PixelSprites[FrameIndex].sprite;
            CallEvent();
            SetBoxSize();
        }

        private void CallEvent()
        {
            var unityEvent = PlayingAnimation.PixelSprites[FrameIndex].methodStorage.methods;
            unityEvent?.Invoke();
        }
        
        private void SetBoxSize(){
            try{
                var groups = PlayingAnimation.BoxGroups;
                for (var g = 0; g < groups.Count; g++){
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
        private Rect GetAdjustedRect(BoxLayer box, int index){
            var f = index == -1 ? 0 : index;
            return PixelAnimatorUtility.MapBoxRectToTransform(box.frames[f].boxRect, PlayingAnimation.GetSpriteList()[f]);
        }

        private GameObject CreateColliderObject(BoxData boxData){
            var gameObj = new GameObject(boxData.boxName){
                transform ={
                    parent = titleObject.transform,
                    localPosition = Vector3.zero,
                    localScale = transform.localScale
                },
                layer =  boxData.layer
            };
            return gameObj;
        }
        
        public void Play(PixelAnimation nextAnimation){//TODO: need to fix
            FrameIndex = 0;
            elapsedTime = 0;
            isPlaying = true;
            PlayingAnimation = nextAnimation;
            RefreshColliderObjects();
            SetBoxes(PlayingAnimation.BoxGroups);
            OnFrameChanged.Invoke(0);
        }
        
        private void RefreshColliderObjects(){
            var names = PlayingAnimation.GetBoxGroupsName(preferences);
            for (var g = 0; g < colliderObjects.Count; g++){
                if (names.Contains(colliderObjects.ElementAt(g).Key)) continue;

                var gameObj = titleObject.transform.GetChild(g).gameObject;
                colliderObjects.Remove(gameObj.name);
                Destroy(gameObj);
            }
        }
        
        //When the current animation changes, the collider info component and the box collider component are added. The size and position of the box collider are adjusted.
        private void SetBoxes(List<BoxGroup> boxGroups){
            foreach (var boxGroup in boxGroups) {
                var colliderObj = SetColliderObject(boxGroup);
                foreach (var box in boxGroup.boxes)
                {
                    SetCollider(colliderObj, box, boxGroup);
                }
            }
        }
        
        //When the current animation changes, the objects of the groups are checked. The collider object is refreshed depending on the situation.
        private GameObject SetColliderObject(BoxGroup boxGroup){
            GameObject colliderObj;
            var boxData = preferences.GetBoxData(boxGroup.BoxDataGuid);
            var isExist = colliderObjects.Keys.Contains(boxData.boxName);
            if (!isExist){
                colliderObj = CreateColliderObject(boxData);
                colliderObjects.Add(colliderObj.name, colliderObj);
            }
            else{
                colliderObj = colliderObjects[boxData.boxName].gameObject;
            }

            foreach (var component in colliderObj.GetComponents<Component>()){
                if(component is not Transform) Destroy(component);
            }

            return colliderObj;
        }
        
        private void SetCollider(GameObject colliderObj, BoxLayer box, BoxGroup boxGroup){
            var col = colliderObj.AddComponent<BoxCollider2D>();
            col.enabled = false;
            var rect = GetAdjustedRect(box, FrameIndex);
            col.isTrigger = boxGroup.collisionTypes == CollisionTypes.Trigger;
            col.size = rect.size;
            col.offset = new Vector2(rect.position.x, rect.position.y);
            col.enabled = true;
            if (col.isTrigger)
            {
                ColliderInfo.Create(colliderObj, this, box);
            }
            else
            {
                CollisionInfo.Create(colliderObj, this, box);
            }
        }


    }
}