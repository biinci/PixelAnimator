using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using binc.PixelAnimator.AnimationData;
using binc.PixelAnimator.DataManipulations;


namespace binc.PixelAnimator.Editor
{

public static class PixelSpriteUtility
{
    public static void AddSprite(SerializedProperty spriteList, int index)
    {
        var so = spriteList.serializedObject;
        index = Mathf.Clamp(index, 0, spriteList.arraySize);
        spriteList.InsertArrayElementAtIndex(index);
        so.ApplyModifiedProperties();
        so.Update();

        var element = spriteList.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("spriteId").stringValue = GUID.Generate().ToString();

        var boxGroups = so.FindProperty("boxGroups");
        for (int i = 0; i < boxGroups.arraySize; i++)
        {
            var group = boxGroups.GetArrayElementAtIndex(i);
            var type = (CollisionTypes)group.FindPropertyRelative("collisionTypes").enumValueIndex;
            var boxes = group.FindPropertyRelative("boxes");
            AddFrames(boxes, element, index, type);
        }

        so.ApplyModifiedProperties();
    }

    public static void RemoveSprite(SerializedProperty spriteList, int index)
    {
        var so = spriteList.serializedObject;
        var boxGroups = so.FindProperty("boxGroups");

        spriteList.DeleteArrayElementAtIndex(index);
        RemoveFrames(boxGroups, index);

        so.ApplyModifiedProperties();
    }

    private static void AddFrames(SerializedProperty boxes, SerializedProperty element, int index, CollisionTypes type)
    {
        for (int i = 0; i < boxes.arraySize; i++)
        {
            var frames = boxes.GetArrayElementAtIndex(i).FindPropertyRelative("frames");
            frames.InsertArrayElementAtIndex(index);
            var frame = frames.GetArrayElementAtIndex(index);
            frame.FindPropertyRelative("guid").stringValue = element.FindPropertyRelative("spriteId").stringValue;

            var rect = frame.FindPropertyRelative("boxRect");
            rect.FindPropertyRelative("x").floatValue = 16;
            rect.FindPropertyRelative("y").floatValue = 16;
            rect.FindPropertyRelative("width").floatValue = 16;
            rect.FindPropertyRelative("height").floatValue = 16;

            var (enter, stay, exit) = (
                frame.FindPropertyRelative("enterMethodStorage"),
                frame.FindPropertyRelative("stayMethodStorage"),
                frame.FindPropertyRelative("exitMethodStorage")
            );

            if (type == CollisionTypes.Collider)
            {
                enter.managedReferenceValue = new MethodStorage<Collision2D>();
                stay.managedReferenceValue = new MethodStorage<Collision2D>();
                exit.managedReferenceValue = new MethodStorage<Collision2D>();
            }
            else
            {
                enter.managedReferenceValue = new MethodStorage<Collider2D>();
                stay.managedReferenceValue = new MethodStorage<Collider2D>();
                exit.managedReferenceValue = new MethodStorage<Collider2D>();
            }
        }
    }

    private static void RemoveFrames(SerializedProperty boxGroups, int index)
    {
        for (int i = 0; i < boxGroups.arraySize; i++)
        {
            var boxes = boxGroups.GetArrayElementAtIndex(i).FindPropertyRelative("boxes");
            for (int j = 0; j < boxes.arraySize; j++)
            {
                var frames = boxes.GetArrayElementAtIndex(j).FindPropertyRelative("frames");
                frames.DeleteArrayElementAtIndex(index);
            }
        }
    }
}

    
    
    [CustomEditor(typeof(PixelAnimation))]
    public class PixelAnimationEditor : UnityEditor.Editor
    {
        private ReorderableList pixelSpriteList;
        private SerializedProperty pixelSpritesProp;
        private SerializedProperty boxGroupsProp;
        private Rect lastRect;

        private void OnEnable()
        {
            pixelSpritesProp = serializedObject.FindProperty("pixelSprites");
            boxGroupsProp = serializedObject.FindProperty("boxGroups");
            InitPixelSpriteList();
        }

