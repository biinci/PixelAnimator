
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Utility;
using binc.PixelAnimator.Common;
using UnityEngine.UIElements;
using System.ComponentModel;
using UnityEngine.TerrainUtils;

namespace binc.PixelAnimator.Editor.Windows{

    [System.Serializable]
    public class TimelineWindow : Window, IUpdate{
        
        #region Variables
        private bool clickedHandle, isPlaying;
        public bool TimelineDragging => clickedHandle;


        private Rect handleRect,
           burgerRect,
           backRect,
           playRect,
           frontRect,
           columnRect,
           rowRect,
           framePlaneRect,
           groupPlaneRect,
           thumbnailPlaneRect;

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

        public static Color FRAME_PLANE_COLOR = new (0.16f, 0.17f, 0.19f);
        public static readonly Color WINDOW_PLANE_COLOR = new (0.1f, 0.1f, 0.1f, 1);

        private GenericMenu timelinePopup, groupPopup, layerPopup;

        private readonly Vector2 frameBlank = new (48, 48);
        private const float HANDLE_HEIGHT = 6;
        private float GROUP_PANEL_WIDTH => PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.GetStyle("Group").fixedWidth;
        private float TOOL_PANEL_HEIGHT => PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.GetStyle("Tool").fixedWidth;

        private float COLUMN_WIDTH => PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.GetStyle("TimelineLayout").fixedWidth;
        private float ROW_HEIGHT => PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.GetStyle("TimelineLayout").fixedHeight;


        private GUIStyle groupStyle, layerStyle, keyFrameStyle, emptyFrameStyle, copyFrameStyle, frameButtonStyle, spriteThumbnailStyle;
        private float timer;
        private Vector2 scrollPos;
        private bool reSizing = false;
        #endregion

        #region Init

        PixelAnimation anim;

        public override void Initialize(){
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
            LoadStyles();
            
            durationTex = playTex;
        }


        private void LoadStyles(){
            var mySkin = PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin;
            groupStyle = new GUIStyle(mySkin.GetStyle("Group"));
            layerStyle = new GUIStyle(mySkin.GetStyle("Layer"));
            keyFrameStyle = new GUIStyle(mySkin.GetStyle("KeyFrame"));
            emptyFrameStyle = new GUIStyle(mySkin.GetStyle("EmptyFrame"));
            copyFrameStyle = new GUIStyle(mySkin.GetStyle("CopyFrame"));
            frameButtonStyle = new GUIStyle(mySkin.GetStyle("FrameButton"));
            spriteThumbnailStyle = new GUIStyle(mySkin.GetStyle("SpriteThumbnail"));
        }
        private void InitRect() {
            var position = PixelAnimatorWindow.AnimatorWindow.position;
            windowRect.x = 0;
            windowRect.y = position.yMax - windowRect.height;
            framePlaneRect = new Rect(columnRect.xMax, rowRect.yMax, windowRect.width - columnRect.xMax, windowRect.height);
            const float buttonSize = 24; // set button rect
            burgerRect = new Rect(10, 15, 32, 32);
            backRect = new Rect(160, 20, buttonSize, buttonSize);
            playRect = new Rect(backRect.width + backRect.xMin + 2, backRect.yMin, buttonSize, buttonSize);
            frontRect = new Rect(playRect.width + playRect.xMin + 2, backRect.yMin, buttonSize, buttonSize);

        }
        #endregion


        public void InspectorUpdate(){
            anim = PixelAnimatorWindow.AnimatorWindow.SelectedAnimation;
        }


        public override void ProcessWindow(Event eventCurrent){
            SetRect(); //TODO: OPTIMUM PLEASE
            if(windowRect.y < 200) windowRect.position = new Vector2(windowRect.x, 200);

            SetMouseIconState();
            SetReSizingState();
            DrawWindow();

        }

        private void DrawWindow(){
            EditorGUI.DrawRect(windowRect, WINDOW_PLANE_COLOR);
            EditorGUI.DrawRect(handleRect, Color.black);
            GUI.Window(1, windowRect, _=> WindowFunction(), GUIContent.none, GUIStyle.none);
        }

