using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using binc.PixelAnimator.AnimationData;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.Editor.Preferences;
using Codice.CM.Client.Differences.Graphic;

namespace binc.PixelAnimator.Editor.Windows{
    [Serializable]
    public class PixelAnimatorWindow : EditorWindow
    {
        #region Singleton
        public static PixelAnimatorWindow AnimatorWindow { get; private set; }
        #endregion
        
        #region Variables
        public static readonly Color BackgroundColor = new(0.1f, 0.1f, 0.1f);
        public static readonly Vector2 MinSize = new(800, 450);
        public GUISkin PixelAnimatorSkin { get; private set; }
        public int IndexOfSelectedSprite { get; private set; }
        public int IndexOfSelectedBox { get; private set; }
        public int IndexOfSelectedBoxGroup { get; private set; }
        public PixelAnimation SelectedAnimation => selectedAnimation;
        [SerializeField] private PixelAnimation selectedAnimation;
        public PixelAnimationPreferences AnimationPreferences { get; private set; }
        public PixelAnimatorPreferences AnimatorPreferences { get; private set; }
        public SerializedObject SerializedSelectedAnimation { get; private set; }
        public Window FocusedWindow { get; private set; }
        public bool FocusChangeable { get; private set; }
        public float EditorDeltaTime { get; private set; }
        private float lifeTime;
        #endregion
        
        #region Initialize
        [MenuItem("Window/Pixel Animator")]
        private static void InitWindow()
        {
            AnimatorWindow = CreateInstance<PixelAnimatorWindow>();
            AnimatorWindow.minSize = MinSize;
            var icon = Resources.Load<Texture2D>("Sprites/PixelAnimatorIcon");
            AnimatorWindow.titleContent = new GUIContent("Pixel Animator", icon);
            AnimatorWindow.Show();
            
        }

        private void OnEnable()
        {
            OnSelectionChange();
            wantsMouseMove = true;
            LoadResources();
            InitWindows();
        }

        private void OnDisable()
        {
            DisposeWindows();
        }
        
        private void LoadResources()
        {
            AnimationPreferences = Resources.Load<PixelAnimationPreferences>("Animation Preferences");
            AnimatorPreferences = Resources.Load<PixelAnimatorPreferences>("Animator Preferences");
            PixelAnimatorSkin = Resources.Load<GUISkin>("PixelAnimationSkin");
        }

        private void InitWindows()
        {
            AnimatorWindow = this;
            FocusedWindow = null;
            for (var i = 0; i < AnimatorPreferences.windows.Count; i++)
            {
                var window = AnimatorPreferences.windows[i];
                window?.Initialize(i);
            }
        }
        
        private void DisposeWindows()
        {
            foreach (var window in AnimatorPreferences.windows)
            {
                window?.Dispose();
            }
        }
        #endregion
        
        private void OnSelectionChange(){
            foreach (var obj in Selection.objects)
            {
                if (obj is not PixelAnimation anim) continue;
                // if (SelectedAnimation == anim) continue;

                SerializedSelectedAnimation = new SerializedObject(anim);
                selectedAnimation = anim;
                IndexOfSelectedBoxGroup = IndexOfSelectedBox = IndexOfSelectedSprite = 0;
                var spriteList = anim.GetSpriteList();
                if (spriteList != null)
                    lifeTime = 0;
            }
            Repaint();
        }
        private void OnGUI()
        {
            DrawBackground();
            
            ProcessWindows();
            
            SetEditorDeltaTime();
            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }

        private void DrawBackground()
        {
            var rect = new Rect(Vector2.zero, position.size);
            EditorGUI.DrawRect(rect, BackgroundColor);
        }
        
        private void ProcessWindows()
        {
            BeginWindows();
            try
            {
                for (var i = 0; i < AnimatorPreferences.windows.Count; i++)
                {
                    var window = AnimatorPreferences.windows[i];
                    if (window == null) continue;
                    window.ProcessWindow();
                    GUI.BringWindowToBack(i);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            EndWindows();
        }
        
        private void SetEditorDeltaTime()
        {
            if (lifeTime == 0f) lifeTime = (float)EditorApplication.timeSinceStartup;
            EditorDeltaTime = (float)(EditorApplication.timeSinceStartup - lifeTime);
            lifeTime = (float)EditorApplication.timeSinceStartup;
        }
        
        #region Selection Methods
        public void SelectBoxGroup(int index)
        {
            var isValid = index >= 0 && index < SelectedAnimation.BoxGroups.Count;
            if (!isValid) throw new IndexOutOfRangeException();

            IndexOfSelectedBoxGroup = index;
        }

        public void SelectBox(int boxIndex)
        {
            var isValid = boxIndex >= 0 && boxIndex < SelectedAnimation.BoxGroups[IndexOfSelectedBoxGroup].boxes.Count;
            if (!isValid) throw new IndexOutOfRangeException();

            IndexOfSelectedBox = boxIndex;
        }

        public void SelectSprite(int index)
        {
            var isValid = index >= 0 && index < SelectedAnimation.GetSpriteList().Count;
            if (!isValid) throw new IndexOutOfRangeException();

            IndexOfSelectedSprite = index;
        }
        
        public void SelectFocusWindow(Window window)
        {
            if (FocusChangeable)
                FocusedWindow = window;
        }
        
        #endregion
        public new T GetWindow<T>() where T : Window
        {
            return AnimatorPreferences.windows.Find(w => w.GetType() == typeof(T)) as T;
        }

        public bool IsValidAnimation()
        {
            return SelectedAnimation;
        }
        
        public bool IsValidBoxGroup()
        {
            if (!IsValidAnimation()) return false;
            if(SelectedAnimation.BoxGroups == null) return false;
            return IndexOfSelectedBoxGroup < SelectedAnimation.BoxGroups.Count;
        }

        public bool IsValidBox()
        {
            if (!IsValidBoxGroup()) return false;
            return IndexOfSelectedBox < SelectedAnimation.BoxGroups[IndexOfSelectedBoxGroup].boxes.Count;
        }

        public bool IsValidFrame()
        {
            if (!IsValidBox()) return false;
            return IndexOfSelectedSprite < SelectedAnimation.GetSpriteList().Count;
        }

        public bool IsValidSprite()
        {
            if (!IsValidAnimation()) return false;
            return IndexOfSelectedSprite < SelectedAnimation.GetSpriteList().Count;
        }
        
        public bool IsFrameSelected(BoxFrame boxFrame)
        {
            return selectedAnimation.BoxGroups[IndexOfSelectedBoxGroup].boxes[IndexOfSelectedBox]
                .frames[IndexOfSelectedSprite] == boxFrame;
        }

        public int GetSpriteCount()
        {
            var count = 0;
            if (selectedAnimation && selectedAnimation.PixelSprites != null)
            {
                count = selectedAnimation.PixelSprites.Count;
            }

            return count;
        }
        
        public static void AddCursorCondition(bool condition, MouseCursor icon)
        {
            if (!condition) return;
            var rect = new Rect(Vector2.zero, AnimatorWindow.position.size);
            EditorGUIUtility.AddCursorRect(rect, icon);
        }
    }

    public struct ButtonData<T>
    {
        public Action<T> DownClick;
        public Action<T> UpClick;
    }

    public struct ButtonData
    {
        public Action DownClick;
        public Action UpClick;
    }
}