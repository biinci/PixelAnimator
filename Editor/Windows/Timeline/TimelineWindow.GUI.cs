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
            var groupPanelRect = new Rect(bottomAreaRect.x, bottomAreaRect.y, columnRect.xMin,
                windowRect.width - rowRect.yMax);
            var framePanelRect = new Rect(columnRect.xMax, rowRect.yMax, thumbnailPanelRect.width, groupPanelRect.height);
            EditorGUI.DrawRect(handleRect, accentColor);
            EditorGUI.DrawRect(toolPanelRect, windowPlaneColor);
            EditorGUI.DrawRect(thumbnailPanelRect, windowPlaneColor);
            EditorGUI.DrawRect(groupPanelRect, windowPlaneColor);
            EditorGUI.DrawRect(framePanelRect, darkColor);
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
                if (GUILayout.Button(targetAnimTex, animatorButtonStyle)) pingAnimationButton.DownClick();
            }
            
            var lastFrame = GUILayoutUtility.GetLastRect();
            var spaceAmount = toolPanelRect.xMax - (animatorButtonStyle.fixedWidth * 5 + lastFrame.xMax + 7);

            GUILayout.Space(spaceAmount);

            var playPauseToolTip = IsPlaying ? "Pause" : "Play";
            
            var prevFrameContent = new GUIContent(prevFrameTex, "Previous Frame");
            var playContent = new GUIContent(playPauseTex, playPauseToolTip);
            var nextFrameContent = new GUIContent(nextFrameTex, "Next Frame");
            
            GUI.SetNextControlName("PreviousFrameButton");
            if (GUILayout.Button(prevFrameContent, animatorButtonStyle)) previousNextSpriteButton.DownClick(true);
            
            GUI.SetNextControlName("PlayButton");
            if (GUILayout.Button(playContent, animatorButtonStyle)) playPauseButton.DownClick();
            if(Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown){
                playPauseButton.DownClick();
            }
            GUI.SetNextControlName("NextFrameButton");
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
                if(!IsPlaying)DrawFrames();
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
            if(GUILayout.Button(new GUIContent("",toolTip), GUIStyle.none,GUILayout.Width(toolBarSize.x), GUILayout.Height(toolBarSize.y))) thumbnailButton.DownClick(new Tuple<int, Sprite>(index, sprite));
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
            GUILayout.BeginArea(bottomAreaRect);
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
                var boxData = animationPreferences.GetBoxData(group.BoxDataGuid);
                DrawGroup(group, boxData);
            }
        }

        private void DrawGroup(BoxGroup boxGroup, BoxData data){
            GUILayout.BeginHorizontal(GUIContent.none, groupStyle);
            
            GUILayout.Box("", groupStyle, GUILayout.Width(toolBarSize.x), GUILayout.Height(groupStyle.fixedHeight));
            
            var rect = GUILayoutUtility.GetLastRect();
            var colorRect = new Rect(rect.position, new Vector2(toolBarSize.x - rect.height / 12, groupStyle.fixedHeight - rect.height / 12));
            var padding = (colorRect.width - animatorButtonStyle.fixedWidth) / 2;
            
            EditorGUI.DrawRect(colorRect, data.color);
            var groupBurgerRect = new Rect(colorRect.position + Vector2.one*padding, new Vector2(animatorButtonStyle.fixedWidth, animatorButtonStyle.fixedHeight));            
            if(GUI.Button(groupBurgerRect, new GUIContent(groupOptionsTex), animatorButtonStyle))
                groupButton.DownClick(boxGroup);

            GUILayout.BeginVertical();
            EditorGUILayout.Space(padding);
            GUILayout.BeginHorizontal();

            GUILayout.Label(data.boxName, PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.label);
            GUILayout.Space(25);
            
            var visibilityTex = boxGroup.isVisible ? visibleTex : invisibleTex;
            var visibilityType = boxGroup.isVisible ? "Make Invisible" : "Make Visible";
            var visibilityContent = new GUIContent(visibilityTex, visibilityType);
            GUI.SetNextControlName("VisibilityButton " + $"{boxGroup.BoxDataGuid}");
            if(GUILayout.Button(visibilityContent,animatorButtonStyle))
                visibilityButton.DownClick(boxGroup);
            
            var collisionTex = boxGroup.collisionTypes == CollisionTypes.Collider ? colliderTex : triggerTex;
            var collisionType = boxGroup.collisionTypes == CollisionTypes.Collider ? "Collider" : "Trigger";
            var collisionContent = new GUIContent(collisionTex, collisionType);
            GUI.SetNextControlName("ColliderButton " + $"{boxGroup.BoxDataGuid}");
            if (GUILayout.Button(collisionContent, animatorButtonStyle))
                colliderButton.DownClick(boxGroup);
            
            var expandableTex = boxGroup.isExpanded ? downTex : upTex;
            var expandableType = boxGroup.isExpanded ? "Collapse Group" : "Expand Group";
            var expandableContent = new GUIContent(expandableTex, expandableType);
            GUI.SetNextControlName("ExpandGroupButton " + $"{boxGroup.BoxDataGuid}");
            if(GUILayout.Button(expandableContent,animatorButtonStyle))
                expandGroupButton.DownClick(boxGroup);
            
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            
            if(!boxGroup.isExpanded) return;
            var layers = boxGroup.boxes;
            DrawBoxes(boxGroup, layers);
                
        }

        private void DrawBoxes(BoxGroup boxGroup, List<Box> boxes){
            for(var i = 0; i < boxes.Count; i++){
                DrawBox(boxes[i], $"Box {i+1}", boxGroup);
            }
        }

        private void DrawBox(Box box, string label, BoxGroup boxGroup){
            GUILayout.BeginHorizontal(GUIContent.none, layerStyle);
            GUILayout.Label(label, PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.label, GUILayout.Width(50));
            var serializedRect = PixelAnimatorWindow.AnimatorWindow.SerializedSelectedAnimation
                .FindProperty("boxGroups")
                .GetArrayElementAtIndex(PixelAnimatorWindow.AnimatorWindow.IndexOfSelectedBoxGroup)
                .FindPropertyRelative("boxes")
                .GetArrayElementAtIndex(PixelAnimatorWindow.AnimatorWindow.IndexOfSelectedBox)
                .FindPropertyRelative("frames")
                .GetArrayElementAtIndex(PixelAnimatorWindow.AnimatorWindow.IndexOfSelectedSprite)
                .FindPropertyRelative("boxRect");
            var serializedX = serializedRect.FindPropertyRelative("x");
            var serializedY = serializedRect.FindPropertyRelative("y");
            var serializedWidth = serializedRect.FindPropertyRelative("width");
            var serializedHeight = serializedRect.FindPropertyRelative("height");

            var width = 35;
            EditorGUILayout.PropertyField(serializedX, GUIContent.none,GUILayout.Width(width));
            EditorGUILayout.PropertyField(serializedY, GUIContent.none, GUILayout.Width(width));
            EditorGUILayout.PropertyField(serializedWidth, GUIContent.none, GUILayout.Width(width));
            EditorGUILayout.PropertyField(serializedHeight, GUIContent.none, GUILayout.Width(width));
            
            serializedRect.serializedObject.ApplyModifiedProperties();
            GUILayout.EndHorizontal();
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