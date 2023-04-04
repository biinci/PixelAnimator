using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Utility;
using binc.PixelAnimator.Common;
using System.Linq;

namespace binc.PixelAnimator.Editor.Window{


    public class TimelineWindow : CustomWindow{
        

        private bool timelineDragging, isPlaying;
        public bool TimelineDragging => timelineDragging;
        private Rect handleRect,
            burgerRect,
            backRect,
            playRect,
            frontRect,
            columnLayout,
            rowLayout;

        private Texture2D backTex,
            playTex,
            frontTex,
            timelineBurgerTex,
            onMouseTimelineBurgerTex,
            singleFrameTex,
            linkedFrameTex,
            emptyFrameTex,
            keyFrameTex,
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


        private GenericMenu timelinePopup, settingsPopup, layerPopup;

        private Rect[] columnRects;
        private List<GroupEditorData> groupEditorData;
        private readonly Vector2 frameBlank = new Vector2(48, 48);
        private const float HandleHeight = 5;

        public TimelineWindow(PixelAnimatorWindow animatorWindow) : base(animatorWindow) {
            LoadInitResources();

        }


        private void LoadInitResources(){ //okey
            backTex = Resources.Load<Texture2D>("Sprites/Back");
            frontTex = Resources.Load<Texture2D>("Sprites/Front");
            timelineBurgerTex = Resources.Load<Texture2D>("Sprites/TimelineBurgerMenu");
            onMouseTimelineBurgerTex = Resources.Load<Texture2D>("Sprites/TimelineBurgerMenu2");
            singleFrameTex = Resources.Load<Texture2D>("Sprites/frame");
            linkedFrameTex = Resources.Load<Texture2D>("Sprites/linked frame");
            keyFrameTex = Resources.Load<Texture2D>("Sprites/key frame");
            emptyFrameTex = Resources.Load<Texture2D>("Sprites/empty frame");
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
            durationTex = playTex;


        }



        public void SetWindow(Event eventCurrent){
            CreateTimeline(eventCurrent);
            DragTimelineWindow(eventCurrent);
        }


        private void CreateTimeline(Event eventCurrent){
            

            var position = animatorWindow.position;
            windowRect.y = position.yMax - windowRect.height;
            windowRect.yMin = Mathf.Clamp(windowRect.y, 200, position.yMax - 150); //Clamped the timeline y position.
            windowRect.size = new Vector2(position.width + 10, windowRect.height);



            windowRect = GUILayout.Window(PixelAnimatorWindow.TimelineId, windowRect, _=>DrawTimeline(eventCurrent), GUIContent.none, GUIStyle.none);
            var clickedTimeline = windowRect.Contains(eventCurrent.mousePosition) && Event.current.type == EventType.MouseDown; 
            if (clickedTimeline) {
                animatorWindow.SetWindowFocus(WindowFocusEnum.TimeLine);
            }


        }

        private void DragTimelineWindow(Event eventCurrent){

            switch (eventCurrent.type)
            {
                case EventType.MouseDrag:
                    if (handleRect.Contains(eventCurrent.mousePosition)) timelineDragging = true;
                    break;

                case EventType.MouseUp:
                    timelineDragging = false;
                    break;
            }


            var isTimelineDraggable = timelineDragging && animatorWindow.CanvasWindow.EditingHandle == HandleTypes.None && eventCurrent.button == 0;
            if (isTimelineDraggable){
                switch (eventCurrent.type)
                {
                    case EventType.MouseDown:
                        windowRect.yMin += 5;
                        break;
                    case EventType.MouseDrag:

                        windowRect.yMin = eventCurrent.mousePosition.y;
                        break;
                }
            }

        }

        private void DrawTimeline(Event eventCurrent){
            var selectedAnim = animatorWindow.SelectedAnim;

            columnLayout = new Rect(frontRect.xMax + 15, 0, 3, windowRect.height);

            EditorGUI.DrawRect(new Rect(0, 0, windowRect.width, windowRect.height), new Color(0.1f, 0.1f, 0.1f, 1));

            EditorGUI.DrawRect(new Rect(columnLayout.xMax, rowLayout.yMax, windowRect.width, windowRect.height),
                new Color(0.16f, 0.17f, 0.19f)
            );

            rowLayout = new Rect(0, frameBlank.y, windowRect.width, 4);

            EditorGUI.DrawRect(rowLayout, Color.black);
            EditorGUI.DrawRect(columnLayout, Color.black);


            DrawTimelineButtons();
            // SetPlayKeys();
            if (animatorWindow.TargetAnimation == null) return;
            if (selectedAnim.GetSpriteList().Count > 0) {
                SetSpriteThumbnail();
                DrawSelectedFrame();
                if (selectedAnim.Groups.Count > 0) {
                    CreateGroupButtons();
                    if (selectedAnim.Groups[animatorWindow.ActiveGroupIndex].layers.Count > 0) {
                        if(!isPlaying)SetFrameRect();
                        
                    }       
                } 
            }
            DrawTimelineHandle(eventCurrent);
            // SetGroupKeys();
        }


