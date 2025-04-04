using System.Linq;
using binc.PixelAnimator.AnimationData;
using binc.PixelAnimator.DataManipulations;
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
            };
            
            groupProps = serializedObject.FindProperty("boxGroups");

            pixelSpriteList.onAddCallback = reorderableList =>
            {
                var sp = reorderableList.serializedProperty;
                var index = reorderableList.index < pixelSpriteList.count && reorderableList.index >= 0
                    ? reorderableList.index : pixelSpriteList.count >= 0  ? pixelSpriteList.count-1 : 0 ;
                AddPixelSprite(sp, index);
            };
            
            pixelSpriteList.onRemoveCallback = reorderableList => {
                RemovePixelSprite(reorderableList.serializedProperty, reorderableList.index);
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
            
            pixelSpriteList.elementHeightCallback = index =>
            {
                var height = EditorGUIUtility.standardVerticalSpacing*4+EditorGUIUtility.singleLineHeight*2;
                var serializedMethodStorage = pixelSpriteList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("methodStorage");
                if (serializedMethodStorage != null)
                {
                    height += EditorGUI.GetPropertyHeight(serializedMethodStorage);
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
            var methodStorageRect = new Rect(spriteIdRect.x,spriteIdRect.yMax+EditorGUIUtility.standardVerticalSpacing*2, spriteWidth, EditorGUIUtility.singleLineHeight);
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
            var serializedList = pixelSpriteList.serializedProperty;
            GUILayout.Space(10);
            serializedObject.ApplyModifiedProperties();

            serializedList.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(serializedList.isExpanded, "Pixel Sprites");
            lastRect = GUILayoutUtility.GetLastRect();
            if(serializedList.isExpanded) pixelSpriteList.DoLayoutList();
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
    
                serializedList
                    .GetArrayElementAtIndex(pixelSpriteList.serializedProperty.arraySize-1)
                    .FindPropertyRelative("sprite").objectReferenceValue = sprite;
            });
            serializedObject.ApplyModifiedProperties();

        }

        public static void AddPixelSprite(SerializedProperty serializedPixelSprites, int index)
        {
            var so = serializedPixelSprites.serializedObject;
            if(index  < 0) index = 0;
            serializedPixelSprites.InsertArrayElementAtIndex(index);
                
            so.ApplyModifiedProperties();
            so.Update();
            var element = serializedPixelSprites.GetArrayElementAtIndex(index);

            element.FindPropertyRelative("spriteId").stringValue = GUID.Generate().ToString();
            so.ApplyModifiedProperties();

            var serializedBoxGroups = so.FindProperty("boxGroups");
                
            for (var i = 0; i < serializedBoxGroups.arraySize; i++)
            {
                var serializedBoxGroup = serializedBoxGroups.GetArrayElementAtIndex(i);
                var collisionType =(CollisionTypes)serializedBoxGroup.FindPropertyRelative("collisionTypes").enumValueIndex;
                var serializedBoxes = serializedBoxGroup.FindPropertyRelative("boxes");
                AddFrames(element, serializedBoxes, index, collisionType);
            }
        }

        public static void RemovePixelSprite(SerializedProperty serializedPixelSprites, int index)
        {
            var serializedBoxGroups = serializedPixelSprites.serializedObject.FindProperty("boxGroups");
            serializedPixelSprites.DeleteArrayElementAtIndex(index);
            RemoveFrames(serializedBoxGroups, index);
            serializedPixelSprites.serializedObject.ApplyModifiedProperties();
        }

        private static void AddFrames(SerializedProperty element, SerializedProperty layersProps, int index, CollisionTypes collisionType){
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
                var enterMethodStorage = frame.FindPropertyRelative("enterMethodStorage");
                var stayMethodStorage = frame.FindPropertyRelative("stayMethodStorage");
                var exitMethodStorage = frame.FindPropertyRelative("exitMethodStorage");
                if (collisionType == CollisionTypes.Collider)
                {
                    enterMethodStorage.managedReferenceValue = new MethodStorage<Collision2D>();
                    stayMethodStorage.managedReferenceValue = new MethodStorage<Collision2D>();
                    exitMethodStorage.managedReferenceValue = new MethodStorage<Collision2D>();
                }
                else
                {
                    enterMethodStorage.managedReferenceValue = new MethodStorage<Collider2D>();
                    stayMethodStorage.managedReferenceValue = new MethodStorage<Collider2D>();
                    exitMethodStorage.managedReferenceValue = new MethodStorage<Collider2D>();
                }
            }
        }
        
        private static void RemoveFrames(SerializedProperty serializedBoxGroups, int index){
            for (var i = 0; i < serializedBoxGroups.arraySize; i ++) {
                var layersProps = serializedBoxGroups.GetArrayElementAtIndex(i).FindPropertyRelative("boxes");
                for (var l = 0; l < layersProps.arraySize; l ++) {
                    var frameProp = layersProps.GetArrayElementAtIndex(l).FindPropertyRelative("frames");
                    frameProp.DeleteArrayElementAtIndex(index);
                }
            }
        }



    }
}
