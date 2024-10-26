using UnityEngine;
using UnityEditor;
using System;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.Editor.Preferences;

namespace binc.PixelAnimator.Editor.Windows{
    

    [Serializable]
    public class PixelAnimatorWindow : EditorWindow{

        #region Variables

        public static PixelAnimatorWindow AnimatorWindow{get; private set;}
        public static readonly Color BackgroundColor = new(0.12f,0.12f,0.12f);
        public static readonly Vector2 MinSize =  new (150,450);
        public GUISkin PixelAnimatorSkin { get; private set; }
        public int IndexOfSelectedFrame{get; private set;}
        public int IndexOfSelectedLayer{get; private set;}
        public int IndexOfSelectedGroup{get; private set;}
        public PixelAnimation SelectedAnimation{get; private set;}
        public float EditorDeltaTime{get; private set;}
        private float lifeTime;
        public PixelAnimationPreferences AnimationPreferences{get; private set;}
        public PixelAnimatorPreferences AnimatorPreferences{get; private set;}
        public SerializedObject TargetAnimation{get; private set;}
        
        private bool initialized = false;
        public WindowEnum WindowFocus{get; set;}
        public PropertyFocusEnum PropertyFocus{get; private set;}
        public Window FocusedWindow { get; private set; }
        public SerializedObject SerializedAnimator {get; private set;} 

        public Rect AvailableSpace{get; private set;}

        #endregion


        #region Initialize

        [MenuItem("Window/Pixel Animator")]
        private static void InitWindow(){ 
            AnimatorWindow = CreateInstance<PixelAnimatorWindow>();
            AnimatorWindow.minSize = MinSize;
            
            AnimatorWindow.Show();
            var icon = Resources.Load<Texture2D>("Sprites/PixelAnimatorIcon");
            AnimatorWindow.titleContent = new GUIContent("Pixel Animator", icon);
        }

        private void OnEnable(){
            AnimatorWindow = this;
            Init();
        }


        private void Init(){ 
            AnimationPreferences = Resources.Load<PixelAnimationPreferences>("Animation Preferences"); 
            AnimatorPreferences = Resources.Load<PixelAnimatorPreferences>("Animator Preferences"); 
            PixelAnimatorSkin = Resources.Load<GUISkin>("PixelAnimationSkin");
            FocusedWindow = null;
            WindowFocus = WindowEnum.None;
            initialized = true;
            SerializedAnimator = new SerializedObject(this);
            InitWindows();
        }

        private void InitWindows(){
            for (var i = 0; i < AnimatorPreferences.windows.Count; i++){
                var window = AnimatorPreferences.windows[i];
                window?.Initialize(i);
            }
        }


        #endregion

        private void OnInspectorUpdate(){
            SetWindowsData();
        }

        private void SetWindowsData(){
            foreach (var window in AnimatorPreferences.windows){
                if(window is not IUpdate update) continue;
                update.InspectorUpdate();
            }
        }


        private void OnGUI(){
            if(!initialized) Init();
            AvailableSpace = position;
            DrawBackground();
            SetEditorDeltaTime();

            FocusedWindowFunction();
            ProcessingWindows();
            SelectedObject();

        }

        private void DrawBackground(){
            var rect = new Rect(Vector2.zero, position.size);
            EditorGUI.DrawRect(rect, BackgroundColor);
        }

        private void ProcessingWindows(){
            BeginWindows();
            for (var i = 0; i < AnimatorPreferences.windows.Count; i++){
                var window = AnimatorPreferences.windows[i];
                var isValidWindow = window != null;
                if(!isValidWindow) continue; 
                window.ProcessWindow(); 
                GUI.BringWindowToBack(i);
                 
            }
            EndWindows();
        }
        private Vector2 mousePos;
        private void FocusedWindowFunction(){
            var eventCurrent = Event.current;
            if(eventCurrent.type != EventType.Used){
                mousePos = Event.current.mousePosition;
            }
            var isLeftClicked = eventCurrent.button == 0 && (eventCurrent.type == EventType.MouseDown || eventCurrent.type == EventType.Used);
            if (isLeftClicked){
                var foundFocusedWindow = IsExistFocusedWindow();
                if (!foundFocusedWindow) FocusedWindow = null;
            
            }
            FocusedWindow?.FocusFunctions();
        }

        private bool IsExistFocusedWindow(){
            foreach (var window in AnimatorPreferences.windows){
                var isInRect = window.WindowRect.Contains(mousePos);
                
                if (!isInRect) continue;
                FocusedWindow = window;
                return true;
            }
            return false;
        }
        #region Common
        private void SetEditorDeltaTime(){

            if(lifeTime == 0f) lifeTime = (float)EditorApplication.timeSinceStartup;
            EditorDeltaTime = (float)(EditorApplication.timeSinceStartup - lifeTime);
            lifeTime = (float)EditorApplication.timeSinceStartup;
        
        }

        private void SelectedObject(){
            foreach(var obj in Selection.objects) {
                if (obj is not PixelAnimation anim){
                    SelectedAnimation = null;
                    continue;
                }

                if(SelectedAnimation == anim){
                    continue;
                }
                TargetAnimation = new SerializedObject(anim);
                var spriteList = anim.GetSpriteList();
                SelectedAnimation = anim;
                
                if(spriteList != null)
                    lifeTime = 0;
            }

        }

        public static void AddCursorBool(bool condition, MouseCursor icon){
            
            if(!condition) return;
            var rect = new Rect(0,0, AnimatorWindow.position.size.x, AnimatorWindow.position.size.y);
            EditorGUIUtility.AddCursorRect(rect, icon);

        }


        public void SelectGroup(int index){
            var isValid = index < SelectedAnimation.Groups.Count && index >= 0;
            if(!isValid) throw new IndexOutOfRangeException();
            
            IndexOfSelectedGroup = index;
        }
        public void SelectLayer(int layerIndex){
            var isValid = layerIndex < SelectedAnimation.Groups[IndexOfSelectedGroup].layers.Count && layerIndex >= 0;
            if(!isValid) throw new IndexOutOfRangeException();
            
            IndexOfSelectedLayer = layerIndex;
        }
        public void SelectFrame(int index){
            var isValid = index < SelectedAnimation.GetSpriteList().Count && index >= 0;
            if(!isValid) throw new IndexOutOfRangeException();
            
            IndexOfSelectedFrame = index;
        }

        public void SetAvailableRect(Rect rect){
            AvailableSpace = rect;
        }

        public void SelectFocusWindow(Window window){
            FocusedWindow = window;
        }


        #endregion



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

    