        private void DrawTimelineHandle(Event eventCurrent)
        {
            const float HandleHeight = 5;

            handleRect = new Rect(0, 0, windowRect.width, HandleHeight);
            

            EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeVertical);
            EditorGUI.DrawRect(handleRect, Color.black);
            switch (eventCurrent.type)
            {
                // drag timeline
                case EventType.MouseDrag:
                    if (handleRect.Contains(eventCurrent.mousePosition)) timelineDragging = true;
                    animatorWindow.Repaint();
                    break;
                case EventType.MouseUp:
                    timelineDragging = false;
                    break;
            }
        }

        private void DrawTimelineButtons(){
            var selectedAnim = animatorWindow.SelectedAnim; 
            using (new GUILayout.HorizontalScope()) {

                if (PixelAnimatorUtility.Button(timelineBurgerTex, onMouseTimelineBurgerTex, burgerRect)){
                    timelinePopup?.ShowAsContext();
                } 

                else if (selectedAnim != null && selectedAnim.GetSpriteList().Count > 0) {

                    if (PixelAnimatorUtility.Button(backTex, backRect)) {
                        if (selectedAnim == null) return;
                        switch (animatorWindow.ActiveFrameIndex) {
                            case 0:
                                animatorWindow.SetActiveFrame(selectedAnim.GetSpriteList().Count - 1);
                                break;
                            case > 0:
                                animatorWindow.SetActiveFrame(animatorWindow.ActiveFrameIndex - 1);
                                break;
                        }

                        animatorWindow.Repaint();
                    }

                    else if (PixelAnimatorUtility.Button(durationTex, playRect)) {
                        if (selectedAnim == null) return;
                        isPlaying = !isPlaying;
                        if (isPlaying && animatorWindow.SelectedAnim.fps == 0) Debug.Log("Frame rate is zero");
                        animatorWindow.Repaint();
                    }
                    else if (PixelAnimatorUtility.Button(frontTex, frontRect)) {
                        if (selectedAnim == null) return;
                        animatorWindow.SetActiveFrame((animatorWindow.ActiveFrameIndex + 1) % animatorWindow.SelectedAnim.GetSpriteList().Count);
                        animatorWindow.Repaint();
                    }
                }

            }

            animatorWindow.Repaint();

            durationTex = !isPlaying ? playTex : stopTex;
        }

        private void SetBurgerMenu(){
            timelinePopup = new GenericMenu{ allowDuplicateNames = true };

            timelinePopup.AddItem(new GUIContent("Go to Preferences"), false, TargetPreferences);
            timelinePopup.AddItem(new GUIContent("Update Animation"), false, UpdateAnimation);

            timelinePopup.AddSeparator("");
            var boxData = animatorWindow.Preferences.BoxData;

            if (animatorWindow.SelectedAnim == null) return;

            for (var i = 0; i < animatorWindow.Preferences.BoxData.Count; i++) {
                timelinePopup.AddItem(new GUIContent(boxData[i].boxType), false, AddGroup, boxData[i]);
            }

        }

        private void AddGroup(object userData){
            
            animatorWindow.TargetAnimation.Update();
            var data = (BoxData)userData;

            if (animatorWindow.SelectedAnim.Groups.Any(x => x.BoxDataGuid == data.Guid)) {
                Debug.LogError("This boxData has already been added! Please add another boxData.");
                return;
            }

            animatorWindow.SelectedAnim.AddGroup(data.Guid);
            animatorWindow.SelectedAnim.Groups[^1].AddLayer(animatorWindow.SelectedAnim.PixelSprites);
            CheckAndFixVariable();

        }

