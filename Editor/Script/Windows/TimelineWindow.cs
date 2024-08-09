using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Common;
using System;


namespace binc.PixelAnimator.Editor.Windows{

    [Serializable]
    public partial class TimelineWindow : Window, IUpdate{
        
        #region Variables
        private bool isPlaying;

        private Rect handleRect,
           columnRect,
           rowRect,
           groupPlaneRect,
           thumbnailPlaneRect,
           toolPanelRect;

        private Texture2D previousFrameTex,
            playTex,
            nextFrameTex,
            timelineBurgerTex,
            pauseTex,
            playPauseTex;

        public static readonly Color WINDOW_PLANE_COLOR = new (0.1f, 0.1f, 0.1f, 1);

        private GenericMenu burgerMenu, groupMenu, layerMenu;

        private const float HANDLE_HEIGHT = 6;
        private float GROUP_PANEL_WIDTH => PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.GetStyle("Group").fixedWidth;
        private float TOOL_PANEL_HEIGHT => PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.GetStyle("Tool").fixedWidth;

        private float COLUMN_WIDTH => PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.GetStyle("TimelineLayout").fixedWidth;
        private float ROW_HEIGHT => PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.GetStyle("TimelineLayout").fixedHeight;

        private GUIStyle groupStyle, 
        layerStyle, 
        keyFrameStyle, 
        emptyFrameStyle, 
        copyFrameStyle, 
        frameButtonStyle, 
        spriteThumbnailStyle,
        animatorButtonStyle,
        timelineStyle;

        private float timer;
        private Vector2 scrollPos;
        private bool reSizing = false;

        private int loopGroupIndex, loopLayerIndex, loopFrameIndex;
        private ButtonData<int> thumbnailButton;
        private ButtonData<Group> groupButton;
        private ButtonData<ValueTuple<Group,Layer>> layerButton;
        private ButtonData<ValueTuple<int,int,int>> frameButton;
        private bool burgerClick, previousSpriteClick, playPauseClick, nextSpriteClick;

        #endregion

        #region Init

        private PixelAnimation anim;

        public override void Initialize(int id){
            Id = id;
            LoadInitResources();
            InitRect();
        }
 
        private void LoadInitResources(){
            LoadTextures();
            LoadStyles();
            
            playPauseTex = playTex;
        }

        private void LoadTextures(){
            previousFrameTex = Resources.Load<Texture2D>("Sprites/Back");
            nextFrameTex = Resources.Load<Texture2D>("Sprites/Front");
            timelineBurgerTex = Resources.Load<Texture2D>("Sprites/TimelineBurgerMenu");
            pauseTex = Resources.Load<Texture2D>("Sprites/Pause");
            playTex = Resources.Load<Texture2D>("Sprites/Play");
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
            animatorButtonStyle = new GUIStyle(mySkin.GetStyle("AnimatorButton"));
            timelineStyle = new GUIStyle(mySkin.GetStyle("Timeline"));
        }


        private void InitRect() {
            var position = PixelAnimatorWindow.AnimatorWindow.position;
            windowRect.x = 0;
            windowRect.y = position.yMax - windowRect.height;
        }
        #endregion


        public void InspectorUpdate(){
            anim = PixelAnimatorWindow.AnimatorWindow.SelectedAnimation;
        }


        public override void ProcessWindow(){
            SetRect(); //TODO: OPTIMUM PLEASE
            ClampTimelinePosition();
            SetMouseIconState();
            SetReSizingState();
            DrawWindow();
        }

        private void ClampTimelinePosition(){
            var height = PixelAnimatorWindow.AnimatorWindow.position.height;
            if(windowRect.y < 200) windowRect.position = new Vector2(windowRect.x, 200);
            if(windowRect.y > height) windowRect.position = new Vector2(windowRect.x,400);
        }

        public override void FocusFunctions(){
            if(SelectedAnim == null) return;
            SetShortcuts();
            LinkWindowButtons();
        }


        #region Timeline
        private void DrawWindow(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            GUI.Window(Id, windowRect, _=> WindowFunction(), GUIContent.none, timelineStyle);
            
        }

        private Vector2 scrollPosition;

