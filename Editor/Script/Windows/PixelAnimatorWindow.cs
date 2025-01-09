using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using binc.PixelAnimator.Common;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.Editor.Preferences;

namespace binc.PixelAnimator.Editor.Windows{


    [Serializable]
    public class PixelAnimatorWindow : EditorWindow
    {

        #region Variables

        public static PixelAnimatorWindow AnimatorWindow { get; private set; }
        public static readonly Color BackgroundColor = new(0.098f, 0.098f, 0.098f);
        public static readonly Vector2 MinSize = new(150, 450);
        public GUISkin PixelAnimatorSkin { get; private set; }
        public int IndexOfSelectedSprite { get; private set; }
        public int IndexOfSelectedLayer { get; private set; }
        public int IndexOfSelectedGroup { get; private set; }
        [SerializeField] private PixelAnimation selectedAnimation;
        public PixelAnimation SelectedAnimation => selectedAnimation;

        public float EditorDeltaTime { get; private set; }
        private float lifeTime;
        public PixelAnimationPreferences AnimationPreferences { get; private set; }
        public PixelAnimatorPreferences AnimatorPreferences { get; private set; }
        public SerializedObject TargetAnimation { get; private set; }
        public Window FocusedWindow { get; private set; }
        public SerializedObject SerializedAnimator { get; private set; }

        public Rect AvailableSpace { get; private set; }
        public bool FocusChangeable { get; private set; }

        #endregion



        #region Initialize

        [MenuItem("Window/Pixel Animator")]
        private static void InitWindow()
        {
            AnimatorWindow = CreateInstance<PixelAnimatorWindow>();
            AnimatorWindow.minSize = MinSize;

            AnimatorWindow.Show();
            var icon = Resources.Load<Texture2D>("Sprites/PixelAnimatorIcon");
            AnimatorWindow.titleContent = new GUIContent("Pixel Animator", icon);
        }

        private void OnEnable()
        {
            AnimatorWindow = this;
            Init();
        }

        private void OnDisable()
        {
            DisposeWindows();
        }

        private void DisposeWindows()
        {
            foreach (var window in AnimatorPreferences.windows)
            {
                window?.Dispose();
            }
        }

        private void Init()
        {

            AnimationPreferences = Resources.Load<PixelAnimationPreferences>("Animation Preferences");
            AnimatorPreferences = Resources.Load<PixelAnimatorPreferences>("Animator Preferences");
            PixelAnimatorSkin = Resources.Load<GUISkin>("PixelAnimationSkin");
            FocusedWindow = null;
            SerializedAnimator = new SerializedObject(this);
            SelectedObject();
            InitWindows();


        }

        private void InitWindows()
        {
            for (var i = 0; i < AnimatorPreferences.windows.Count; i++)
            {
                var window = AnimatorPreferences.windows[i];
                window?.Initialize(i);
            }
        }


        #endregion



        private void SetWindowsData()
        {
            foreach (var window in AnimatorPreferences.windows)
            {
                if (window is not IUpdate update) continue;
                update.InspectorUpdate();
            }
        }


        private void OnGUI()
        {


            // AvailableSpace = position;
            DrawBackground();
            SetEditorDeltaTime();
            SelectedObject();

            FocusedWindowFunction();
            ProcessingWindows();


            if (Event.current.type != EventType.Used && Event.current.isMouse)
            {
                GUI.FocusControl("");
            }

            // Repaint();
        }

        private void DrawBackground()
        {
            var rect = new Rect(Vector2.zero, position.size);
            EditorGUI.DrawRect(rect, BackgroundColor);
        }

