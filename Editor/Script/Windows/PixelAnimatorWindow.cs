using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using binc.PixelAnimator.DataProvider;
using binc.PixelAnimator.Common;
using binc.PixelAnimator.Preferences;
using static binc.PixelAnimator.Utility.PixelAnimatorUtility;
using UnityEditor.Search;
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
        public PixelAnimatorPreferences AnimatorPeferences{get; private set;}
        public SerializedObject TargetAnimation{get; private set;}
        
        [SerializeField] private bool initialized;
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
        private PixelAnimatorWindow() { }

        private void OnEnable(){
            AnimatorWindow = this;
            initialized = false;
            SelectedObject();
            Init();
        }


        private void Init(){ 
            AnimationPreferences = Resources.Load<PixelAnimationPreferences>("Animation Preferences"); 
            AnimatorPeferences = Resources.Load<PixelAnimatorPreferences>("Animator Preferences"); 
            PixelAnimatorSkin = Resources.Load<GUISkin>("PixelAnimationSkin");
            FocusedWindow = null;
            WindowFocus = WindowEnum.none;
            initialized = true;
            SerializedAnimator = new SerializedObject(this);
        }


        #endregion




        private void OnGUI(){
            if(!initialized) Init();
            SetEditorDeltaTime();
            BeginWindows();
            foreach (var window in AnimatorPeferences.windows){
                var isValidWindow = window != null && window.GetType() != typeof(Window);
                if(isValidWindow){
                    window.SetWindow(EventCurrent); 
                } 
            }
            EndWindows();
        }





        #region Popup Functions
        // public void AddGroup(object userData)
        // {
            
        //     TargetAnimation.Update();
        //     var boxData = (BoxData)userData;

        //     var isGroupExist = SelectedAnimation.Groups.Any(x => x.BoxDataGuid == boxData.Guid);
        //     if (isGroupExist){
        //         Debug.LogError("This group has already been added! Please add another group.");
        //         return;
        //     }

        //     var groups = TargetAnimation.FindProperty("groups");

        //     PixelAnimationEditor.AddGroup(groups, boxData.Guid);
        //     AddLayer(SelectedAnimation.Groups.Count-1);

        //     GroupAdded?.Invoke(userData);
        // }

        // /// <summary>
        // /// This function allows you to legally delete group.
        // /// </summary>
        // /// <param name="groupIndex"> </param>
        // /// 
        // public void OnRemoveGroup(object groupIndex) {
        //     var index = (int)groupIndex;
        //     TargetAnimation.Update();
        //     var groups = TargetAnimation.FindProperty("groups");
        //     TargetAnimation.ApplyModifiedProperties();  
            
        //     PixelAnimationEditor.RemoveGroup(groups, (int)groupIndex);
            
        //     GroupRemoved?.Invoke(groupIndex);
        // }


        // /// <summary>
        // /// This function allows you to legally delete layer.
        // /// </summary>
        // /// <param name="array">array[0] = groupIndex and array[1] = layerIndex</param>
        // /// 
        // public void OnRemoveLayer(object array) {
        //     var indexs = array as int[];
        //     var group = TargetAnimation.FindProperty("groups");
        //     var layers = group.GetArrayElementAtIndex(indexs[0]).FindPropertyRelative("layers");
        //     PixelAnimationEditor.RemoveLayer(layers, indexs[1]);
        //     LayerRemoved?.Invoke(array);
        // }


        // public void AddLayer(object groupIndex) {
        //     var groupsProp = TargetAnimation.FindProperty("groups");
        //     var layersProp = groupsProp.GetArrayElementAtIndex((int)groupIndex).FindPropertyRelative("layers");
        //     PixelAnimationEditor.AddLayer(layersProp, TargetAnimation);

        //     LayerAdded?.Invoke((int)groupIndex);
        // }

        #endregion
        

        #region Common
        private void SetEditorDeltaTime(){

            if(lifeTime == 0f) lifeTime = (float)EditorApplication.timeSinceStartup;
            editorDeltaTime = (float)(EditorApplication.timeSinceStartup - lifeTime);
            lifeTime = (float)EditorApplication.timeSinceStartup;
        
        }

        private void SelectedObject(){
            foreach(var obj in Selection.objects) {
                if (obj is not PixelAnimation anim) continue;
                TargetAnimation = new SerializedObject(anim);

                if (SelectedAnimation == anim) continue;
                var spriteList = anim.GetSpriteList();
                if(spriteList != null)
                // TimelineWindow?.ReloadVariable(spriteList.Count);

                SelectedAnimation = anim;
                lifeTime = 0;
                ActiveFrameIndex = 0;
                ActiveGroupIndex = 0; 
                ActiveLayerIndex = 0;

            }

        }
        #endregion



    }

}

    


