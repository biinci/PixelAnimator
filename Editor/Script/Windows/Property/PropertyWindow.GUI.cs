using System;
<<<<<<< Updated upstream
using System.Linq;
using binc.PixelAnimator.Common;
using binc.PixelAnimator.DataProvider;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.Utility;
=======
>>>>>>> Stashed changes
using UnityEditor;
using UnityEngine;


namespace binc.PixelAnimator.Editor.Windows
{
    public partial class PropertyWindow
    {

        public override void ProcessWindow()
        {
<<<<<<< Updated upstream
            
            if(!timelineWindow.IsPlaying) DrawPropertyWindow();
            
        }
        
        private void DrawProperties(PropertyType propertyType, string header)
        {
            var scrollScope = new EditorGUILayout.ScrollViewScope(propertyScrollPos);

            EditorGUI.LabelField(propertyWindowRect, header, EditorStyles.boldLabel); // Drawing header

            var rect = new Rect(7, 30, 300, 2f);
            var color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            EditorGUI.DrawRect(rect, color);

            GUILayout.Space(50);

            using (scrollScope)
            {
                propertyScrollPos = scrollScope.scrollPosition;
                switch (propertyType)
                {
                    case PropertyType.Sprite:
                        DrawSpriteProp();
                        break;
                    case PropertyType.HitBox:
                        DrawHitBoxProp();
                        break;
                }
            }

        }
        

        private void DrawSpriteProp()
        {
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            
            var targetAnimation = PixelAnimatorWindow.AnimatorWindow.TargetAnimation;
            var frameIndex = animatorWindow.IndexOfSelectedSprite;

            var propPixelSprite = targetAnimation.FindProperty("pixelSprites")
                .GetArrayElementAtIndex(frameIndex);

            
            var propSpriteData = propPixelSprite.FindPropertyRelative("spriteData");
            var propSpriteEventNames = propSpriteData.FindPropertyRelative("eventNames");
            var propSpriteDataValues = propSpriteData.FindPropertyRelative("genericData");
            var spriteDataValues = animatorWindow.SelectedAnimation.PixelSprites[frameIndex]
                .SpriteData
                .genericData;

            foreach (var prop in animatorWindow.AnimationPreferences.SpriteProperties)
            {
                var single = spriteDataValues.FirstOrDefault(x => x.baseData.Guid == prop.Guid)
                    .baseData;
                var selectedIndex = single == null
                    ? -1
                    : spriteDataValues.FindIndex(x => x.baseData == single);
                SetPropertyField(prop, propSpriteDataValues, single, selectedIndex);
            }

            DrawEventField(propSpriteEventNames);
            targetAnimation.ApplyModifiedProperties();
        }

        private void DrawHitBoxProp()
        {
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var groupIndex = animatorWindow.IndexOfSelectedGroup;
            var layerIndex = animatorWindow.IndexOfSelectedLayer;
            var frameIndex = animatorWindow.IndexOfSelectedSprite;

            var targetAnimation = animatorWindow.TargetAnimation;

            if (SelectedAnim.Groups[groupIndex].layers.Count <= 0) return;
            var group = targetAnimation.FindProperty("groups").GetArrayElementAtIndex(groupIndex);
            var layer = group.FindPropertyRelative("layers").GetArrayElementAtIndex(layerIndex);
            var frames = layer.FindPropertyRelative("frames");
            var frame = frames.GetArrayElementAtIndex(frameIndex);


            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Box", GUILayout.Width(70));
                var propHitBoxRect = frame.FindPropertyRelative("hitBoxRect");
                EditorGUILayout.PropertyField(propHitBoxRect, GUIContent.none, GUILayout.Width(140),
                    GUILayout.MaxHeight(60));
            }

            targetAnimation.ApplyModifiedProperties();

            var dataProps = frame.FindPropertyRelative("hitBoxData");
            var eventNamesProp = dataProps.FindPropertyRelative("eventNames");
            var dataValuesProp = dataProps.FindPropertyRelative("genericData");

            var hitBoxDataValues = SelectedAnim.Groups[groupIndex].layers[layerIndex].frames[frameIndex].hitBoxData.genericData;
            foreach (var dataWareHouse in PixelAnimatorWindow.AnimatorWindow.AnimationPreferences.HitBoxProperties) {
                var single = hitBoxDataValues.FirstOrDefault(x => x.baseData.Guid == dataWareHouse.Guid) // is property  exist?
                    .baseData;
                var selectedIndex = single == null
                    ? -1
                    : hitBoxDataValues.FindIndex(x => x.baseData == single);
                SetPropertyField(dataWareHouse, dataValuesProp, single, selectedIndex);
            }

            DrawEventField(eventNamesProp);
        }
=======
>>>>>>> Stashed changes