        float value;
        private void WindowFunction(){
            LoadStyles();
            EditorGUI.DrawRect(columnRect, Color.black);
            EditorGUI.DrawRect(rowRect, Color.black);
            // DrawGroups();
            // DrawThumbnails();
            GUILayout.Space(windowRect.height-20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(columnRect.xMax);
            value = GUILayout.HorizontalScrollbar(value, 0.1f, 0, 10, GUILayout.Width(windowRect.width- columnRect.xMax));
            GUILayout.EndHorizontal();

        }
        Vector2 scrollPosition;
        private void DrawThumbnails(){




            // scrollPosition = GUI.BeginScrollView(new Rect(columnRect.xMax,windowRect.height-40,windowRect.width-columnRect.xMax,40 ), scrollPosition, new Rect(0, 0,48,48));
            // GUILayout.HorizontalScrollbar
            GUILayout.BeginHorizontal();
            GUILayout.Space(columnRect.xMax);

            var isValid = anim != null;
            if(isValid){
                var sprite = anim.GetSpriteList();
                for(var i = 0; i < sprite.Count; i++){
                    DrawThumbnail($"{i+1}");
                }
            }
            GUILayout.EndHorizontal();

        }

        private void DrawThumbnail(string label){
            GUILayout.Button(label, spriteThumbnailStyle);
        }


        private void DrawGroups(){
            
            GUILayout.BeginArea(groupPlaneRect);
            EditorGUILayout.BeginVertical();
            var scrollPosition = EditorGUILayout.BeginScrollView(
                Vector2.up * scrollPos.y,
                false,
                false,
                GUIStyle.none,
                GUI.skin.verticalScrollbar,
                GUIStyle.none
            );
            
            scrollPos = new Vector2(scrollPos.x, scrollPosition.y );
            var isValid = anim != null;
            if(isValid){
                var groups = anim.Groups;
                for (var i = 0; i < groups.Count; i++){
                    var group = groups[i];
                    var name = $"Group {i+1}";
                    // var name = PixelAnimatorWindow.AnimatorWindow.AnimationPreferences.GetBoxData(group.BoxDataGuid).boxType;
                    DrawGroup(group, name);
                }
            } 
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawGroup(Group group, string label){
            GUILayout.Label(label, groupStyle);
            if(!group.isExpanded) return;
            var layers = group.layers;
            for(var i = 0; i < layers.Count; i++){
                DrawLayer(layers[i], $"Layer {i+1}");
            }
                
        }


        Rect rect;
        Vector2 pos;
        private void DrawLayer(Layer layer, string label){
            GUILayout.Label(label, layerStyle);
            var inRepaint = Event.current.type == EventType.Repaint;
            if(inRepaint) rect=GUILayoutUtility.GetLastRect();
            var planeRect = new Rect(columnRect.xMax, rect.y, framePlaneRect.width,layerStyle.fixedHeight);
            
            EditorGUILayout.BeginHorizontal();
            
            var frames = layer.frames;
            for (var i = 0; i < frames.Count; i++){
                // DrawFrame();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawFrame(){
            GUILayout.Button("tests", GUILayout.Width(16),GUILayout.Height(16));
        }



        private void SetRect(){
            var animatorWindowRect = PixelAnimatorWindow.AnimatorWindow.position;
            windowRect.size = new Vector2(animatorWindowRect.size.x, animatorWindowRect.height - windowRect.position.y);
            handleRect = new Rect(windowRect.x, windowRect.y-HANDLE_HEIGHT, windowRect.width, HANDLE_HEIGHT);
            columnRect = new Rect(GROUP_PANEL_WIDTH,0, COLUMN_WIDTH, windowRect.height);
            rowRect = new Rect(0, TOOL_PANEL_HEIGHT, windowRect.width, ROW_HEIGHT);
            groupPlaneRect = new Rect(0, rowRect.yMax, windowRect.width, windowRect.height - rowRect.yMax);
            framePlaneRect = new Rect(columnRect.xMax, rowRect.yMax, windowRect.width - columnRect.xMax, windowRect.height);
            thumbnailPlaneRect = new Rect(columnRect.xMax, 0, windowRect.width - columnRect.xMax,TOOL_PANEL_HEIGHT);
            ReSizeWindowRect();
        }
        private void SetReSizingState(){
            var eventCurrent = PixelAnimatorWindow.AnimatorWindow.EventCurrent;
            var eventType = eventCurrent.type;
            var leftCLicked = eventType == EventType.MouseDown && eventCurrent.button == 0;
            var inHandleRect = handleRect.Contains(eventCurrent.mousePosition);
            if(leftCLicked && inHandleRect) reSizing = true;
            var isMouseUp = eventType == EventType.MouseUp;
            if(isMouseUp) reSizing = false;
        }
        private void ReSizeWindowRect(){
            if(!reSizing) return;
            var eventCurrent = PixelAnimatorWindow.AnimatorWindow.EventCurrent;
            windowRect.position = Vector2.up * eventCurrent.mousePosition.y;
        }
        private void SetMouseIconState(){
            EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeVertical);
            PixelAnimatorWindow.AddCursorBool(reSizing, MouseCursor.ResizeVertical);
        }





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