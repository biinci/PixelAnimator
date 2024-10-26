using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Common;
using System;


namespace binc.PixelAnimator.Editor.Windows{

    [Serializable]
    public partial class TimelineWindow{

        public void InspectorUpdate(){
            anim = PixelAnimatorWindow.AnimatorWindow.SelectedAnimation;
        }
        
        public override void ProcessWindow(){
            SetRect(); //TODO: OPTIMUM PLEASE
            ClampTimelinePosition();
            SetMouseIconState();
            SetReSizingState();
            RenderWindow();
        }

        private void ClampTimelinePosition(){
            var height = PixelAnimatorWindow.AnimatorWindow.position.height;
            if(windowRect.y < 200) windowRect.position = new Vector2(windowRect.x, 200);
            if(windowRect.y > height) windowRect.position = new Vector2(windowRect.x,400);
        }

        public override void FocusFunctions(){
            if(!SelectedAnim) return;
            SetShortcuts();
        }


        #region Timeline
        private void RenderWindow() => GUI.Window(Id, windowRect, _ => RenderWindowContent(), GUIContent.none, timelineStyle);
        

        private Vector2 scrollPosition;

        private void RenderWindowContent(){
            EditorGUI.DrawRect(handleRect, Color.black);
            EditorGUI.DrawRect(new Rect(0, handleRect.height, windowRect.width, windowRect.height), WindowPlaneColor);
            
            EditorGUI.DrawRect(new Rect(columnRect.xMax, rowRect.yMax, windowRect.width-columnRect.xMax,windowRect.height-rowRect.yMax), new Color(0.04f,0.04f,0.04f));
            EditorGUI.DrawRect(new Rect(groupPlaneRect.position,new Vector2(columnRect.xMin, groupPlaneRect.height)), new Color(0.07f, 0.07f, 0.07f));
            LoadStyles();

            DrawGridLines();
            DrawToolButtons();

            DrawGroupPanel();
            DrawThumbnailPanel();


            if(isPlaying) Play();
        }

        private void DrawGridLines(){
            EditorGUI.DrawRect(columnRect, Color.black);
            EditorGUI.DrawRect(rowRect, Color.black);
        }

        private float spaceTool = 110;
        private void DrawToolButtons(){
            GUILayout.BeginArea(toolPanelRect);
            GUILayout.BeginHorizontal();
           
            GUILayout.Button(timelineBurgerTex, animatorButtonStyle);
           
            GUILayout.Space(spaceTool);

            if (GUILayout.Button(previousFrameTex, animatorButtonStyle))
                previousNextSpriteButton.DownClick(true);
            if (GUILayout.Button(playPauseTex, animatorButtonStyle))
                playPauseButton.DownClick();
            if (GUILayout.Button(nextFrameTex, animatorButtonStyle))
                previousNextSpriteButton.DownClick(false);
           
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawThumbnailPanel(){
            GUILayout.BeginVertical();

            GUILayout.Space(HandleHeight);

            GUILayout.BeginHorizontal();

            var space = columnRect.xMax;
            GUILayout.Space(space);
            
            scrollPosition = GUILayout.BeginScrollView(
                scrollPosition, 
                GUI.skin.horizontalScrollbar,
                GUIStyle.none,
                GUILayout.Width(windowRect.width - space), 
                GUILayout.Height(windowRect.height-HandleHeight)

                );

            GUILayout.BeginHorizontal();           

            if(anim) DrawThumbnails(anim.GetSpriteList()); 

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawThumbnails(List<Sprite> sprites){
            for(var i = 0; i < sprites.Count; i++){
                var clicked = DrawThumbnail($"{i+1}",i);
                if (clicked) thumbnailButton.DownClick(i);
            }
            
        }

        private bool DrawThumbnail(string label, int index){
            var style = GetThumbnailStyle(index);
            return GUILayout.Button(label,style);
        }
        private GUIStyle GetThumbnailStyle(int index){
            var isSameIndex = index == PixelAnimatorWindow.AnimatorWindow.IndexOfSelectedFrame;
            if(isSameIndex) return new GUIStyle(spriteThumbnailStyle){normal = spriteThumbnailStyle.hover};
            else return spriteThumbnailStyle;
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
            
            if(anim) DrawGroups(anim.Groups);
        
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
            if(clickedGroupButton) groupButton.DownClick(group);
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
            if(GUILayout.Button(label, layerStyle)) layerButton.DownClick((group,layer));
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
            var clicked = GUILayout.Button("", style);
            if (clicked) frameButton.DownClick((loopGroupIndex,loopLayerIndex,loopFrameIndex));
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
            handleRect = new Rect(0, 0, windowRect.width, HandleHeight);
            columnRect = new Rect(GroupPanelWidth, HandleHeight, ColumnWidth, windowRect.height);
            rowRect = new Rect(0, ToolPanelHeight + HandleHeight, windowRect.width, RowHeight);
            groupPlaneRect = new Rect(0, rowRect.yMax, windowRect.width, windowRect.height - rowRect.yMax);
            thumbnailPlaneRect = new Rect(columnRect.xMax, HandleHeight, windowRect.width - columnRect.xMax,ToolPanelHeight);
            toolPanelRect = new Rect(10,8+HandleHeight, columnRect.x, ToolPanelHeight);
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
                var deltaTime = animatorWindow.EditorDeltaTime;
                timer += deltaTime;
                if(timer >= 1f/fps){
                    timer -= 1f/fps;
                    var frame = (  animatorWindow.IndexOfSelectedFrame +1 ) % SelectedAnim.GetSpriteList().Count;
                    animatorWindow.SelectFrame(frame);
                   
                }
                animatorWindow.Repaint();
            }else if(!isPlaying && timer != 0){
                timer = 0;
            }
        }
    }
}