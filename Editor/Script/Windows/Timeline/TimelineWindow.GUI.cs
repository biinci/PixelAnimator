using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Common;
using System;


namespace binc.PixelAnimator.Editor.Windows{

    [Serializable]
    public partial class TimelineWindow{

        private void RenderWindow()
        {
            windowRect.y = PixelAnimatorWindow.AnimatorWindow.position.height - windowRect.height;
            windowRect.width = PixelAnimatorWindow.AnimatorWindow.position.width;
            GUI.Window(Id, windowRect, _ => RenderWindowContent(), GUIContent.none, timelineStyle);
        }
        private Vector2 scrollPosition;
        private void RenderWindowContent()
        {
            DrawBackgrounds();//ok
            DrawGridLines();//ok
            DrawToolButtons();//ok
            DrawThumbnailPanel();
            DrawGroupPanel();

        }

        private void DrawBackgrounds()
        {
            EditorGUI.DrawRect(handleRect, AccentColor);
            EditorGUI.DrawRect(new Rect(0, handleRect.height, windowRect.width, windowRect.height), WindowPlaneColor);
            EditorGUI.DrawRect(new Rect(columnRect.xMax, rowRect.yMax, windowRect.width-columnRect.xMax,windowRect.height-rowRect.yMax), new Color(0.04f,0.04f,0.04f));
            EditorGUI.DrawRect(new Rect(groupPlaneRect.position,new Vector2(columnRect.xMin, groupPlaneRect.height)), new Color(0.07f, 0.07f, 0.07f));
        }
        
        private void DrawGridLines(){
            EditorGUI.DrawRect(columnRect, AccentColor);
            EditorGUI.DrawRect(rowRect, AccentColor);
        }

        private float spaceTool = 110;
        private void DrawToolButtons(){
            GUILayout.BeginArea(toolPanelRect);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(timelineBurgerTex, animatorButtonStyle))
            {
                burgerButton.DownClick();
            }
           
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

            if(SelectedAnim) DrawThumbnails(SelectedAnim.GetSpriteList()); 

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawThumbnails(List<Sprite> sprites){
            
            GUILayout.BeginHorizontal();
            for(var i = 0; i < sprites.Count; i++){
                var clicked = DrawThumbnail($"{i+1}",i);
                if (clicked) thumbnailButton.DownClick(i);
            }
            GUILayout.EndHorizontal();
            
            DrawFrames();
        }

        private void DrawFrames()
        {
            if (!PixelAnimatorWindow.AnimatorWindow.IsValidBoxGroup()) return;
            GUILayout.BeginArea(new Rect(0, rowRect.yMax-HandleHeight, spriteThumbnailStyle.fixedWidth*SelectedAnim.GetSpriteList().Count, groupPlaneRect.height));
            EditorGUILayout.BeginScrollView(
                Vector2.up * scrollPos.y,
                false,
                false,
                GUIStyle.none,
                GUIStyle.none,
                GUIStyle.none
            );
            
            GUILayout.BeginVertical();

            for (var i = 0; i < SelectedAnim.BoxGroups.Count; i++)
            {
                loopGroupIndex = i;
                GUILayout.Space(groupStyle.fixedHeight);
                var group = SelectedAnim.BoxGroups[i];
                GUILayout.BeginVertical();
                for (var j = 0; j < group.boxes.Count; j++)
                {
                    loopLayerIndex = j;
                    var layer = group.boxes[j];
                    GUILayout.BeginHorizontal();
                    for (var k = 0; k < layer.frames.Count; k++)
                    {
                        loopFrameIndex = k;
                        DrawFrame(layer.frames[k]);
                        
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            
            GUILayout.EndScrollView();
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private bool DrawThumbnail(string label, int index){

            var style = GetThumbnailStyle(index);
            return GUILayout.Button(label,style);
        }

        private GUIStyle hoverSpriteThumbnailStyle;
        private GUIStyle GetThumbnailStyle(int index){
            var isSameIndex = index == PixelAnimatorWindow.AnimatorWindow.IndexOfSelectedSprite;
            return isSameIndex ? hoverSpriteThumbnailStyle : spriteThumbnailStyle;
        }
        private void DrawGroupPanel(){
            GUILayout.BeginArea(groupPlaneRect);
            EditorGUILayout.BeginVertical();
            var position = EditorGUILayout.BeginScrollView(
                Vector2.up * scrollPos.y,
                false,
                false,
                GUIStyle.none,
                GUI.skin.verticalScrollbar,
                GUIStyle.none
            );

            scrollPos = Vector2.right * scrollPos.x + Vector2.up * position.y;
            
            if(SelectedAnim) DrawGroups(SelectedAnim.BoxGroups);
        
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawGroups(List<BoxGroup> groups)
        {
            var animationPreferences = PixelAnimatorWindow.AnimatorWindow.AnimationPreferences;
            foreach (var group in groups)
            {
                var name = animationPreferences.GetBoxData(group.BoxDataGuid).boxName;
                DrawGroup(group, name);
            }
        }

        private void DrawGroup(BoxGroup boxGroup, string label){
            var clickedGroupButton = GUILayout.Button(label, groupStyle);
            if(clickedGroupButton) groupButton.DownClick(boxGroup);
            if(!boxGroup.isExpanded) return;
            var layers = boxGroup.boxes;
            DrawBoxes(boxGroup, layers);
                
        }

        private void DrawBoxes(BoxGroup boxGroup, List<Box> boxes){
            for(var i = 0; i < boxes.Count; i++){
                DrawBox(boxes[i], $"Box {i+1}", boxGroup);
            }
        }

        private Rect boxRect;
        private void DrawBox(Box box, string label, BoxGroup boxGroup){
            if(GUILayout.Button(label, layerStyle)) layerButton.DownClick((boxGroup,box));
        }
        
        private float framePanelWidth;
        
        private void DrawFrame(BoxFrame boxFrame){
            var style = GetFrameStyle(boxFrame.GetFrameType());
            var clicked = GUILayout.Button("", style);

            
            if (PixelAnimatorWindow.AnimatorWindow.IsFrameSelected(boxFrame) && Event.current.type == EventType.Repaint)
            {
                var rect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(rect,selectedFrameTex);
            }            
            if (clicked) frameButton.DownClick((loopGroupIndex,loopLayerIndex,loopFrameIndex));
        }

        private GUIStyle GetFrameStyle(BoxFrameType type){
            switch (type){
                case BoxFrameType.KeyFrame:
                    return keyFrameStyle;
                case BoxFrameType.CopyFrame:
                    return copyFrameStyle;
                case BoxFrameType.EmptyFrame:
                    return emptyFrameStyle;
                default:
                    return GUIStyle.none;
            }
        }
    }
}