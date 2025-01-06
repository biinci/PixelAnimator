using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Common;
using System;


namespace binc.PixelAnimator.Editor.Windows{

    [Serializable]
    public partial class TimelineWindow{
        
        private void RenderWindow() => GUI.Window(Id, windowRect, _ => RenderWindowContent(), GUIContent.none, timelineStyle);
        
        
        private Vector2 scrollPosition;

        private void RenderWindowContent(){
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
            if (!PixelAnimatorWindow.AnimatorWindow.IsValidGroup()) return;
            //EditorGUI.DrawRect(new Rect(0, rowRect.yMax-HandleHeight, thumbnailPlaneRect.width+scrollPos.x*10, groupPlaneRect.height), new Color(0.5f,0.5f,0.5f,0.5f));
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

            for (var i = 0; i < SelectedAnim.Groups.Count; i++)
            {
                loopGroupIndex = i;
                GUILayout.Space(groupStyle.fixedHeight);
                var group = SelectedAnim.Groups[i];
                GUILayout.BeginVertical();
                for (var j = 0; j < group.layers.Count; j++)
                {
                    loopLayerIndex = j;
                    var layer = group.layers[j];
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
            
            if(SelectedAnim) DrawGroups(SelectedAnim.Groups);
        
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawGroups(List<Group> groups)
        {
            var animationPreferences = PixelAnimatorWindow.AnimatorWindow.AnimationPreferences;
            foreach (var group in groups)
            {
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
                DrawLayer(layers[i], $"Layer {i+1}", group);
            }
        }

        private Rect layerRect;
        private void DrawLayer(Layer layer, string label, Group group){
            if(GUILayout.Button(label, layerStyle)) layerButton.DownClick((group,layer));
        }
        
        private float framePanelWidth;
        
        private void DrawFrame(Frame frame){
            var style = GetFrameStyle(frame.GetFrameType());
            var clicked = GUILayout.Button("", style);

            
            if (PixelAnimatorWindow.AnimatorWindow.IsSelectedFrame(frame) && Event.current.type == EventType.Repaint)
            {
                var rect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(rect,selectedFrameTex);
            }            
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
        



    }
}