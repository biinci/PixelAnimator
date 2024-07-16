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

        private const string PixelAnimatorPath = "Assets/binc/PixelAnimator/";
        private static readonly Color BACKGROUND_COLOR = new(0.13f, 0.13f, 0.15f);
        public static readonly Vector2 MIN_SIZE =  new Vector2(150,450);
        public GUISkin PixelAnimatorSkin { get; private set; }
        public int ActiveFrameIndex{get; private set;}
        public int ActiveGroupIndex{get; private set;} 
        public int ActiveLayerIndex{get; private set;}
        public PixelAnimation SelectedAnimation{get; private set;}
        public float editorDeltaTime{get; private set;}
        private float lifeTime;
        public PixelAnimationPreferences AnimationPreferences{get; private set;}
        public PixelAnimatorPreferences AnimatorPreferences{get; private set;}
        public SerializedObject TargetAnimation{get; private set;}
        
        private bool initialized = false;
        public Event EventCurrent => Event.current;
        public WindowEnum WindowFocus{get; set;}
        public PropertyFocusEnum PropertyFocus{get; private set;}
        public Window FocusedWindow { get; private set; }
        public SerializedObject SerializedAnimator {get; private set;} 


        #endregion


        #region Initialize

        [MenuItem("Window/Pixel Animator")]
        private static void InitWindow(){ 
            AnimatorWindow = CreateInstance<PixelAnimatorWindow>();
            AnimatorWindow.minSize = MIN_SIZE;
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
            WindowFocus = WindowEnum.none;
            initialized = true;
            SerializedAnimator = new SerializedObject(this);
            InitWindows();
        }

        private void InitWindows(){
            foreach(var item in AnimatorPreferences.windows){
                item.Initialize();
            }
        }


        #endregion

        private void OnInspectorUpdate(){
            SetWindowsData();
        }

        private void SetWindowsData(){
            foreach (var window in AnimatorPreferences.windows){
                if(window is not IUpdate update) return;
                update.InspectorUpdate();
            }
        }

        private void OnGUI(){
            if(!initialized) Init();
            SetEditorDeltaTime();
            ProcessingWindows();
            FocusedWindowFunction();
            SelectedObject();
        }

        private void ProcessingWindows(){
            BeginWindows();
            foreach (var window in AnimatorPreferences.windows){
                var isValidWindow = window != null && window.GetType() != typeof(Window);
                if(isValidWindow) window.ProcessWindow(EventCurrent); 
                 
            }
            EndWindows();
        }
        private void FocusedWindowFunction(){
            var isLeftClicked = EventCurrent.type == EventType.MouseDown && EventCurrent.button == 0;

            if (isLeftClicked){
                var foundFocusedWindow = false;

                foreach (var window in AnimatorPreferences.windows){
                    var isInRect = window.WindowRect.Contains(EventCurrent.mousePosition);
                    var isValid = FocusedWindow == null || FocusedWindow.FocusChangeable;
                    if (!isInRect || !isValid) continue;
                    FocusedWindow = window;
                    foundFocusedWindow = true;
                    break; 
                }
                if (!foundFocusedWindow) FocusedWindow = null;
            
            }
            
            FocusedWindow?.FocusFunctions();
        }

        
        #region Common
        private void SetEditorDeltaTime(){

            if(lifeTime == 0f) lifeTime = (float)EditorApplication.timeSinceStartup;
            editorDeltaTime = (float)(EditorApplication.timeSinceStartup - lifeTime);
            lifeTime = (float)EditorApplication.timeSinceStartup;
        
        }

        private void SelectedObject(){
            foreach(var obj in Selection.objects) {
                if (obj is not PixelAnimation anim){
                    SelectedAnimation = null;
                    continue;
                } 
                else if(SelectedAnimation == anim){
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

        #endregion



    }

}

    


