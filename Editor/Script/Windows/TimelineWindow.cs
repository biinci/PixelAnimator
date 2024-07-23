using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Utility;
using binc.PixelAnimator.Common;


namespace binc.PixelAnimator.Editor.Windows{

    [System.Serializable]
    public class TimelineWindow : Window, IUpdate{
        
        #region Variables
        private bool clickedHandle, isPlaying;
        public bool TimelineDragging => clickedHandle;


        private Rect handleRect,
           columnRect,
           rowRect,
           framePlaneRect,
           groupPlaneRect,
           thumbnailPlaneRect,
           toolPanelRect;

        private Texture2D previousFrameTex,
            playTex,
            nextFrameTex,
            timelineBurgerTex,
            pauseTex,
            playPauseTex,
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

        private GenericMenu burgerMenu, groupMenu, layerMenu;

        private readonly Vector2 frameBlank = new (48, 48);
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
        animatorButtonStyle;
        private float timer;
        private Vector2 scrollPos;
        private bool reSizing = false;

        
        private ButtonData<int> thumbnailButton;
        private ButtonData<Group> groupButton;
        private ButtonData<KeyValuePair<Group,Layer>> layerButton;
        private bool burgerClick, previousSpriteClick, playPauseClick, nextSpriteClick;

        #endregion

        #region Init

        private PixelAnimation anim;

        public override void Initialize(){
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
        }


        private void InitRect() {
            var position = PixelAnimatorWindow.AnimatorWindow.position;
            windowRect.x = 0;
            windowRect.y = position.yMax - windowRect.height;
            framePlaneRect = new Rect(columnRect.xMax, rowRect.yMax, windowRect.width - columnRect.xMax, windowRect.height);
        }
        #endregion


        public void InspectorUpdate(){
            anim = PixelAnimatorWindow.AnimatorWindow.SelectedAnimation;
        }


        public override void ProcessWindow(){
            SetRect(); //TODO: OPTIMUM PLEASE
            if(windowRect.y < 200) windowRect.position = new Vector2(windowRect.x, 200);
            if(windowRect.y > PixelAnimatorWindow.AnimatorWindow.position.height) windowRect.position = new Vector2(windowRect.x,400);
            SetMouseIconState();
            SetReSizingState();
            DrawWindow();
            
            // PixelAnimatorWindow.AnimatorWindow.Repaint();
        }
        public override void FocusFunctions(){
           LinkWindowButtons();
        }


        #region Link Buttons
        private void LinkWindowButtons(){
            if(Event.current.type == EventType.KeyDown){
                if(Event.current.keyCode == KeyCode.Return){
                    playPauseClick = !playPauseClick;   
                }
                if(Event.current.keyCode == KeyCode.LeftArrow){
                    previousSpriteClick = true;
                }
                if(Event.current.keyCode == KeyCode.RightArrow){
                    nextSpriteClick = true;
                }

            }
            LinkThumbnailButton();
            LinkPlayPauseButton();
            LinkChangeSpriteButton();
            LinkBurgerMenuButton();
            LinkGroupsButton();
            LinkLayersButton();
        }

        private void LinkLayersButton(){
            if(!layerButton.clicked) return;
            layerMenu = new GenericMenu();
            
            layerMenu.AddItem(new GUIContent("Delete Layer"), false, ()=>{layerButton.data.Key.layers.Remove(layerButton.data.Value);});
            layerMenu.ShowAsContext();
            layerButton.clicked = false;
        }

        private void LinkGroupsButton(){
            if(!groupButton.clicked) return;
            var group = groupButton.data;
            groupMenu = new GenericMenu();
            groupMenu.AddItem(new GUIContent("Delete Group"), false, ()=>{SelectedAnim.RemoveGroup(group.BoxDataGuid);});
            groupMenu.AddItem(new GUIContent("Add Layer"), false, ()=>{group.AddLayer(SelectedAnim.PixelSprites);});
            groupMenu.AddItem(new GUIContent("Expand"), group.isExpanded, () => {group.isExpanded = !group.isExpanded;});
            groupMenu.ShowAsContext();
            groupButton.clicked = false;

        }

        private void LinkBurgerMenuButton(){
            if(!burgerClick) return;
            burgerMenu = new GenericMenu();

            var boxData = PixelAnimatorWindow.AnimatorWindow.AnimationPreferences.BoxData;
            for(var i = 0 ; i < boxData.Count; i ++){
                burgerMenu.AddItem(new GUIContent($"Add Group/{boxData[i].boxType}"), false, 
                obj=>{SelectedAnim.AddGroup(boxData[(int)obj].Guid);},i);
 
            }   
            burgerMenu.ShowAsContext();
            burgerClick = true;
        }


        

        private void LinkThumbnailButton(){
            if(!thumbnailButton.clicked) return;
            PixelAnimatorWindow.AnimatorWindow.SelectFrame(thumbnailButton.data);
            
        }

        private void LinkPlayPauseButton(){
            
            if(!playPauseClick) return;
            isPlaying = !isPlaying ;
            playPauseTex = isPlaying ? pauseTex : playTex;
            playPauseClick = false;
        }