        private void WindowFunction(){
            EditorGUI.DrawRect(handleRect, Color.black);
            EditorGUI.DrawRect(new Rect(0, handleRect.height, windowRect.width, windowRect.height), WINDOW_PLANE_COLOR);
            
            EditorGUI.DrawRect(new Rect(columnRect.xMax, rowRect.yMax, windowRect.width-columnRect.xMax,windowRect.height-rowRect.yMax), new Color(0.04f,0.04f,0.04f));
            EditorGUI.DrawRect(new Rect(groupPlaneRect.position,new Vector2(columnRect.xMin, groupPlaneRect.height)), new Color(0.07f, 0.07f, 0.07f));
            LoadStyles();

            DrawPartitionLines();
            DrawToolButtons();

            DrawGroupPanel();
            DrawThumbnailPanel();


            if(isPlaying) Play();
        }

        private void DrawPartitionLines(){
            EditorGUI.DrawRect(columnRect, Color.black);
            EditorGUI.DrawRect(rowRect, Color.black);
        }

        private float spaceTool = 110;
        private void DrawToolButtons(){
            GUILayout.BeginArea(toolPanelRect);
            GUILayout.BeginHorizontal();
           
            burgerClick = GUILayout.Button(timelineBurgerTex, animatorButtonStyle);
           
            GUILayout.Space(spaceTool);
           
            if(GUILayout.Button(previousFrameTex, animatorButtonStyle))
                previousSpriteClick = true;
            if(GUILayout.Button(playPauseTex, animatorButtonStyle))
                playPauseClick = true;
            if(GUILayout.Button(nextFrameTex, animatorButtonStyle))
                nextSpriteClick = true;
           
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawThumbnailPanel(){
            GUILayout.BeginVertical();
            GUILayout.Space(HANDLE_HEIGHT);
            GUILayout.BeginHorizontal();
            var space = columnRect.xMax;
            GUILayout.Space(space);
            
            scrollPosition = GUILayout.BeginScrollView(
                scrollPosition, 
                GUI.skin.horizontalScrollbar,
                GUIStyle.none,
                GUILayout.Width(windowRect.width - space), 
                GUILayout.Height(windowRect.height-HANDLE_HEIGHT)

                );

            GUILayout.BeginHorizontal();           
            var isValid = anim != null;
            if(isValid) DrawThumbnails(anim.GetSpriteList());
 
 
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawThumbnails(List<Sprite> sprites){
            for(var i = 0; i < sprites.Count; i++){
                var clicked = DrawThumbnail($"{i+1}",i);
                if(clicked && !thumbnailButton.clicked){
                    thumbnailButton.clicked = true;
                    thumbnailButton.data = i;
                }
                if(thumbnailButton.clicked && thumbnailButton.data == i){
                    thumbnailButton.clicked = clicked;
                }
            }
            
        }

        private bool DrawThumbnail(string label, int index){
            if(index == PixelAnimatorWindow.AnimatorWindow.IndexOfSelectedFrame){
                var temp = new GUIStyle(spriteThumbnailStyle){normal = spriteThumbnailStyle.hover};
                var clicked = GUILayout.Button(label, temp);
                return clicked;
            }else{
                var clicked = GUILayout.Button(label, spriteThumbnailStyle);
                return clicked;
            }
        }

        private void DrawGroupPanel(){
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

            scrollPos = Vector2.right * scrollPos.x + Vector2.up * scrollPosition.y;
            
            var isValid = anim != null;
            if(isValid) DrawGroups(anim.Groups);
        
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawGroups(List<Group> groups){
            var animationPreferences = PixelAnimatorWindow.AnimatorWindow.AnimationPreferences;
            for (var i = 0; i < groups.Count; i++){
                loopGroupIndex = i;
                var group = groups[i];
                var name = animationPreferences.GetBoxData(group.BoxDataGuid).boxType;
                DrawGroup(group, name);
            }        
            
        }

        private void DrawGroup(Group group, string label){
            var clickedGroupButton = GUILayout.Button(label, groupStyle);
            if(clickedGroupButton){
                groupButton.clicked = true;
                groupButton.data = group;
            }
            if(!group.isExpanded) return;
            var layers = group.layers;
            DrawLayers(group, layers);
                
        }

        private void DrawLayers(Group group, List<Layer> layers){
            for(var i = 0; i < layers.Count; i++){
                loopLayerIndex = i;
                DrawLayer(layers[i], $"Layer {i+1}", group);
            }
        }

        private Rect layerRect;
        private void DrawLayer(Layer layer, string label, Group group){
            GUILayout.BeginHorizontal();
            if(GUILayout.Button(label, layerStyle)){
                layerButton.clicked = true;
                layerButton.data = (group, layer); 
            }
            DrawFrames(layer.frames);
            GUILayout.EndHorizontal();

        }

        private float framePanelWidth;
        private void DrawFrames(List<Frame> frames){

            var inRepaint = Event.current.type == EventType.Repaint;
            if(inRepaint) layerRect=GUILayoutUtility.GetLastRect();
            
            var position = new Rect(columnRect.xMax, layerRect.y, thumbnailPlaneRect.width, frameButtonStyle.fixedHeight);
            var viewRect = new Rect(columnRect.xMax, layerRect.y, framePanelWidth, frameButtonStyle.fixedHeight);
            // GUI.BeginScrollView(
            //     position, 
            //     scrollPosition,
            //     viewRect,
            //     GUIStyle.none, GUIStyle.none
            // );
            GUILayout.BeginHorizontal();
            for (var i = 0; i < frames.Count; i++){
                loopFrameIndex = i;
                DrawFrame(frames[i]);
            }
            // if(inRepaint) framePanelWidth = GUILayoutUtility.GetLastRect().xMax;
            GUILayout.EndHorizontal();
            // GUI.EndScrollView();
        }

        private void DrawFrame(Frame frame){
            var style = GetFrameStyle(frame.GetFrameType());
            var click = GUILayout.Button("", style);
            if (click){
                frameButton.clicked = true;
                frameButton.data = (loopGroupIndex, loopLayerIndex, loopFrameIndex);
            }
        }

        private GUIStyle GetFrameStyle(FrameType type){
            switch (type){
                case FrameType.KeyFrame:
                    return keyFrameStyle;
                case FrameType.CopyFrame:
                    return copyFrameStyle;
                case FrameType.EmptyFrame:
                    return emptyFrameStyle;
                default:
                    return GUIStyle.none;
            }
        }

        #endregion

        #region Rect
        private void SetRect(){
            var animatorWindowRect = PixelAnimatorWindow.AnimatorWindow.position;
            windowRect.size = new Vector2(animatorWindowRect.size.x, animatorWindowRect.height - windowRect.position.y);
            handleRect = new Rect(0, 0, windowRect.width, HANDLE_HEIGHT);
            columnRect = new Rect(GROUP_PANEL_WIDTH, HANDLE_HEIGHT, COLUMN_WIDTH, windowRect.height);
            rowRect = new Rect(0, TOOL_PANEL_HEIGHT + HANDLE_HEIGHT, windowRect.width, ROW_HEIGHT);
            groupPlaneRect = new Rect(0, rowRect.yMax, windowRect.width, windowRect.height - rowRect.yMax);
            thumbnailPlaneRect = new Rect(columnRect.xMax, HANDLE_HEIGHT, windowRect.width - columnRect.xMax,TOOL_PANEL_HEIGHT);
            toolPanelRect = new Rect(10,8+HANDLE_HEIGHT, columnRect.x, TOOL_PANEL_HEIGHT);
            var availableSpace = new Rect(0,0, animatorWindowRect.width, windowRect.y);
            PixelAnimatorWindow.AnimatorWindow.SetAvailableRect(availableSpace);
            
            ReSizeWindowRect();
        }
        private void SetReSizingState(){
            var r = new Rect(windowRect.position, handleRect.size);
            var eventCurrent = Event.current;
            var eventType = eventCurrent.type;
            var leftCLicked = eventType == EventType.MouseDown && eventCurrent.button == 0;
            var inHandleRect = r.Contains(eventCurrent.mousePosition);
            if(leftCLicked && inHandleRect) reSizing = true;
            var isMouseUp = eventType == EventType.MouseUp;
            if(isMouseUp) reSizing = false;
        }
        private void ReSizeWindowRect(){
            if(!reSizing) return;
            windowRect.position = Vector2.up * Event.current.mousePosition.y;
        }
        private void SetMouseIconState(){
            var r = new Rect(windowRect.position, handleRect.size);
            EditorGUIUtility.AddCursorRect(r, MouseCursor.ResizeVertical);
            PixelAnimatorWindow.AddCursorBool(reSizing, MouseCursor.ResizeVertical);
        }
        #endregion
        private void Play(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            if (isPlaying) {
                var fps = SelectedAnim.fps;
                if(fps == 0) Debug.Log("Frame rate is zero");
                var deltaTime = animatorWindow.editorDeltaTime;
                timer += deltaTime;
                if(timer >= 1f/fps){
                    timer -= 1f/fps;
                    var mod = (  animatorWindow.IndexOfSelectedFrame +1 ) % SelectedAnim.GetSpriteList().Count;
                    animatorWindow.SelectFrame(mod);
                   
                }
                animatorWindow.Repaint();
            }else if(!isPlaying && timer != 0){
                timer = 0;
            }
        }
    }
}