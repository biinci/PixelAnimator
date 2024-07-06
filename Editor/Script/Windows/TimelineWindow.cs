
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Utility;
using binc.PixelAnimator.Common;

namespace binc.PixelAnimator.Editor.Windows{

    [System.Serializable]
    public class TimelineWindow : Window{
        
        #region Variables
        private bool clickedHandle, isPlaying;
        public bool TimelineDragging => clickedHandle;


        private Rect handleRect,
           burgerRect,
           backRect,
           playRect,
           frontRect,
           columnLayout,
           rowLayout,
           framePlaneRect,
           spriteThumbnailRect;

        private Texture2D backTex,
            playTex,
            frontTex,
            timelineBurgerTex,
            onMouseTimelineBurgerTex,
            stopTex,
            durationTex,
            closeTex,
            openTex,
            settingsTex,
            unvisibleTex,
            visibleTex,
            layerBurgerMenu,
            triggerTex,
            noTriggerTex;

        private Texture2D[] pingTexs = new Texture2D[4];

        public static Color framePlaneColour = new (0.16f, 0.17f, 0.19f);
        public static Color timelinePlaneColour = new (0.1f, 0.1f, 0.1f, 1);

        private GenericMenu timelinePopup, groupPopup, layerPopup;

        private Rect[] columnRects;
        private readonly Vector2 frameBlank = new (48, 48);
        private const float HandleHeight = 9;
        private const float GroupPanelWidth = 255;
        private GUIStyle groupStyle, layerStyle, keyFrameStyle, emptyFrameStyle, copyFrameStyle, frameButtonStyle, spriteThumbnailStyle;
        private float timer;
        private Vector2 scrollPos;
        #endregion

        #region Init
        public TimelineWindow() {
            var position = PixelAnimatorWindow.AnimatorWindow.position;
            windowRect.x = 0;
            windowRect.y = position.yMax - windowRect.height;
            LoadInitResources();
            InitRect();
        }

        
        private void LoadInitResources(){
            backTex = Resources.Load<Texture2D>("Sprites/Back");
            frontTex = Resources.Load<Texture2D>("Sprites/Front");
            timelineBurgerTex = Resources.Load<Texture2D>("Sprites/TimelineBurgerMenu");
            onMouseTimelineBurgerTex = Resources.Load<Texture2D>("Sprites/TimelineBurgerMenu2");
            stopTex = Resources.Load<Texture2D>("Sprites/Stop");
            playTex = Resources.Load<Texture2D>("Sprites/Play");
            closeTex = Resources.Load<Texture2D>("Sprites/up");
            openTex = Resources.Load<Texture2D>("Sprites/down");
            settingsTex = Resources.Load<Texture2D>("Sprites/settings");
            layerBurgerMenu = Resources.Load<Texture2D>("Sprites/LayerBurgerMenu");
            visibleTex = Resources.Load<Texture2D>("Sprites/visible");
            unvisibleTex = Resources.Load<Texture2D>("Sprites/unvisible");
            triggerTex = Resources.Load<Texture2D>("Sprites/trigger");
            noTriggerTex = Resources.Load<Texture2D>("Sprites/notrigger");
            pingTexs[0] = Resources.Load<Texture2D>("Sprites/Ping T-L");
            pingTexs[1] = Resources.Load<Texture2D>("Sprites/Ping T-R");
            pingTexs[2] = Resources.Load<Texture2D>("Sprites/Ping B-L");
            pingTexs[3] = Resources.Load<Texture2D>("Sprites/Ping B-R");


            var mySkin = PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin;
            groupStyle = new GUIStyle(mySkin.GetStyle("Group"));
            layerStyle = new GUIStyle(mySkin.GetStyle("Layer"));
            keyFrameStyle = new GUIStyle(mySkin.GetStyle("KeyFrame"));
            emptyFrameStyle = new GUIStyle(mySkin.GetStyle("EmptyFrame"));
            copyFrameStyle = new GUIStyle(mySkin.GetStyle("CopyFrame"));
            frameButtonStyle = new GUIStyle(mySkin.GetStyle("FrameButton"));
            spriteThumbnailStyle = new GUIStyle(mySkin.GetStyle("SpriteThumbnail"));
            
            durationTex = playTex;
        }

        private void InitRect() {
            framePlaneRect = new Rect(columnLayout.xMax, rowLayout.yMax, windowRect.width - columnLayout.xMax, windowRect.height);
            const float buttonSize = 24; // set button rect
            burgerRect = new Rect(10, 15, 32, 32);
            backRect = new Rect(160, 20, buttonSize, buttonSize);
            playRect = new Rect(backRect.width + backRect.xMin + 2, backRect.yMin, buttonSize, buttonSize);
            frontRect = new Rect(playRect.width + playRect.xMin + 2, backRect.yMin, buttonSize, buttonSize);

        }
        #endregion

        public override void ProcessWindow(Event eventCurrent){
            DrawWindow();

        }



        private void DrawWindow(){
            var animatorRect = PixelAnimatorWindow.AnimatorWindow.position;
            windowRect.size = animatorRect.size;
            
            windowRect = GUI.Window(1, windowRect, _=>WindowFunction(), GUIContent.none, GUIStyle.none);
        }
        private void WindowFunction(){

            


        }



        // private void WindowFunction(){
        //     columnLayout = new Rect(GroupPanelWidth, HandleHeight, 3, windowRect.height);
        //     rowLayout = new Rect(0, frameBlank.y+HandleHeight, windowRect.width, 4);

        //     var timelinePlaneRect = new Rect(0, HandleHeight, windowRect.width, windowRect.height);
        //     EditorGUI.DrawRect(timelinePlaneRect, timelinePlaneColour);

        //     framePlaneRect = new Rect(columnLayout.xMax,rowLayout.yMax, windowRect.width - columnLayout.xMax, windowRect.height - rowLayout.yMax);
        //     EditorGUI.DrawRect(framePlaneRect, framePlaneColour);

        //     spriteThumbnailRect = new Rect(columnLayout.xMax, handleRect.height, windowRect.width - columnLayout.xMax, rowLayout.yMin-handleRect.height);


        //     EditorGUI.DrawRect(rowLayout, Color.black);
        //     EditorGUI.DrawRect(columnLayout, Color.black);

        //     if (PixelAnimatorWindow.AnimatorWindow.TargetAnimation == null) return;

        // }


        public override void FocusFunctions(){

            
        }



















        private void Play(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            if (isPlaying) {
                var fps = SelectedAnim.fps;
                if(fps == 0) Debug.Log("Frame rate is zero");
                var deltaTime = animatorWindow.editorDeltaTime;
                timer += deltaTime;
                if(timer >= 1f/fps){
                    timer -= 1f/fps;
                    var mod = (  animatorWindow.ActiveFrameIndex +1 ) % SelectedAnim.GetSpriteList().Count;
                    // animatorWindow.SetActiveFrame(mod);
                    animatorWindow.Repaint();
                }
            }else if(!isPlaying && timer != 0){
                timer = 0;
            }
        }


    }
}