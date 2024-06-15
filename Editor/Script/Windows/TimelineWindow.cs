
using System.Collections;
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

        public override void SetWindow(Event eventCurrent){
            base.SetWindow(eventCurrent);
            CreateTimeline();
        }

        private void CreateTimeline(){
            var position = PixelAnimatorWindow.AnimatorWindow.position;
            windowRect.y = position.height - windowRect.height;
            windowRect.yMin = Mathf.Clamp(windowRect.y, 200, position.yMax - 150); 
            windowRect.size = new Vector2(position.width-30, windowRect.height);
            
            windowRect = GUI.Window(1, windowRect, _=>SetTimeline(), GUIContent.none, GUIStyle.none);
        }

        public override void FocusFunctions() {
            if (PixelAnimatorWindow.AnimatorWindow.WindowFocus != WindowEnum.Timeline) return;
            UIOperations();
            IsFocusChangeable = true;

        }

        private void SetTimeline(){
            columnLayout = new Rect(GroupPanelWidth, HandleHeight, 3, windowRect.height);
            rowLayout = new Rect(0, frameBlank.y+HandleHeight, windowRect.width, 4);

            var timelinePlaneRect = new Rect(0, HandleHeight, windowRect.width, windowRect.height);
            EditorGUI.DrawRect(timelinePlaneRect, timelinePlaneColour);

            framePlaneRect = new Rect(columnLayout.xMax,rowLayout.yMax, windowRect.width - columnLayout.xMax, windowRect.height - rowLayout.yMax);
            EditorGUI.DrawRect(framePlaneRect, framePlaneColour);

            spriteThumbnailRect = new Rect(columnLayout.xMax, handleRect.height, windowRect.width - columnLayout.xMax, rowLayout.yMin-handleRect.height);


            EditorGUI.DrawRect(rowLayout, Color.black);
            EditorGUI.DrawRect(columnLayout, Color.black);

            SetTimelineButtons();
            SetBurgerMenu();
            // SetPlayKeys();
            if (PixelAnimatorWindow.AnimatorWindow.TargetAnimation == null) return;
            if (SelectedAnim.GetSpriteList().Count > 0) {
            //    SetSpriteThumbnail();
            //    DrawSelectedFrame();
            //    if (selectedAnim.Groups.Count > 0) {
            //        CreateGroupButtons();
            //        if (selectedAnim.Groups[animatorWindow.ActiveGroupIndex].layers.Count > 0) {
            //            if(!isPlaying)SetFrameRect();

            //        }       
            //    } 
            }
            DrawHandle();
            SetTimelinePanel();
            // SetGroupKeys();

            Play();

        }


        private void SetTimelinePanel(){
            var groups = PixelAnimatorWindow.AnimatorWindow.SelectedAnimation.Groups;
            SetGroupPanel(groups);
            SetFramePanel(groups);
            SetThumbnailPanel();

        }


        private void SetGroupPanel(List<Group> groups) {
            var groupPanelRect = new Rect(0, rowLayout.yMax, windowRect.width, windowRect.height);

            using (new GUILayout.AreaScope(groupPanelRect)) {
                var scrollScope = new EditorGUILayout.ScrollViewScope(new Vector2(0, scrollPos.y),GUILayout.Height(windowRect.height - rowLayout.yMax));
                scrollPos = new Vector2(scrollPos.x, scrollScope.scrollPosition.y );

                using(scrollScope) {
                    using (new EditorGUILayout.VerticalScope()) {
                        if (PixelAnimatorWindow.AnimatorWindow.SelectedAnimation.Groups.Count > 0){
                            for(var i = 0; i < groups.Count; i ++){
                                var group = groups[i];
                                GUILayout.Label("", groupStyle);
                                var rect = GUILayoutUtility.GetLastRect();
                                SetGroupUI(group, rect, i);
                            }

                        }
                    }
                }                
            }

        }

        private void SetThumbnailPanel(){
            using (new GUILayout.AreaScope(spriteThumbnailRect)){
                var scrollScope = new EditorGUILayout.ScrollViewScope(new Vector2(scrollPos.x, 0),false,false, GUIStyle.none, GUIStyle.none, GUIStyle.none);
                using(scrollScope){
                    using(new EditorGUILayout.VerticalScope()){
                        DrawSpriteTumbnail();
                    }
                }
            }
        }


        public override void UIOperations(){
            var eventCurrent = PixelAnimatorWindow.AnimatorWindow.EventCurrent;
            DragTimeline(eventCurrent);

            
        }

        private void DrawHandle(){
            handleRect = new Rect(0, 0, windowRect.width, HandleHeight);
            EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeVertical);

            EditorGUI.DrawRect(handleRect, Color.black);
        }


        private void DragTimeline(Event eventCurrent){
            var worldHandleRect = PixelAnimatorUtility.GetWorldRect(handleRect, windowRect.position);

            switch (eventCurrent.type)
            {
                // drag timeline
                case EventType.MouseDown:
                    if (worldHandleRect.Contains(eventCurrent.mousePosition)) {
                        clickedHandle = true;
                    }
                    break;
                case EventType.MouseUp:
                    clickedHandle = false;
                    break;
            }

            // var isTimelineDraggable = clickedHandle && PixelAnimatorWindow.AnimatorWindow.CanvasWindow.EditingHandle == HandleTypes.None && eventCurrent.button == 0;
            // if (!isTimelineDraggable) return;
            windowRect.yMin = eventCurrent.mousePosition.y;
            PixelAnimatorWindow.AnimatorWindow.Repaint();

        }

        private void SetTimelineButtons(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var selectedAnim = animatorWindow.SelectedAnimation; 
            using (new GUILayout.HorizontalScope()) {

                if (PixelAnimatorUtility.Button(timelineBurgerTex, onMouseTimelineBurgerTex, burgerRect))
                {
                    timelinePopup?.ShowAsContext();
                }

                else if (selectedAnim != null && selectedAnim.GetSpriteList().Count > 0)
                {

                    if (PixelAnimatorUtility.Button(backTex, backRect))
                    {
                        switch (animatorWindow.ActiveFrameIndex)
                        {
                            case 0:
                                // animatorWindow.SetActiveFrame(selectedAnim.GetSpriteList().Count - 1);
                                break;
                            case > 0:
                                // animatorWindow.SetActiveFrame(animatorWindow.ActiveFrameIndex - 1);
                                break;
                        }

                        animatorWindow.Repaint();
                    }

                    else if (PixelAnimatorUtility.Button(durationTex, playRect))
                    {
                        isPlaying = !isPlaying;

                    }
                    else if (PixelAnimatorUtility.Button(frontTex, frontRect))
                    {

                        // animatorWindow.SetActiveFrame((animatorWindow.ActiveFrameIndex + 1) % animatorWindow.SelectedAnimation.GetSpriteList().Count);
                        animatorWindow.Repaint();
                    }
                }

            }

            animatorWindow.Repaint();

            durationTex = !isPlaying ? playTex : stopTex;
        }

        private void SetBurgerMenu(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            timelinePopup = new GenericMenu{ allowDuplicateNames = true };

            timelinePopup.AddItem(new GUIContent("Go to Preferences"), false, TargetPreferences);
            timelinePopup.AddItem(new GUIContent("Update Animation"), false, UpdateAnimation);

            timelinePopup.AddSeparator("");
            if (animatorWindow.SelectedAnimation == null) return;
            var boxData = animatorWindow.AnimationPreferences.BoxData;

            for (var i = 0; i < animatorWindow.AnimationPreferences.BoxData.Count; i++) {
                // timelinePopup.AddItem(new GUIContent(boxData[i].boxType), false, animatorWindow.AddGroup, boxData[i]);
            }
        }


        private void TargetPreferences(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            EditorGUIUtility.PingObject(animatorWindow.AnimationPreferences);
            Selection.activeObject = animatorWindow.AnimationPreferences;
        }

        private void UpdateAnimation(){
            // foreach(var group in selectedAnim.Groups) {
            //     var spriteCount = selectedAnim.PixelSprites.Count;
            //     for (var f = 0; f < spriteCount; f++) {
            //         
            //         foreach (var layer in group.layers) {
            //             var frame = layer.frames[f];
            //             
            //         }
            //     }
            //     
            //     
            //     
            //     foreach (var layer in group.layers) {
            //         for (var f = 0; f < layer.frames.Count; f++) {
            //             var frame = layer.frames[f];
            //             if (frame.frameType != FrameType.KeyFrame) {
            //                 frame.hitBoxData = new PropertyData();
            //                 continue;
            //             }
            //
            //             var hitBoxData = frame.HitBoxData;
            //             for (var i = 0; i < hitBoxData.genericData.Count; i++) {
            //                 var propertyValue = hitBoxData.genericData[i];
            //                 var myGuid = propertyValue.baseData.Guid;
            //                 var exist = preferences.HitBoxProperties.Any(x => x.Guid == myGuid);
            //
            //                 if (exist) continue;
            //                 hitBoxData.genericData.Remove(propertyValue);
            //                 i = -1;
            //             }
            //         }
            //     }
            // }

        }
        private void DrawSpriteTumbnail(){
            
            var sprites = SelectedAnim.GetSpriteList();

            if (columnRects == null || columnRects.Length != sprites.Count) columnRects = new Rect[sprites.Count];
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;

            using(new EditorGUILayout.HorizontalScope()){
                for(var i = 0; i < columnRects.Length; i++){

                    var sprite = sprites[i];
                    var spriteTex = AssetPreview.GetAssetPreview(sprite);
                    
                    var spriteTexRect = new Rect(i*48, -handleRect.height+2, 64, 64);
                    if(i == 0) spriteTexRect.position -= new Vector2(8,0);


                    if(GUILayout.Button("", spriteThumbnailStyle, GUILayout.Width(48), GUILayout.Height(48))){
                        // animatorWindow.SetActiveFrame(i);
                        // animatorWindow.SetPropertyFocus(PropertyFocusEnum.Sprite);
                    }
                    var rect = GUILayoutUtility.GetLastRect();

                    if(spriteTex != null){
                        GUI.DrawTexture(rect, spriteTex);
                    }
                    
                    if( i == animatorWindow.ActiveFrameIndex){
                        EditorGUI.DrawRect(rect, new Color(0.8f,0.8f, 0.8f, 0.2f));
                        if(animatorWindow.PropertyFocus == PropertyFocusEnum.Sprite) DrawPingTextures(rect);
                    }

                    
                }



            }
        }

        private void DrawPingTextures(Rect rect, int size = 8){
            var topLeft = new Rect(rect.x, rect.y, size, size);
            var bottomLeft = new Rect(rect.x, rect.yMax - size, size, size);
            var topRight = new Rect(rect.x + rect.width - size, rect.y, size, size);
            var bottomRight = new Rect(rect.x + rect.width - size, rect.yMax-size, size, size);

            GUI.DrawTexture(topLeft, pingTexs[0]);
            GUI.DrawTexture(topRight, pingTexs[1]);
            GUI.DrawTexture(bottomLeft, pingTexs[2]);
            GUI.DrawTexture(bottomRight, pingTexs[3]);
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
                    var mod = (  animatorWindow.ActiveFrameIndex +1 )% SelectedAnim.GetSpriteList().Count;
                    // animatorWindow.SetActiveFrame(mod);
                    animatorWindow.Repaint();
                }
            }else if(!isPlaying && timer != 0){
                timer = 0;
            }
        }
        private void SetGroupUI(Group group, Rect rect, int index){
            var boxData = PixelAnimatorWindow.AnimatorWindow.AnimationPreferences.GetBoxData(group.BoxDataGuid);
            SetGroupButtons(group, rect, index);
            GUI.Label(new Rect(rect.x + rect.height + 2, rect.y + 5, 100, 20), boxData.boxType);
            SetLayerUI(group, index);
        }

        private void SetGroupButtons(Group group, Rect rect, int index){

            var foldoutRect = new Rect (rect.xMax - 40, rect.yMax - 25, 25, 15);
            var colliderTypeRect = new Rect (foldoutRect.x - 28, foldoutRect.y-5, 24, 24);
            var visibleRect = new Rect(colliderTypeRect.x - 32, foldoutRect.y, 24, 18);
            
            var foldoutTex = group.isExpanded ? openTex : closeTex;
            
            if(PixelAnimatorUtility.Button(foldoutTex, foldoutRect)){
                group.isExpanded = !group.isExpanded;
            }

            var isTriggerTex = group.colliderTypes == ColliderTypes.NoTrigger ? noTriggerTex : triggerTex;
            if(PixelAnimatorUtility.Button(isTriggerTex, colliderTypeRect)){
                group.colliderTypes = (int)group.colliderTypes == 1 ?(ColliderTypes) 2 :(ColliderTypes) 1;
            }

            var isVisibleTex = group.isVisible ? visibleTex : unvisibleTex;
            if (PixelAnimatorUtility.Button(isVisibleTex, visibleRect)) {
                group.isVisible = !group.isVisible;
            }

            var settingsRect = new Rect(rect.x, rect.y, (int)rect.height-4, (int)rect.height-4);
            var settingsTex = new Texture2D((int)rect.height, (int)rect.height-1);
            if(PixelAnimatorUtility.Button(settingsTex, settingsRect)){
                SetGroupMenu(index);
                groupPopup.ShowAsContext();
            }

        }
        private void SetLayerUI(Group group, int groupIndex){
            var darkColor = new Color(0.13f, 0.13f, 0.15f);
            var layers = group.layers;
            for(var i =0; i< layers.Count; i ++){
                using(new GUILayout.HorizontalScope()){
                    if(group.isExpanded){
                        GUILayout.Label($"Layer {i+1}", layerStyle);
                        var layerRect = GUILayoutUtility.GetLastRect();
                        var burgerMenuRect = new Rect(layerRect.x, layerRect.y, 24, layerRect.height);
                        CreateLayerButtons(burgerMenuRect, groupIndex, i);
                        if(i == layers.Count-1){
                            var bottomRect = new Rect(layerRect.xMax + columnLayout.width, layerRect.yMax - 1, windowRect.width-layerRect.width-columnLayout.width, 2);
                            EditorGUI.DrawRect(bottomRect, darkColor);
                        }    
                    }
                }
            }            
        
        }

        private void CreateLayerButtons(Rect burgerMenuRect, int groupIndex, int layerIndex){
            if(PixelAnimatorUtility.Button(layerBurgerMenu, burgerMenuRect)){
                SetLayerMenu(groupIndex, layerIndex);

                layerPopup.ShowAsContext();
            } 
        
        }

        private void SetGroupMenu(int groupIndex){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            groupPopup = new GenericMenu{ allowDuplicateNames = true };
            // groupPopup.AddItem(new GUIContent("Settings/Delete Group"), false, animatorWindow.OnRemoveGroup, groupIndex);
            // groupPopup.AddItem(new GUIContent("Settings/Add Layer"), false, animatorWindow.AddLayer, groupIndex);
        }

        private void SetLayerMenu(int groupIndex, int layerIndex){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var array = new int[]{groupIndex, layerIndex};
            layerPopup = new GenericMenu{allowDuplicateNames = true};
            // layerPopup.AddItem(new GUIContent("Add Layer"), false, animatorWindow.AddLayer, groupIndex);
            // layerPopup.AddItem(new GUIContent("Delete Layer"), false, animatorWindow.OnRemoveLayer, array);

        }



        #region Frame

        private void SetFramePanel(List<Group> groups){
            using (new GUILayout.AreaScope(framePlaneRect)) {
                
                var barSkin = GUI.skin.horizontalScrollbar;
                var height = GUILayout.Height(windowRect.height - rowLayout.yMax);

                var scrollScope = new EditorGUILayout.ScrollViewScope(scrollPos,false,false,barSkin, GUIStyle.none,GUIStyle.none,height);
                
                scrollPos = scrollScope.scrollPosition;
                using(scrollScope) {
                    var space = groupStyle.fixedHeight;
                    for(var i = 0; i < groups.Count; i ++){
                        var group = groups[i];
                        GUILayout.Space(space);

                        for(var j = 0; j < group.layers.Count; j++){

                            var layer = group.layers[j];
                            using(new GUILayout.HorizontalScope()){
                                if(group.isExpanded){
                                    for(var k = 0; k < layer.frames.Count; k++){
                                        var frame = layer.frames[k];
                                        SetFrame(i, j, k, frame);
                                    }

                                }
                            }

                        }
                    }
                }
            }

        }


        private GUIStyle GetFrameStyle(FrameType frameType){
            switch (frameType)
            {
                case FrameType.CopyFrame:
                return copyFrameStyle;
                case FrameType.EmptyFrame:
                return emptyFrameStyle;
                case FrameType.KeyFrame:
                return keyFrameStyle;
            }

            return GUIStyle.none;
        }

        private void SetFrame(int groupIndex, int layerIndex, int frameIndex, Frame frame){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var eventCurrent = animatorWindow.EventCurrent;
            var activeStyle = GetFrameStyle(frame.frameType);

            var buttonClicked = GUILayout.Button("", activeStyle);
            var rightClicked =  eventCurrent.button == 1 && buttonClicked;
            var leftClicked = eventCurrent.button == 0 && buttonClicked;

            // var isCurrentFrame = animatorWindow.IsCurrentFrame(groupIndex, layerIndex, frameIndex); 

            var rect = GUILayoutUtility.GetLastRect();
            // if(isCurrentFrame && animatorWindow.PropertyFocus == PropertyFocusEnum.HitBox){
                // DrawPingTextures(rect, 12);
            // }

            if(leftClicked){
                // animatorWindow.SetActiveGroup(groupIndex);
                // animatorWindow.SetActiveLayer(layerIndex);
                // animatorWindow.SetActiveFrame(frameIndex);
                // animatorWindow.SetPropertyFocus(PropertyFocusEnum.HitBox);
            }
            if(rightClicked) ChangeFrameType(frame);
        }

        private void ChangeFrameType(Frame frame){
            switch (frame.frameType){
                case FrameType.CopyFrame:
                    frame.frameType = FrameType.KeyFrame;
                break;
               case FrameType.EmptyFrame:
                    frame.frameType = FrameType.CopyFrame;
                break;
                case FrameType.KeyFrame:
                    frame.frameType = FrameType.EmptyFrame;
                break;
            }

        }

        //Drawing the selected frame or sprite corner.

        private void SetFrameCopyPaste(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            if (animatorWindow.WindowFocus != WindowEnum.Timeline || animatorWindow.PropertyFocus != PropertyFocusEnum.HitBox) return;
            var eventCurrent = Event.current;
            if (eventCurrent.type == EventType.ValidateCommand && eventCurrent.commandName == "Copy")
                eventCurrent.Use();
            
            switch (eventCurrent.type) {
                case EventType.ExecuteCommand when eventCurrent.commandName == "Copy":
                    // EditorGUIUtility.systemCopyBuffer =
                        // JsonUtility.ToJson(selectedAnimation.Groups[activeGroupIndex].layers[activeLayerIndex].frames[activeFrameIndex]);
                    break;
                case EventType.ValidateCommand when eventCurrent.commandName == "Paste":
                    eventCurrent.Use();
                    break;
            }

            if (eventCurrent.type == EventType.ExecuteCommand && eventCurrent.commandName == "Paste") {
                var targetAnimation = animatorWindow.TargetAnimation;
                var copiedFrame = JsonUtility.FromJson<Frame>(EditorGUIUtility.systemCopyBuffer);
                
                var frameProp = targetAnimation.FindProperty("layers").GetArrayElementAtIndex(animatorWindow.ActiveLayerIndex)
                    .FindPropertyRelative("frames").GetArrayElementAtIndex(animatorWindow.ActiveFrameIndex);
                
                var hitBoxRectProp = frameProp.FindPropertyRelative("hitBoxRect");
                // var colliderType = frameProp.FindPropertyRelative("colliderType");

                // colliderType.enumValueIndex = (int)copiedFrame.colliderType;
                hitBoxRectProp.rectValue = copiedFrame.hitBoxRect;
                targetAnimation.ApplyModifiedProperties();

            }
        }

        #endregion


    }
}