using System;
using UnityEditor;
using UnityEngine;

namespace binc.PixelAnimator.Editor.Windows
{
    public partial class PropertyWindow
    {
        
        private void DrawPropertyWindow()
        {
            const float ratio = 0.30063115f;
            var width = Math.Clamp(ratio*PixelAnimatorWindow.AnimatorWindow.position.width, 250, 450);
            windowRect = new Rect(10, 10, width, 250);
            GUI.Window(Id, windowRect, _ =>
            {
                EditorGUI.DrawRect(new Rect(Vector2.zero, windowRect.size), new Color(0.2f, 0.2f, 0.2f));
                
                var tempSkin = GUI.skin;
                GUI.skin = PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin;
                selectedTab = EditorTabsAPI.DrawTabs(selectedTab, _tabTitles, width/2f);
                GUI.skin = tempSkin;

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
            var targetAnimation = animatorWindow.SerializedSelectedAnimation;

            if (targetAnimation != null && animatorWindow.IsValidFrame())
            {
                var collisionType = animatorWindow.GetActiveBoxGroup().collisionTypes;
                var serializedFrame = targetAnimation
                    .FindProperty("boxGroups")
                    .GetArrayElementAtIndex(animatorWindow.IndexOfSelectedBoxGroup)
                    .FindPropertyRelative("boxes")
                    .GetArrayElementAtIndex(animatorWindow.IndexOfSelectedBox)
                    .FindPropertyRelative("frames")
                    .GetArrayElementAtIndex(animatorWindow.IndexOfSelectedSprite);
                var isCollisionTrigger = collisionType == CollisionTypes.Trigger;


                SerializedProperty enterProperty;
                SerializedProperty stayProperty;
                SerializedProperty exitProperty;
                

                if (isCollisionTrigger)
                {
                    enterProperty = serializedFrame.FindPropertyRelative("triggerEnterMethodStorage");
                    stayProperty = serializedFrame.FindPropertyRelative("triggerStayMethodStorage");
                    exitProperty = serializedFrame.FindPropertyRelative("triggerExitMethodStorage");
                }
                else
                {
                    enterProperty = serializedFrame.FindPropertyRelative("collisionEnterMethodStorage");
                    stayProperty = serializedFrame.FindPropertyRelative("collisionStayMethodStorage");
                    exitProperty = serializedFrame.FindPropertyRelative("collisionExitMethodStorage");
                }

                var enterHeight = EditorGUI.GetPropertyHeight(enterProperty);
                var stayHeight = EditorGUI.GetPropertyHeight(stayProperty);
                var exitHeight = EditorGUI.GetPropertyHeight(exitProperty);


                const int initYPos = 30;
                const float padding = 20f;

                var enterRect = new Rect(10,initYPos, windowRect.width-30, enterHeight);
                var stayRect = new Rect(10,enterRect.yMax+padding, windowRect.width-30, stayHeight);
                var exitRect = new Rect(10,stayRect.yMax+padding, windowRect.width-30, exitHeight);
                
                var positionRect = new Rect(0,30, windowRect.width-1, windowRect.height);
                var viewRect = new Rect(0, positionRect.y, windowRect.width, exitRect.yMax);
                
                serializedFrame.serializedObject.UpdateIfRequiredOrScript();
                scrollPos = GUI.BeginScrollView(positionRect,scrollPos,viewRect,false,false);
                

                EditorGUI.PropertyField(enterRect, enterProperty, GUIContent.none, true);
                EditorGUI.PropertyField(stayRect, stayProperty, GUIContent.none, true);
                EditorGUI.PropertyField(exitRect, exitProperty, GUIContent.none, true);

                serializedFrame.serializedObject.ApplyModifiedProperties();
                GUI.EndScrollView();
                
            }
            else if(targetAnimation == null)
            {
                EditorGUILayout.LabelField("Please Select an Animation", PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.label);
            }
            else
            {
                EditorGUILayout.LabelField("Please Select a BoxFrame", PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.label);
            }
        }

        private void DrawSpriteTab()
        {
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var targetAnimation = animatorWindow.SerializedSelectedAnimation;
            var spriteIndex = animatorWindow.IndexOfSelectedSprite;
            if (targetAnimation != null && animatorWindow.IsValidSprite())
            {
                targetAnimation.Update();
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
                EditorGUILayout.LabelField("Please Select an Animation", PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.label);
            }
            else
            {
                EditorGUILayout.LabelField("Please Select a Sprite", PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.label);
            }
        }
    }
}

