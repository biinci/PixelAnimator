using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.AnimationData;
using System;

namespace binc.PixelAnimator.Editor.Windows{

    [Serializable]
    public partial class TimelineWindow{



        private void DrawBackgrounds()
        {
            EditorGUI.DrawRect(handleRect, accentColor);
            EditorGUI.DrawRect(new Rect(0, handleRect.height, windowRect.width, windowRect.height), windowPlaneColor);
            EditorGUI.DrawRect(new Rect(columnRect.xMax, rowRect.yMax, windowRect.width-columnRect.xMax,windowRect.height-rowRect.yMax), darkColor);
            EditorGUI.DrawRect(new Rect(groupAreaRect.position,new Vector2(columnRect.xMin, groupAreaRect.height)), windowPlaneColor);
        }
        
        private void DrawGridLines(){
            EditorGUI.DrawRect(columnRect, insideAccentColor);
            EditorGUI.DrawRect(rowRect, insideAccentColor);
            EditorGUI.DrawRect(handleShadowRect, insideAccentColor);
        }

        private void DrawToolButtons(){
            GUILayout.BeginArea(toolPanelRect);
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Space(3);
            
            var mainMenuContent = new GUIContent(mainMenuTex, "Main Menu");
            
            if (GUILayout.Button(mainMenuContent, animatorButtonStyle)) mainMenuButton.DownClick();

            if (SelectedAnim)
            {
                if (GUILayout.Button(mainMenuTex, animatorButtonStyle)) pingAnimationButton.DownClick();
            }
            
            var lastFrame = GUILayoutUtility.GetLastRect();
            var spaceAmount = toolPanelRect.xMax - (animatorButtonStyle.fixedWidth * 5 + lastFrame.xMax + 7);

            GUILayout.Space(spaceAmount);
            
            var prevFrameContent = new GUIContent(prevFrameTex, "Previous Frame");
            var playContent = new GUIContent(playTex, "Play");
            var nextFrameContent = new GUIContent(nextFrameTex, "Next Frame");
            
            if (GUILayout.Button(prevFrameContent, animatorButtonStyle)) previousNextSpriteButton.DownClick(true);
            if (GUILayout.Button(playContent, animatorButtonStyle)) playPauseButton.DownClick();
            if (GUILayout.Button(nextFrameContent, animatorButtonStyle)) previousNextSpriteButton.DownClick(false);
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawThumbnailPanelAndFrame()
        {
            var verticalSpace = HandleHeight + handleShadowRect.height;
            var horizontalSpace = columnRect.xMax;
            EditorGUILayout.BeginVertical();
            GUILayout.Space(verticalSpace);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(horizontalSpace);
            
            scrollPosition.x = EditorGUILayout.BeginScrollView(
                scrollPosition, 
                GUI.skin.horizontalScrollbar, 
                GUIStyle.none, 
                GUILayout.Height(windowRect.height-HandleHeight)).x;
            
            EditorGUILayout.BeginHorizontal();
            if (SelectedAnim)
            {
                DrawThumbnails(SelectedAnim.GetSpriteList());
                DrawFrames();
            } 
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawThumbnails(List<Sprite> sprites)
        {
            if (sprites == null) return;
            GUILayout.BeginHorizontal(); 
            
            for(var i = 0; i < sprites.Count; i++){
                DrawThumbnail(i);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawThumbnail(int index){
            var isSelectedIndex = index == PixelAnimatorWindow.AnimatorWindow.IndexOfSelectedSprite;
            var sprite = SelectedAnim.PixelSprites[index].sprite;
            var toolTip = !sprite ? "" : sprite.name;
            if(GUILayout.Button(new GUIContent("",toolTip), GUIStyle.none,GUILayout.Width(toolBarSize.x), GUILayout.Height(toolBarSize.y))) thumbnailButton.DownClick(index);
            var rect = GUILayoutUtility.GetLastRect();

            if (Event.current.type != EventType.Repaint) return;
            
            var spriteRect = PixelAnimatorUtility.DrawSpriteThumb(rect,index+2, sprite);
                
            if (isSelectedIndex)
            {
                EditorGUI.DrawRect(spriteRect, new Color(1,1,1,0.3f));
            }
            else if (rect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(spriteRect, new Color(1,1,1,0.2f));
                PixelAnimatorWindow.AnimatorWindow.Repaint();
            }

        }
        
        private void DrawFrames()
        {
            if (!PixelAnimatorWindow.AnimatorWindow.IsValidBoxGroup()) return;
            GUILayout.BeginArea(frameAreaRect);

            EditorGUILayout.BeginScrollView(
                Vector2.up * scrollPosition.y,
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

        private void DrawFrame(BoxFrame boxFrame){
            var texture = GetFrameTexture(boxFrame.GetFrameType());
            var clicked = GUILayout.Button(new GUIContent(texture), GUIStyle.none, GUILayout.Width(toolBarSize.x), GUILayout.Height(layerStyle.fixedHeight));
            
            if (PixelAnimatorWindow.AnimatorWindow.IsFrameSelected(boxFrame) && Event.current.type == EventType.Repaint)
            {
                var rect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(rect,selectedFrameTex);
            }            
            if (clicked) frameButton.DownClick((loopGroupIndex,loopLayerIndex,loopFrameIndex));
        }
        private void DrawGroupPanel(){
            GUILayout.BeginArea(groupAreaRect);
            EditorGUILayout.BeginVertical();
            
            scrollPosition.y = EditorGUILayout.BeginScrollView(
                Vector2.up * scrollPosition.y,
                false,
                false,
                GUIStyle.none,
                GUI.skin.verticalScrollbar,
                GUIStyle.none
            ).y;

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

        private Texture2D GetFrameTexture(BoxFrameType type)
        {
            return type switch
            {
                BoxFrameType.KeyFrame => keyFrameTex,
                BoxFrameType.CopyFrame => copyFrameTex,
                BoxFrameType.EmptyFrame => emptyFrameTex,
                _ => Texture2D.redTexture
            };
        }
    }
}