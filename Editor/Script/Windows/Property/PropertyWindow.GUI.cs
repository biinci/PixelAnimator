using System;
using UnityEditor;
using UnityEngine;


namespace binc.PixelAnimator.Editor.Windows
{
    public partial class PropertyWindow
    {

        public override void ProcessWindow()
        {
            if (!timelineWindow.IsPlaying) DrawPropertyWindow();
        }

        private void DrawPropertyWindow()
        {
            const float ratio = 0.30063115f;
            var factor = Math.Clamp(ratio*PixelAnimatorWindow.AnimatorWindow.position.width, 250, 450);
            windowRect = new Rect(10, 10, factor, 250);
            

            GUI.Window(Id, windowRect, _ =>
            {
                EditorGUI.DrawRect(new Rect(Vector2.zero, windowRect.size), new Color(0.2f, 0.2f, 0.2f));
                selectedTab = EditorTabsAPI.DrawTabs(selectedTab, _tabTitles, factor/2f);
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
                EditorGUI.BeginChangeCheck();

                property.serializedObject.UpdateIfRequiredOrScript();

                try
                {
                    EditorGUI.PropertyField(listRect, property, GUIContent.none, true);
                }
                finally
                {
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.serializedObject.ApplyModifiedProperties();
                    }
                    GUI.EndScrollView();
                }
                

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
            var spriteIndex = animatorWindow.IndexOfSelectedSprite;
            if (targetAnimation != null && animatorWindow.IsValidSprite())
            {
                var property = targetAnimation
                    .FindProperty("pixelSprites")
                    .GetArrayElementAtIndex(spriteIndex)
                    .FindPropertyRelative("methodStorage");
                var positionRect = new Rect(0,20, windowRect.width, windowRect.height);
                var viewRect = new Rect(0, 20, windowRect.width-30, EditorGUI.GetPropertyHeight(property)+20);
                var listRect = new Rect(10,30, windowRect.width-30, windowRect.height - 30);
                    
                scrollPos = GUI.BeginScrollView(positionRect,scrollPos,viewRect,false,false);

                try
                {
                    EditorGUI.PropertyField(listRect, property, GUIContent.none, true);
                    property.serializedObject.ApplyModifiedProperties();
                }
                finally
                {
                    GUI.EndScrollView();
                }
                    
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

