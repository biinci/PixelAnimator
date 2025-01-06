using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using binc.PixelAnimator.Utility;
using Object = UnityEngine.Object;


namespace binc.PixelAnimator.Editor{
    
    [CustomEditor(typeof(PixelAnimation))]
    public class PixelAnimationEditor : UnityEditor.Editor{

        private PixelAnimation pixelAnimation;
        private ReorderableList pixelSpriteList;
        [SerializeField] private bool pixelSpriteFoldout;
        private Rect lastRect;
        private SerializedProperty groupProps;
        private float lineHeight;
        private float lineHeightSpace;
        [SerializeField] private bool showDetails;

        private void OnEnable(){
            pixelAnimation = target as PixelAnimation;
            InitPixelSpriteList();
            
        }

        private void InitPixelSpriteList(){
            lineHeight = EditorGUIUtility.singleLineHeight;
            lineHeightSpace = lineHeight + 10;
            
            pixelSpriteList = new ReorderableList(serializedObject, serializedObject.FindProperty("pixelSprites"),
                true, true, true, true){
                drawElementCallback = DrawPixelSprite,
                elementHeight = 170
            };


            groupProps = serializedObject.FindProperty("groups");

            
            pixelSpriteList.onAddCallback = reorderableList => {
                var sp = reorderableList.serializedProperty;
                var so = sp.serializedObject;
                var index = reorderableList.index < pixelSpriteList.count && reorderableList.index >= 0
                 ? reorderableList.index : pixelSpriteList.count >= 0  ? pixelSpriteList.count-1 : 0 ;
                if(index  < 0) index = 0;
                sp.InsertArrayElementAtIndex(index);
                
                so.ApplyModifiedProperties();
                so.Update();
                var element = sp.GetArrayElementAtIndex(index);

                element.FindPropertyRelative("spriteId").stringValue = GUID.Generate().ToString();
                so.ApplyModifiedProperties();
                if(pixelAnimation.Groups == null) return;
                for (var i = 0; i < groupProps.arraySize; i++) {
                    var layerProps = groupProps.GetArrayElementAtIndex(i).FindPropertyRelative("layers");
                    Add(element, layerProps, index, groupProps);
                }
                
                reorderableList.index = index;

            };
            
            pixelSpriteList.onRemoveCallback = reorderableList => {
                
                reorderableList.serializedProperty.DeleteArrayElementAtIndex(reorderableList.index);
                if(pixelAnimation.Groups == null) return;
                Remove(groupProps, reorderableList);
                if(reorderableList.index == groupProps.arraySize-1)
                    reorderableList.index -= 1;
            };
            
            pixelSpriteList.onReorderCallbackWithDetails = (_, index, newIndex) => {
                for (var i = 0; i < groupProps.arraySize; i ++) {
                    var layersProps = groupProps.GetArrayElementAtIndex(i).FindPropertyRelative("layers");
                    for (var l = 0; l < layersProps.arraySize; l ++) {
                        var frameProp = layersProps.GetArrayElementAtIndex(l).FindPropertyRelative("frames");
                        frameProp.MoveArrayElement(index, newIndex);
                    }

                }
                
            };

            pixelSpriteList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Sprites! ");
                rect.x = rect.xMax - 120;
                EditorGUI.LabelField(rect,"Show Details");
                rect.x -= 20;
                showDetails = EditorGUI.Toggle(rect, showDetails);
            };

            pixelSpriteList.elementHeightCallback = index => {
                float height = 60;
                var methodStorageProp = pixelSpriteList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("methodStorage");
                if (methodStorageProp != null)
                {
                    height += EditorGUI.GetPropertyHeight(methodStorageProp);
                }
                return height;

            };


        }