        private void TargetPreferences(){
            EditorGUIUtility.PingObject(animatorWindow.Preferences);
            Selection.activeObject = animatorWindow.Preferences;
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
        private void SetSpriteThumbnail(){
            
            var sprites = animatorWindow.SelectedAnim.GetSpriteList();

            if (columnRects.Length != sprites.Count) columnRects = new Rect[sprites.Count];
            
            for(var i = 0; i < columnRects.Length; i++){
                var sprite = sprites[i];
                var spriteTex = AssetPreview.GetAssetPreview(sprite);
                var column = columnRects[i];
                if(i > 0)
                    columnRects[i] = new Rect(columnRects[i-1].xMax + frameBlank.x, 0, 3, frameBlank.y);
                else
                    columnRects[0] = new Rect(columnLayout.xMax + frameBlank.x, 0, 3, frameBlank.y);

                EditorGUI.DrawRect(column, Color.black);

                var spriteCell = new Rect(column.xMin - frameBlank.x, column.yMin, frameBlank.x, frameBlank.x); // Setting the sprite texture rect
                var spriteWidth = sprite.rect.width < frameBlank.x ? sprite.rect.width : frameBlank.x;
                var spriteHeight = sprite.rect.height < frameBlank.x ? sprite.rect.height : frameBlank.x;
                float spriteXpos;
                float spriteYpos;
                
                if(spriteWidth != frameBlank.x){
                    spriteXpos = column.xMin - frameBlank.x * 0.5f - sprite.rect.width * 0.5f;
                }else{
                    spriteXpos = spriteCell.x;
                }
                
                if(spriteHeight != frameBlank.x){
                    spriteYpos = rowLayout.yMin - frameBlank.x * 0.5f - sprite.rect.height * 0.5f;
                }else{
                    spriteYpos = spriteCell.y;
                }

                var spriteTexRect = new Rect(spriteXpos, spriteYpos, spriteWidth, spriteHeight); //Texture rect
                if(spriteTex != null)
                    GUI.DrawTexture(spriteTexRect, spriteTex);

                if(spriteCell.Contains(Event.current.mousePosition) && animatorWindow.LeftClicked){
                    animatorWindow.SetActiveFrame(i);
                    animatorWindow.PropertyFocus = PropertyFocusEnum.Sprite;
                }

                if (i != animatorWindow.ActiveFrameIndex) continue;
                var height = windowRect.height +20;
                var transparentRect = new Rect(column.xMin - frameBlank.x, 0, frameBlank.x, height); //Setting the transparent rect.
                EditorGUI.DrawRect(transparentRect, new Color(1, 1, 1, 0.1f)); 
                if(i > 0){
                    columnRects[i].height = height;
                    columnRects[i-1].height = height;
                }
                else
                    columnRects[i].height = height;


            }

            EditorGUI.DrawRect(rowLayout, Color.black);// Must be deleted

        }


        private void DrawGroup(IReadOnlyList<Group> groups){

            var width = columnLayout.xMin - rowLayout.xMin;
            const int height = 35;
            const int bottomLineHeight = 3;

            groupEditorData ??= new List<GroupEditorData>(); // null check
            var offColor = new Color(0.13f, 0.13f, 0.15f);

            
            for (var g = 0; g < groups.Count; g++) {
                var group = groups[g];
                var boxData = animatorWindow.Preferences.GetBoxData(groups[g].BoxDataGuid);

                if (g == groupEditorData.Count) groupEditorData.Add(new GroupEditorData()); //Sync with group.
                

                var groupColor = animatorWindow.ActiveGroupIndex == g ? boxData.color : boxData.color * Color.gray;

                var editorData = groupEditorData[g];
                
                var xPos = rowLayout.xMin;
                var yPos = g == 0 ? rowLayout.yMax : groupEditorData[g - 1].bottomLine.yMax;
                
                editorData.bodyRect = new Rect(xPos, yPos, width, height);
                editorData.settingsRect = new Rect(xPos, yPos, height, height);
                editorData.bottomLine = new Rect(xPos, yPos + height, columnRects[^1].xMax - xPos,
                    bottomLineHeight);
                var labelRect = new Rect(editorData.settingsRect.xMax + 5,
                    editorData.settingsRect.yMin + bottomLineHeight * 0.5f, width, 30);

                //Shortcut.
                var bodyRect = editorData.bodyRect;
                var settingsRect = editorData.settingsRect;
                var bottomLine = editorData.bottomLine;
 
                EditorGUI.DrawRect(bodyRect, offColor);
                EditorGUI.DrawRect(settingsRect, groupColor);
                
                EditorGUI.DrawRect(new Rect(settingsRect.xMax, settingsRect.y, 2, settingsRect.height), Color.black);


                var tempColor = GUI.color;
                GUI.color = animatorWindow.ActiveGroupIndex == g
                    ? new Color(0.75f, 0.75f, 0.75f)
                    : new Color(0.39f, 0.43f, 0.47f); // Changing GUI color for the text color.
                EditorGUI.LabelField(labelRect, boxData.boxType);
                GUI.color = tempColor;


                if (group.isExpanded) {
                    while(editorData.layerRects.Count < groups[g].layers.Count){
                        editorData.layerRects.Add(new Rect());
                    }
                    while(editorData.layerRects.Count > groups[g].layers.Count){
                        editorData.layerRects.RemoveAt(editorData.layerRects.Count-1);
                    }
                    var layerYPos = bodyRect.yMax + (height * editorData.layerRects.Count);
                    var layerRect = new Rect(xPos, layerYPos, width, height);
                    for(var l = 0; l < editorData.layerRects.Count; l++){
                        layerYPos = bodyRect.yMax + (height * l);
                        layerRect.y = layerYPos;
                        editorData.layerRects[l] = layerRect;
                    } 
                    
                    if(editorData.layerRects.Count > 0){
                        bottomLine.y = editorData.layerRects[^1].yMax;
                        editorData.bottomLine = bottomLine;
                    }
                    DrawLayer(editorData);
                }
                EditorGUI.DrawRect(bottomLine, Color.black);


                while(editorData.layerRects.Count > groups[g].layers.Count){
                    editorData.layerRects.RemoveAt(editorData.layerRects.Count-1);
                }
            }
        }

        private void DrawLayer(GroupEditorData editorData){
            
            const float square = 25;
            var groupIndex = groupEditorData.IndexOf(editorData);
            for(var i = 0; i < editorData.layerRects.Count; i++){
                var layerRect = editorData.layerRects[i];
                var burgerMenu = new Rect(layerRect.xMin + 5, layerRect.yMin + 7, square, square);
                var color = new Color(0.08f, 0.08f, 0.10f);
                EditorGUI.DrawRect(layerRect, color);


                var labelRect = new Rect(burgerMenu.xMax + 20, layerRect.y + layerRect.height/2 -5, 300, 10);
                var tempColor = GUI.color;
                
                GUI.color = animatorWindow.ActiveGroupIndex == groupIndex && animatorWindow.ActiveLayerIndex == i ?
                new Color(0.75f, 0.75f, 0.75f) : 
                new Color(0.39f, 0.43f, 0.47f); // Changing GUI color for the text color.
                
                EditorGUI.LabelField(labelRect, "HitBox " + (i + 1));
                GUI.color = tempColor;
                
                CreateLayerButtons(burgerMenu);
            }
            
        }

        private void CreateLayerButtons(Rect burgerMenu){
            var layerCount = animatorWindow.SelectedAnim.Groups[animatorWindow.ActiveGroupIndex].layers.Count;
            for(var i = 0; i < layerCount; i++ ){
                
                if(PixelAnimatorUtility.Button(layerBurgerMenu, burgerMenu)){
                    animatorWindow.SetActiveLayer(i);
                    layerPopup.ShowAsContext();
                } 
            }

        }

        private void CreateGroupButtons(){

            var evtCurrent = Event.current;
            var groups = animatorWindow.SelectedAnim.Groups;
            if(isPlaying) return;
            DrawGroup(groups);
            for(var g = 0; g < groups.Count; g ++){
                var gRect = groupEditorData[g];
                var group = groups[g];
                
                if (animatorWindow.LeftClicked) {
                    var groupIndex = animatorWindow.ActiveGroupIndex;
                    var layerIndex = animatorWindow.ActiveLayerIndex;
                    if(groupEditorData[g].bodyRect.Contains(evtCurrent.mousePosition) && g != groupIndex){
                        
                        animatorWindow.SetActiveGroup(g);
                        while (layerIndex > 0 &&
                               layerIndex >= groups[groupIndex].layers.Count) {
                            animatorWindow.SetActiveLayer(layerIndex--);
                        }
                    } 
                    
                    for(var l = 0; l < groupEditorData[g].layerRects.Count; l++){
                        if(groupEditorData[g].layerRects[l].Contains(evtCurrent.mousePosition)){
                            if(g != groupIndex){
                                animatorWindow.SetActiveGroup(g);
                                animatorWindow.SetActiveLayer(l);
                            }
                            else 
                                animatorWindow.SetActiveLayer(l);
                        
                        }
                    }
                    
                }
                
                if(PixelAnimatorUtility.Button(settingsTex, gRect.settingsRect)){  // Opening settings popup
                    settingsPopup.ShowAsContext();
                    animatorWindow.SetActiveGroup(g); 
                }
                else if(gRect.bodyRect.Contains(evtCurrent.mousePosition) && animatorWindow.RightClicked){  
                    settingsPopup.ShowAsContext();
                    animatorWindow.SetActiveGroup(g);
                }
                var foldoutTex = group.isExpanded ? openTex : closeTex;
                if (PixelAnimatorUtility.Button(foldoutTex, gRect.FoldoutRect)) {
                    group.isExpanded = !group.isExpanded;
                }

                var isVisibleTex = group.isVisible ? visibleTex : unvisibleTex;
                if (PixelAnimatorUtility.Button(isVisibleTex, gRect.VisibleRect)) {
                    group.isVisible = !group.isVisible;
                }
                
                var isTriggerTex = group.colliderTypes == ColliderTypes.NoTrigger ? noTriggerTex : triggerTex;
                if(PixelAnimatorUtility.Button(isTriggerTex, gRect.ColliderTypeRect)){
                    group.colliderTypes = (int)group.colliderTypes == 1 ?(ColliderTypes) 2 :(ColliderTypes) 1;
                }


            }
        }

        private void SetGroupMenu(){
            settingsPopup = new GenericMenu{ allowDuplicateNames = true };
            settingsPopup.AddItem(new GUIContent("Settings/Delete"), false, DeleteGroup);
        }

        private void SetLayerMenu(){
            layerPopup = new GenericMenu{allowDuplicateNames = true};
            layerPopup.AddItem(new GUIContent("Add Layer Below"), false, AddBelowLayer);
            layerPopup.AddItem(new GUIContent("Delete Layer"), false, DeleteLayer);
        }

        private void DeleteGroup(){
            
            animatorWindow.TargetAnimation.Update();
            animatorWindow.TargetAnimation.FindProperty("groups").DeleteArrayElementAtIndex(animatorWindow.ActiveGroupIndex);
            animatorWindow.TargetAnimation.ApplyModifiedProperties();
            CheckAndFixVariable();
        }

        private void DeleteLayer(){
            var propGroups = animatorWindow.TargetAnimation.FindProperty("groups");
            var propLayers = propGroups.GetArrayElementAtIndex(animatorWindow.ActiveGroupIndex).FindPropertyRelative("layers");
            propLayers.DeleteArrayElementAtIndex(animatorWindow.ActiveLayerIndex);
            animatorWindow.TargetAnimation.ApplyModifiedProperties();
            CheckAndFixVariable();
        }

        private void AddBelowLayer(){
            var groupsProp = animatorWindow.TargetAnimation.FindProperty("groups");
            var layersProp = groupsProp.GetArrayElementAtIndex(animatorWindow.ActiveGroupIndex).FindPropertyRelative("layers");
            PixelAnimationEditor.AddLayer(layersProp, animatorWindow.TargetAnimation);
            CheckAndFixVariable();
        }
        private void CheckAndFixVariable(){
            var selectedAnim = animatorWindow.SelectedAnim;
            if(selectedAnim == null || selectedAnim.Groups == null) return;
            var groupIndex = animatorWindow.ActiveGroupIndex;
            var frameIndex = animatorWindow.ActiveFrameIndex;
            var layerIndex = animatorWindow.ActiveLayerIndex;
            if (selectedAnim.Groups != null && groupIndex >= selectedAnim.Groups.Count || groupIndex < 0) {
                animatorWindow.SetActiveGroup(0);   
            }
            
            if (frameIndex >= selectedAnim.GetSpriteList().Count || frameIndex < 0) {
                animatorWindow.SetActiveFrame(0);
            }
            if(selectedAnim.Groups.Count <= 0) return;
            if(layerIndex >= selectedAnim.Groups[groupIndex].layers.Count){
                animatorWindow.SetActiveLayer(0);
            }
        }

        private void SetFrameRect(){
            var selectedAnim = animatorWindow.SelectedAnim;

            if (groupEditorData.Count != selectedAnim.Groups.Count) return;
            var evtCurr = Event.current;
            var groups = selectedAnim.Groups;
            const int size = 16;

            var columnWidth = columnRects[0].xMin - columnLayout.xMax; //Size of Fields of frames 
            var columnHeight =  groupEditorData[0].bodyRect.yMax - rowLayout.yMax;

            //maybe I'll use selectedFrameRect or use selected frame rect as a parameter
            for (var g = 0; g < groups.Count; g++) {

                var group = animatorWindow.SelectedAnim.Groups[g];
                var editorData = groupEditorData[g];
                if(!group.isExpanded) continue;
                if (group.layers.Count != editorData.layerRects.Count) return;
                for (var l = 0; l < group.layers.Count; l++) {
                    var layer = group.layers[l];
                    var layerRect = editorData.layerRects[l];
                    var frames = layer.frames;

                    var yFramePos = layerRect.yMin + columnHeight/2 - size/2;
                    SetFrameVariable(frames);

                    for(var f = 0; f < frames.Count; f++) {
                        var frame = layer.frames[f];
                        var column = columnRects[f];
                        var cell = new Rect(column.xMin - columnWidth, layerRect.yMin, columnWidth, columnHeight);//frame cell

                        var xFramePos = cell.xMin + columnWidth/2 - size/2;
                        var frameRect = new Rect(xFramePos , yFramePos, size, size);

                        switch (frame.frameType) {
                            case FrameType.KeyFrame:
                                GUI.DrawTexture(frameRect, keyFrameTex);
                                break;
                            case FrameType.CopyFrame:
                                GUI.DrawTexture(frameRect, singleFrameTex);
                                break;
                            case FrameType.EmptyFrame:
                                GUI.DrawTexture(frameRect, emptyFrameTex);
                            break;
                        }

                        if (f != 0) {
                            var prevFrameRect = new Rect(columnRects[f - 1].xMin - columnWidth / 2 - size / 2, yFramePos, size, size);
                            SetFrameType(frameRect, layer.frames, f, prevFrameRect,evtCurr);
                        }else if(layer.frames.Count > 1){
                            if(frameRect.Contains(evtCurr.mousePosition) && animatorWindow.LeftClicked){
                                frame.frameType = frame.frameType == FrameType.KeyFrame ? FrameType.EmptyFrame : FrameType.KeyFrame;
                                if(frame.frameType == FrameType.EmptyFrame  && layer.frames[f+1].frameType == FrameType.CopyFrame)
                                    layer.frames[f+1].frameType = FrameType.KeyFrame;
                            }
                        }
                        
                        if(cell.Contains(evtCurr.mousePosition) && animatorWindow.LeftClicked ){
                            animatorWindow.SetActiveLayer(g);
                            animatorWindow.SetActiveLayer(l);
                            animatorWindow.SetActiveFrame(f);
                            animatorWindow.PropertyFocus = PropertyFocusEnum.HitBox;
                        }

                    }
                }
                
            }
        }

        private void SetFrameType(Rect frameRect, IReadOnlyList<Frame> frames, int frameIndex, Rect prevRect, Event evtCurr ){
            
            var frame = frames[frameIndex];
            var prevIndex = frameIndex-1;
            
            if (frame.frameType == FrameType.CopyFrame && frames[prevIndex].frameType != FrameType.EmptyFrame) {
                var linkedRect = new Rect(prevRect.xMax - 5, prevRect.yMin, frameRect.xMin - prevRect.xMax+10 , frameRect.height);
                GUI.DrawTexture(linkedRect, linkedFrameTex);
            }

            if (frameRect.Contains(evtCurr.mousePosition) && animatorWindow.LeftClicked) {
                if(animatorWindow.ActiveFrameIndex != frameIndex){
                    animatorWindow.SetActiveFrame(frameIndex);
                    return;
                }
                
                switch (frame.frameType) {
                    case FrameType.KeyFrame :
                        if(frames[frameIndex-1].frameType != FrameType.EmptyFrame){
                            frames[frameIndex].frameType = FrameType.CopyFrame;
                            for(var i = frameIndex-1; i > 0; i--){
                                if(frames[i].frameType == FrameType.KeyFrame){
                                    break;
                                }
                                frames[i].frameType = FrameType.CopyFrame;
                            }
                        }else{
                            frame.frameType = FrameType.EmptyFrame;
                        }

                        break;
                    case FrameType.CopyFrame:
                        frame.frameType = FrameType.EmptyFrame;
                        
                        if(frameIndex != frames.Count-1){
                            
                            if(frames[frameIndex+1].frameType == FrameType.CopyFrame)
                                frames[frameIndex+1].frameType = FrameType.KeyFrame; //(If exist)The copy frame on the left is converted to the key frame
                            else{
                                for(var i = frameIndex-1; i>0; i--){
                                    if(frames[i].frameType != FrameType.KeyFrame){
                                        frames[i].frameType = FrameType.EmptyFrame;
                                    }else{
                                        break;
                                    }
                                }           
                            }
                        }
                        break;
                    case FrameType.EmptyFrame:
                        frame.frameType = FrameType.KeyFrame;
                        break;

                }
            
            
            }
    
        }

        private void SetFrameVariable(List<Frame> frames){
            var frameIndex = animatorWindow.ActiveFrameIndex;
            var frame = frames[frameIndex];

            for(var i = frameIndex; i >= 0; i--){
                frames[i].hitBoxRect = frame.hitBoxRect;
                
                if(frames[i].frameType != FrameType.CopyFrame) break;
                
            }
            for(var i = frameIndex+1; i < frames.Count; i++){
                if(frames[i].frameType != FrameType.CopyFrame) break;

                frames[i].hitBoxRect = frame.hitBoxRect;
            }
        }
        
        //Drawing the selected frame or sprite corner.
        private void DrawSelectedFrame(){
            var selectedAnim = animatorWindow.SelectedAnim;
            
            int size;
            
            var topLeft = new Rect();
            var topRight = new Rect();
            var bottomLeft = new Rect();
            var bottomRight = new Rect();
            if (columnRects.Length != selectedAnim.PixelSprites.Count) columnRects = new Rect[selectedAnim.PixelSprites.Count];
            var column = columnRects[animatorWindow.ActiveFrameIndex];
            
            var groupIndex = animatorWindow.ActiveGroupIndex;
            var layerIndex = animatorWindow.ActiveLayerIndex;

            switch (animatorWindow.PropertyFocus) {
                case PropertyFocusEnum.HitBox:
                    if(selectedAnim.Groups.Count <= 0) return;
                    if (!selectedAnim.Groups[groupIndex].isExpanded) return;
                    if (selectedAnim.Groups[groupIndex].layers.Count <= 0) break;
                    if(groupEditorData.Count-1 < groupIndex) break;
                    if (groupEditorData[groupIndex].layerRects.Count - 1 < layerIndex) return;
                    var layerRect = groupEditorData[groupIndex].layerRects[layerIndex];
                    size = 12;
                    topLeft = new Rect(column.xMin - frameBlank.x, layerRect.yMin, size, size);
                    topRight = new Rect(column.xMin - size, layerRect.yMin, size, size);
                    bottomLeft = new Rect(column.xMin - frameBlank.x, layerRect.yMax - size, size,
                        size);
                    bottomRight = new Rect(column.xMin - size, layerRect.yMax - size, size, size);

                    break;
                case PropertyFocusEnum.Sprite:
                    size = 10;
                    var leftX = column.x - frameBlank.x;
                    var rightX = column.x - size;
                    topLeft = new Rect(leftX, column.yMin, size, size);
                    topRight = new Rect(rightX, column.yMin, size, size);
                    bottomLeft = new Rect(leftX, rowLayout.y - size, size, size);
                    bottomRight = new Rect(rightX, rowLayout.y - size, size, size);
                    break;
            }

            GUI.DrawTexture(topLeft, Resources.Load<Texture2D>("Sprites/Top Left"));
            GUI.DrawTexture(topRight, Resources.Load<Texture2D>("Sprites/Top Right"));
            GUI.DrawTexture(bottomLeft, Resources.Load<Texture2D>("Sprites/Bottom Left"));
            GUI.DrawTexture(bottomRight, Resources.Load<Texture2D>("Sprites/Bottom Right"));

        }

        private void SetFrameCopyPaste(){
            if (animatorWindow.WindowFocus != WindowFocusEnum.TimeLine || animatorWindow.PropertyFocus != PropertyFocusEnum.HitBox) return;
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

        public void ReloadVariable(int columnCount){
            columnRects = new Rect[columnCount];
            groupEditorData = new List<GroupEditorData>();
        }

    }
}