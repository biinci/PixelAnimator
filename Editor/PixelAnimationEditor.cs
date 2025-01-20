using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace binc.PixelAnimator.Editor{
    
    [CustomEditor(typeof(PixelAnimation))]
    public class PixelAnimationEditor : UnityEditor.Editor
    {
        private PixelAnimation pixelAnimation;
        private ReorderableList pixelSpriteList;
        private bool pixelSpriteFoldout;
        private Rect lastRect;
        private SerializedProperty groupProps;

        private void OnEnable(){
            pixelAnimation = target as PixelAnimation;
            InitPixelSpriteList();
        }

        private void InitPixelSpriteList(){
            
            pixelSpriteList = new ReorderableList(serializedObject, serializedObject.FindProperty("pixelSprites"),
                true, false, true, true){
                drawElementCallback = DrawPixelSprite,
                elementHeight = 170
            };
            
            groupProps = serializedObject.FindProperty("boxGroups");
            
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
                if(pixelAnimation.BoxGroups == null) return;
                for (var i = 0; i < groupProps.arraySize; i++) {
                    var layerProps = groupProps.GetArrayElementAtIndex(i).FindPropertyRelative("boxes");
                    Add(element, layerProps, index, groupProps);
                }
                reorderableList.index = index;
            };
            
            pixelSpriteList.onRemoveCallback = reorderableList => {
                reorderableList.serializedProperty.DeleteArrayElementAtIndex(reorderableList.index);
                if(pixelAnimation.BoxGroups == null) return;
                Remove(groupProps, reorderableList);
                if(reorderableList.index == groupProps.arraySize-1)
                    reorderableList.index -= 1;
            };
            
            pixelSpriteList.onReorderCallbackWithDetails = (_, index, newIndex) => {
                for (var i = 0; i < groupProps.arraySize; i ++) {
                    var layersProps = groupProps.GetArrayElementAtIndex(i).FindPropertyRelative("boxes");
                    for (var l = 0; l < layersProps.arraySize; l ++) {
                        var frameProp = layersProps.GetArrayElementAtIndex(l).FindPropertyRelative("frames");
                        frameProp.MoveArrayElement(index, newIndex);
                    }
                }
            };
            
            pixelSpriteList.elementHeightCallback = index => {
                var height = EditorGUIUtility.standardVerticalSpacing*2;
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
            rect.y += 2;
            var spriteWidth = rect.width-50; 
            var spriteIdWidth = rect.width-50;
            var spriteIdRect = new Rect(rect.x+48+EditorGUIUtility.standardVerticalSpacing, rect.y+5, spriteIdWidth, EditorGUIUtility.singleLineHeight);
            var spriteRect = new Rect(spriteIdRect.x, spriteIdRect.yMax+EditorGUIUtility.standardVerticalSpacing, spriteWidth, EditorGUIUtility.singleLineHeight);
            var spritePreviewRect = new Rect(rect.x, rect.y, 48, 48);
            var methodStorageRect =new Rect(spriteIdRect.x,spritePreviewRect.yMax+EditorGUIUtility.standardVerticalSpacing*2, spriteWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginProperty(rect,GUIContent.none, element);
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

            EditorGUI.EndProperty();
        }

        public override void OnInspectorGUI(){
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject,  "m_Script", "pixelSprites");
            
            GUILayout.Space(10);
            
            pixelSpriteFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(pixelSpriteFoldout, "Pixel Sprites");
            lastRect = GUILayoutUtility.GetLastRect();
            if(pixelSpriteFoldout) pixelSpriteList.DoLayoutList();
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            PixelAnimatorUtility.DropAreaGUI(lastRect, pixelSpriteList, obj =>
            {
                var sprite = obj as Object;
                if (obj is Texture2D texture2D)
                {
                    var texturePath = AssetDatabase.GetAssetPath(texture2D);
                    var sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
                        .OfType<Sprite>()
                        .ToArray();
            
                    if (sprites.Length > 0)
                        sprite = sprites[0];
                    else
                        Debug.LogWarning("No sprites found in texture. Make sure the texture is set up as a sprite sheet.");
                }
    
                pixelSpriteList.serializedProperty
                    .GetArrayElementAtIndex(pixelSpriteList.serializedProperty.arraySize-1)
                    .FindPropertyRelative("sprite").objectReferenceValue = sprite;
            });
            serializedObject.ApplyModifiedProperties();
        }
        
        private static void Add(SerializedProperty element, SerializedProperty layersProps, int index, SerializedProperty groupProps){
            AddFrames(element, layersProps, index);
        }
        
        private static void Remove(SerializedProperty groupProps, ReorderableList reorderableList){
            RemoveFrames(groupProps, reorderableList);
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
                var hitBoxRectProp = frame.FindPropertyRelative("boxRect");
                hitBoxRectProp.FindPropertyRelative("x").floatValue = 16;
                hitBoxRectProp.FindPropertyRelative("y").floatValue = 16;
                hitBoxRectProp.FindPropertyRelative("width").floatValue = 16;
                hitBoxRectProp.FindPropertyRelative("height").floatValue = 16;
            }
        }

        private static void RemoveFrames(SerializedProperty groupProps, ReorderableList reorderableList){
            for (var i = 0; i < groupProps.arraySize; i ++) {
                var layersProps = groupProps.GetArrayElementAtIndex(i).FindPropertyRelative("boxes");
                for (var l = 0; l < layersProps.arraySize; l ++) {
                    var frameProp = layersProps.GetArrayElementAtIndex(l).FindPropertyRelative("frames");
                    frameProp.DeleteArrayElementAtIndex(reorderableList.index);
                        
                }
            }
        }
    }
}