        private void InitPixelSpriteList()
        {
            pixelSpriteList = new ReorderableList(serializedObject, pixelSpritesProp, true, false, true, true)
            {
                drawElementCallback = DrawPixelSpriteElement,
                onAddCallback = list => PixelSpriteUtility.AddSprite(pixelSpritesProp, list.index),
                onRemoveCallback = list =>
                {
                    PixelSpriteUtility.RemoveSprite(pixelSpritesProp, list.index);
                    list.index = Mathf.Clamp(list.index - 1, 0, pixelSpritesProp.arraySize - 1);
                },
                onReorderCallbackWithDetails = (list, from, to) =>
                {
                    for (int i = 0; i < boxGroupsProp.arraySize; i++)
                    {
                        var boxes = boxGroupsProp.GetArrayElementAtIndex(i).FindPropertyRelative("boxes");
                        for (int j = 0; j < boxes.arraySize; j++)
                        {
                            var frames = boxes.GetArrayElementAtIndex(j).FindPropertyRelative("frames");
                            frames.MoveArrayElement(from, to);
                        }
                    }
                },
                elementHeightCallback = index =>
                {
                    var element = pixelSpritesProp.GetArrayElementAtIndex(index);
                    var methodStorage = element.FindPropertyRelative("methodStorage");
                    return 60 + EditorGUI.GetPropertyHeight(methodStorage);
                }
            };
        }

        private void DrawPixelSpriteElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = pixelSpritesProp.GetArrayElementAtIndex(index);
            rect.y += 2;

            var spritePreviewRect = new Rect(rect.x, rect.y, 48, 48);
            var spriteIdRect = new Rect(rect.x + 52, rect.y, rect.width - 52, EditorGUIUtility.singleLineHeight);
            var spriteFieldRect = new Rect(rect.x + 52, rect.y + 20, rect.width - 52, EditorGUIUtility.singleLineHeight);
            var methodStorageRect = new Rect(rect.x + 52, rect.y + 40, rect.width - 52, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(rect, GUIContent.none, element);
            EditorGUI.PropertyField(spriteIdRect, element.FindPropertyRelative("spriteId"), new GUIContent("Sprite ID"));
            EditorGUI.PropertyField(spriteFieldRect, element.FindPropertyRelative("sprite"), new GUIContent("Sprite"));
            EditorGUI.PropertyField(methodStorageRect, element.FindPropertyRelative("methodStorage"), new GUIContent("Method Storage"));
            EditorGUI.EndProperty();

            var pixelAnim = (PixelAnimation)target;
            var sprite = (Sprite)EditorGUI.ObjectField(spritePreviewRect, pixelAnim.PixelSprites[index].sprite, typeof(Sprite), true);

            if (sprite != pixelAnim.PixelSprites[index].sprite)
            {
                Undo.RecordObject(target, "Changed Sprite");
                pixelAnim.PixelSprites[index].sprite = sprite;
                EditorUtility.SetDirty(target);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script", "pixelSprites");
            GUILayout.Space(10);

            pixelSpritesProp.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(pixelSpritesProp.isExpanded, "Pixel Sprites");
            lastRect = GUILayoutUtility.GetLastRect();
            if (pixelSpritesProp.isExpanded)
            {
                pixelSpriteList.DoLayoutList();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            PixelAnimatorUtility.DropAreaGUI(lastRect, pixelSpriteList, obj =>
            {
                if (obj is Texture2D tex)
                {
                    string path = AssetDatabase.GetAssetPath(tex);
                    var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
                    if (sprites.Length > 0)
                    {
                        obj = sprites[0];
                    }
                }

                if (obj is Sprite sprite)
                {
                    int last = pixelSpritesProp.arraySize - 1;
                    pixelSpritesProp.GetArrayElementAtIndex(last).FindPropertyRelative("sprite").objectReferenceValue = sprite;
                }
            });

            serializedObject.ApplyModifiedProperties();
        }
    }
}