        public Color color;
        private void ProcessingWindows()
        {
            BeginWindows();
            try
            {

                for (var i = 0; i < AnimatorPreferences.windows.Count; i++)
                {

                    var window = AnimatorPreferences.windows[i];
                    var isValidWindow = window != null;
                    if (!isValidWindow) continue;
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

        private Vector2 mousePos;

        private void FocusedWindowFunction()
        {
            var eventCurrent = Event.current;
            if (eventCurrent.type != EventType.Used)
            {
                mousePos = Event.current.mousePosition;
            }

            var isLeftClicked = eventCurrent.button == 0 && eventCurrent.type is EventType.MouseDown or EventType.Used;
            if (isLeftClicked)
            {
                var foundFocusedWindow = IsExistFocusedWindow();
                if (!foundFocusedWindow) FocusedWindow = null;

            }

            FocusedWindow?.FocusFunctions();
        }

        private bool IsExistFocusedWindow()
        {
            foreach (var window in from window in AnimatorPreferences.windows
                     let isInRect = window.WindowRect.Contains(mousePos)
                     where isInRect
                     select window)
            {
                FocusedWindow = window;
                FocusChangeable = false;
                return true;
            }

            FocusChangeable = true;
            return false;
        }

        #region Common

        private void SetEditorDeltaTime()
        {

            if (lifeTime == 0f) lifeTime = (float)EditorApplication.timeSinceStartup;
            EditorDeltaTime = (float)(EditorApplication.timeSinceStartup - lifeTime);
            lifeTime = (float)EditorApplication.timeSinceStartup;

        }

        private void SelectedObject()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj is not PixelAnimation anim)
                {
                    selectedAnimation = null;
                    TargetAnimation = null;
                    continue;
                }

                TargetAnimation = new SerializedObject(anim);
                TargetAnimation.Update();
                if (SelectedAnimation == anim)
                {
                    continue;
                }

                var spriteList = anim.GetSpriteList();
                selectedAnimation = anim;
                IndexOfSelectedGroup = 0;
                IndexOfSelectedLayer = 0;
                IndexOfSelectedSprite = 0;

                if (spriteList != null)
                    lifeTime = 0;
            }

        }

        public static void AddCursorBool(bool condition, MouseCursor icon)
        {

            if (!condition) return;
            var rect = new Rect(0, 0, AnimatorWindow.position.size.x, AnimatorWindow.position.size.y);
            EditorGUIUtility.AddCursorRect(rect, icon);

        }


        public void SelectGroup(int index)
        {
            var isValid = index < SelectedAnimation.BoxGroups.Count && index >= 0;
            if (!isValid) throw new IndexOutOfRangeException();

            IndexOfSelectedGroup = index;
        }

        public void SelectLayer(int layerIndex)
        {
            var isValid = layerIndex < SelectedAnimation.BoxGroups[IndexOfSelectedGroup].boxes.Count && layerIndex >= 0;
            if (!isValid) throw new IndexOutOfRangeException();

            IndexOfSelectedLayer = layerIndex;
        }

        public void SelectSprite(int index)
        {
            var isValid = index < SelectedAnimation.GetSpriteList().Count && index >= 0;
            if (!isValid) throw new IndexOutOfRangeException();

            IndexOfSelectedSprite = index;
        }

        public void SetAvailableRect(Rect rect)
        {
            AvailableSpace = rect;
        }

        public void SelectFocusWindow(Window window)
        {
            if (FocusChangeable)
                FocusedWindow = window;
        }


        #endregion

        public T GetWindow<T>() where T : Window
        {
            return AnimatorPreferences.windows.Find(w => w.GetType() == typeof(T)) as T;
        }

        public bool IsValidAnimation()
        {
            return SelectedAnimation;
        }
        
        public bool IsValidGroup()
        {
            if (!IsValidAnimation()) return false;
            if(SelectedAnimation.BoxGroups == null) return false;
            return IndexOfSelectedGroup < SelectedAnimation.BoxGroups.Count;
        }

        public bool IsValidLayer()
        {
            if (IsValidGroup())
            {
                return IndexOfSelectedLayer < SelectedAnimation.BoxGroups[IndexOfSelectedGroup].boxes.Count;
            }

            return false;
        }

        public bool IsValidFrame()
        {
            if (IsValidGroup() && IsValidLayer())
            {
                return IndexOfSelectedSprite < SelectedAnimation.GetSpriteList().Count;
            }

            return false;

        }

        public bool IsValidSprite()
        {
            if (!IsValidAnimation()) return false;
            return IndexOfSelectedSprite < SelectedAnimation.GetSpriteList().Count;
        }
        

        public bool IsSelectedFrame(BoxFrame boxFrame)
        {
            return selectedAnimation.BoxGroups[IndexOfSelectedGroup].boxes[IndexOfSelectedLayer]
                .frames[IndexOfSelectedSprite] == boxFrame;
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

    