        private void LinkChangeSpriteButton(){
            if(!previousSpriteClick && !nextSpriteClick) return;
            var factor = previousSpriteClick ? -1 : 1;
            var animator = PixelAnimatorWindow.AnimatorWindow;
            var mod = SelectedAnim.GetSpriteList().Count;
            var index = (animator.IndexOfSelectedFrame + factor) % mod;
            index = index == -1 ? mod-1 : index;
            animator.SelectFrame(index); 
            previousSpriteClick = false;
            nextSpriteClick = false;
        }

        #endregion

        #region Timeline
        private void DrawWindow(){
            EditorGUI.DrawRect(windowRect, WINDOW_PLANE_COLOR);
            EditorGUI.DrawRect(handleRect, Color.black);
            GUI.Window(1, windowRect, _=> WindowFunction(), GUIContent.none, GUIStyle.none);
        }

        private Vector2 scrollPosition;

        private void WindowFunction(){
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
            GUILayout.BeginHorizontal();
            var space = columnRect.xMax;
            GUILayout.Space(space);
            
            scrollPosition = GUILayout.BeginScrollView(
                scrollPosition, 
                GUI.skin.horizontalScrollbar,
                GUIStyle.none,
                GUILayout.Width(windowRect.width - space), 
                GUILayout.Height(windowRect.height)

                );

            GUILayout.BeginHorizontal();           
            var isValid = anim != null;
            if(isValid) DrawThumbnails(anim.GetSpriteList());
 
 
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
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
                var clicked1 = GUILayout.Button(label, temp);
                return clicked1;
            }
            var clicked = GUILayout.Button(label, spriteThumbnailStyle);

            return clicked;
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
            for (var i = 0; i < groups.Count; i++){
                var group = groups[i];
                var name = PixelAnimatorWindow.AnimatorWindow.AnimationPreferences.GetBoxData(group.BoxDataGuid).boxType;
                DrawGroup(group, name);
            }        
            
        }

        private void DrawGroup(Group group, string label){
            if(GUILayout.Button(label, groupStyle)){
                groupButton.clicked = true;
                groupButton.data = group;
            }
            if(!group.isExpanded) return;
            var layers = group.layers;
            DrawLayers(group, layers);
                
        }

        private void DrawLayers(Group group, List<Layer> layers){
            for(var i = 0; i < layers.Count; i++){
                DrawLayer(layers[i], $"Layer {i+1}", group);
            }
        }

        private Rect layerRect;
        private void DrawLayer(Layer layer, string label, Group group){
            GUILayout.BeginHorizontal();
            if(GUILayout.Button(label, layerStyle)){
                layerButton.clicked = true;
                layerButton.data = new KeyValuePair<Group, Layer>(group,layer);  
              
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
            GUI.BeginScrollView(
                position, 
                scrollPosition,
                viewRect,
                GUIStyle.none, GUIStyle.none);
            GUILayout.BeginHorizontal();
            for (var i = 0; i < frames.Count; i++){
                DrawFrame();
            }
            if(inRepaint) framePanelWidth = GUILayoutUtility.GetLastRect().xMax;
            GUILayout.EndHorizontal();
            GUI.EndScrollView();
        }

        private void DrawFrame(){
            GUILayout.Button("", frameButtonStyle);
        }

        #endregion

        #region Rect
        private void SetRect(){
            var animatorWindowRect = PixelAnimatorWindow.AnimatorWindow.position;
            windowRect.size = new Vector2(animatorWindowRect.size.x, animatorWindowRect.height - windowRect.position.y);
            handleRect = new Rect(windowRect.x, windowRect.y-HANDLE_HEIGHT, windowRect.width, HANDLE_HEIGHT);
            columnRect = new Rect(GROUP_PANEL_WIDTH,0, COLUMN_WIDTH, windowRect.height);
            rowRect = new Rect(0, TOOL_PANEL_HEIGHT, windowRect.width, ROW_HEIGHT);
            groupPlaneRect = new Rect(0, rowRect.yMax, windowRect.width, windowRect.height - rowRect.yMax);
            framePlaneRect = new Rect(columnRect.xMax, rowRect.yMax, windowRect.width - columnRect.xMax, windowRect.height);
            thumbnailPlaneRect = new Rect(columnRect.xMax, 0, windowRect.width - columnRect.xMax,TOOL_PANEL_HEIGHT);
            toolPanelRect = new Rect(10,8, columnRect.x, TOOL_PANEL_HEIGHT);
            ReSizeWindowRect();
        }
        private void SetReSizingState(){
            var eventCurrent = Event.current;
            var eventType = eventCurrent.type;
            var leftCLicked = eventType == EventType.MouseDown && eventCurrent.button == 0;
            var inHandleRect = handleRect.Contains(eventCurrent.mousePosition);
            if(leftCLicked && inHandleRect) reSizing = true;
            var isMouseUp = eventType == EventType.MouseUp;
            if(isMouseUp) reSizing = false;
        }
        private void ReSizeWindowRect(){
            if(!reSizing) return;
            windowRect.position = Vector2.up * Event.current.mousePosition.y;
        }
        private void SetMouseIconState(){
            EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeVertical);
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