        private void DrawPixelSprite(Rect rect, int index, bool isActive, bool isFocused){
            var element = pixelSpriteList.serializedProperty.GetArrayElementAtIndex(index);
            var currentViewWidth = EditorGUIUtility.currentViewWidth;
            
            rect.width /= EditorGUIUtility.currentViewWidth;
            rect.y += 2;

            var spriteWidth = currentViewWidth > 523 ? 174f : currentViewWidth / 3; 
            var spriteIdWidth = currentViewWidth > 952 ? 238f : currentViewWidth/4;
            var spriteRect = new Rect(rect.x, rect.y, spriteWidth, EditorGUIUtility.singleLineHeight);
            var spriteIdRect = new Rect(rect.x + spriteRect.width + 20, rect.y, spriteIdWidth, EditorGUIUtility.singleLineHeight);
            var spritePreviewRect = new Rect(spriteIdRect.x + spriteIdRect.width + 40, rect.y, 48, 48);
            var methodStorageRect =new Rect(rect.x,spriteRect.y+EditorGUIUtility.singleLineHeight*1.2f, spriteIdRect.xMax-spriteRect.x, EditorGUIUtility.singleLineHeight);
            
            EditorGUI.PropertyField(
                spriteRect,
                element.FindPropertyRelative("sprite"),
                GUIContent.none
            );
            
            EditorGUI.PropertyField(
                spriteIdRect,
                element.FindPropertyRelative("spriteId"),
                GUIContent.none
            );

            EditorGUI.BeginChangeCheck();
            var sprite = (Sprite)EditorGUI.ObjectField(
                spritePreviewRect, 
                pixelAnimation.PixelSprites[index].sprite, typeof(Sprite), true
            );
            
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Changed Sprite");
                pixelAnimation.PixelSprites[index].sprite = sprite;
            }

            EditorGUI.PropertyField(methodStorageRect,element.FindPropertyRelative("methodStorage"));

            if (!showDetails) return;
            
            rect.x += 30;
            rect.y += lineHeightSpace;
                
            var propSpriteData = element.FindPropertyRelative("spriteData");
            var propSpriteDataValues = propSpriteData.FindPropertyRelative("genericData");

            var dataRect = new Rect(rect.x, rect.y, currentViewWidth / 2, 70){
                width = currentViewWidth > 768 ? 768 / 2 : currentViewWidth / 2 < 330/2? 330/2: currentViewWidth / 2 
            };
                

            EditorGUI.PropertyField(
                dataRect,
                propSpriteDataValues
            );

            
        }

        public override void OnInspectorGUI(){
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject,  "m_Script", "pixelSprites");
            
            GUILayout.Space(10);
            
            pixelSpriteFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(pixelSpriteFoldout, "Pixel Sprites");
            lastRect = GUILayoutUtility.GetLastRect();
            if(pixelSpriteFoldout) pixelSpriteList.DoLayoutList();
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            PixelAnimatorUtility.DropAreaGUI(lastRect, pixelSpriteList, obj => {
                pixelSpriteList.serializedProperty
                    .GetArrayElementAtIndex(pixelSpriteList.serializedProperty.arraySize-1)
                    .FindPropertyRelative("sprite").objectReferenceValue = obj as Object;
            });
            serializedObject.ApplyModifiedProperties();
            
        }
        


        private static void Add(SerializedProperty element, SerializedProperty layersProps, int index, SerializedProperty groupProps){
            AddFrames(element, layersProps, index);
            // AddHitBoxData(groupProps, index);
        }
        
        private static void Remove(SerializedProperty groupProps, ReorderableList reorderableList){
            RemoveFrames(groupProps, reorderableList);
            // RemoveHitBoxData(groupProps, reorderableList);
        }        
        
        private static void AddFrames(SerializedProperty element, SerializedProperty layersProps, int index){
            for (var i = 0; i < layersProps.arraySize; i ++) {
                var framesProp = layersProps.GetArrayElementAtIndex(i).FindPropertyRelative("frames");
                framesProp.InsertArrayElementAtIndex(index);
                layersProps.serializedObject.ApplyModifiedProperties();
                layersProps.serializedObject.Update();
                var frame = framesProp.GetArrayElementAtIndex(index);
                frame.FindPropertyRelative("guid").stringValue =
                    element.FindPropertyRelative("spriteId").stringValue;
                var hitBoxRectProp = frame.FindPropertyRelative("hitBoxRect");
                hitBoxRectProp.FindPropertyRelative("x").floatValue = 16;
                hitBoxRectProp.FindPropertyRelative("y").floatValue = 16;
                hitBoxRectProp.FindPropertyRelative("width").floatValue = 16;
                hitBoxRectProp.FindPropertyRelative("height").floatValue = 16;
            }
        }

        private static void RemoveFrames(SerializedProperty groupProps, ReorderableList reorderableList){
            for (var i = 0; i < groupProps.arraySize; i ++) {
                var layersProps = groupProps.GetArrayElementAtIndex(i).FindPropertyRelative("layers");
                for (var l = 0; l < layersProps.arraySize; l ++) {
                    var frameProp = layersProps.GetArrayElementAtIndex(l).FindPropertyRelative("frames");
                    frameProp.DeleteArrayElementAtIndex(reorderableList.index);
                        
                }
            }
        }





    }
}