            if (!timelineWindow.IsPlaying) DrawPropertyWindow();
        } 
        private void DrawPropertyWindow()
        {
<<<<<<< Updated upstream
            var tempColor = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.2f);
            windowRect = new Rect(10, 10, 250, 250); // Background rect.


            switch (PixelAnimatorWindow.AnimatorWindow.PropertyFocus)
            {
                case PropertyFocusEnum.HitBox:
                    var selectedAnim = PixelAnimatorWindow.AnimatorWindow.SelectedAnimation;
                    if (selectedAnim == null || selectedAnim.Groups == null || selectedAnim.Groups.Count == 0)
                    {
                        // PixelAnimatorWindow.AnimatorWindow.SetPropertyFocus(PropertyFocusEnum.Sprite);
                        break;
                    }

                    // if(ActiveGroupIndex >= SelectedAnim.Groups.Count) CheckAndFixVariable();
                    var groupIndex = PixelAnimatorWindow.AnimatorWindow.IndexOfSelectedGroup;
                    var layerIndex = PixelAnimatorWindow.AnimatorWindow.IndexOfSelectedLayer;
                    var frameIndex = PixelAnimatorWindow.AnimatorWindow.IndexOfSelectedSprite;
                    try
                    {
                        if (selectedAnim.Groups[groupIndex].layers[layerIndex].frames[frameIndex]
                                .GetFrameType() != FrameType.KeyFrame) break;
                        GUI.Window(4, windowRect,
                            _ => { DrawProperties(PropertyType.HitBox, "HitBox Properties"); }, GUIContent.none);
                    }
                    catch
                    {
                        PixelAnimatorWindow.AnimatorWindow.SelectGroup(0);
                    }
                    break;
                case PropertyFocusEnum.Sprite:
                    GUI.Window(4, windowRect,
                        _ => { DrawProperties(PropertyType.Sprite, "Sprite Properties"); }, GUIContent.none);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
=======
>>>>>>> Stashed changes
            
            const float ratio = 0.35063115f;
            var factor = Math.Clamp(ratio*PixelAnimatorWindow.AnimatorWindow.position.width, 250, 450);
            windowRect = new Rect(10, 10, factor, factor);
            
            
            GUI.Window(Id, windowRect, _ =>
            {
                EditorGUI.DrawRect(new Rect(Vector2.zero, windowRect.size), new Color(0.22f,0.22f,0.22f,0.4f));
                selectedTab = EditorTabsAPI.DrawTabs(selectedTab, _tabTitles);
                
                switch (selectedTab)
                {
                    case 0:
                        DrawSpriteTab();
                        break;
                    case 1:
                        DrawHitboxTab();
                        break;
                }

            }, GUIContent.none, GUIStyle.none);
            
        }

        private void DrawHitboxTab()
        {
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var targetAnimation = animatorWindow.TargetAnimation;

            if (targetAnimation != null && animatorWindow.IsValidFrame())
            {
                var property = targetAnimation
                    .FindProperty("groups")
                    .GetArrayElementAtIndex(animatorWindow.IndexOfSelectedGroup)
                    .FindPropertyRelative("layers")
                    .GetArrayElementAtIndex(animatorWindow.IndexOfSelectedLayer)
                    .FindPropertyRelative("frames")
                    .GetArrayElementAtIndex(animatorWindow.IndexOfSelectedSprite)
                    .FindPropertyRelative("methodStorage");
                var positionRect = new Rect(0,20, windowRect.width, windowRect.height);
                var viewRect = new Rect(0, 20, windowRect.width-30, EditorGUI.GetPropertyHeight(property)+20);
                var listRect = new Rect(10,30, windowRect.width-30, windowRect.height - 30);
                scrollPos = GUI.BeginScrollView(positionRect,scrollPos,viewRect,false,false);

                property.serializedObject.UpdateIfRequiredOrScript();
                EditorGUI.BeginProperty(listRect,GUIContent.none, property);
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(listRect, property, GUIContent.none, true);
                if (EditorGUI.EndChangeCheck())
                {
                    property.serializedObject.ApplyModifiedProperties();
                }
                    
                EditorGUI.EndProperty();
                GUI.EndScrollView();
            }
            else if(targetAnimation == null)
            {
                EditorGUILayout.LabelField("Please Select an Animation");
            }
            else
            {
                EditorGUILayout.LabelField("Please Select a Frame");
            }
        }

        private void DrawSpriteTab()
        {
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var targetAnimation = animatorWindow.TargetAnimation;
            if (targetAnimation != null && animatorWindow.IsValidSprite(animatorWindow.IndexOfSelectedSprite))
            {
                var property = targetAnimation.FindProperty("pixelSprites").GetArrayElementAtIndex(animatorWindow.IndexOfSelectedSprite).FindPropertyRelative("methodStorage");
                var positionRect = new Rect(0,20, windowRect.width, windowRect.height);
                var viewRect = new Rect(0, 20, windowRect.width-30, EditorGUI.GetPropertyHeight(property)+20);
                var listRect = new Rect(10,30, windowRect.width-30, windowRect.height - 30);
                    
                scrollPos = GUI.BeginScrollView(positionRect,scrollPos,viewRect,false,false);
                    
                EditorGUI.PropertyField(listRect, property, GUIContent.none, true);
                property.serializedObject.ApplyModifiedProperties();
                    
                GUI.EndScrollView();
            }
            else if(targetAnimation == null)
            {
                EditorGUILayout.LabelField("Please Select an Animation");
            }
            else
            {
                EditorGUILayout.LabelField("Please Select a Sprite");
            }

        }
        


    }
    
    
    
}

