using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

namespace binc.PixelAnimator.Editor
{
    [CustomPropertyDrawer(typeof(MethodStorage))]
    public class MethodStorageDrawer : PropertyDrawer
    {
        private const float Spacing = 0f;
        private const float PartingLineHeight = 0.1f;
        private static readonly Color PartingLineColor = new(0.14f, 0.14f, 0.14f);

        private readonly Dictionary<string, ReorderableList> reorderableListsByPropertyPath = new();
        
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            try
            {
                property.serializedObject.UpdateIfRequiredOrScript();
                var methods = property.FindPropertyRelative("methodData");
                var list = GetReorderableList(property, methods);
                list.DoList(position);

                property.serializedObject.ApplyModifiedProperties();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in MethodStorageDrawer.OnGUI: {e.Message}");
                EditorGUI.LabelField(position, "Error drawing property. Check console for details.");
            }
            finally
            {
                EditorGUI.EndProperty();
            }

        }
        
        private ReorderableList GetReorderableList(SerializedProperty property, SerializedProperty methods)
        {
            var propertyPath = property.propertyPath;

            var isFound = reorderableListsByPropertyPath.TryGetValue(propertyPath, out var list);
            if (isFound) return list;
            list = new ReorderableList(
                property.serializedObject, 
                methods, 
                false, 
                false, 
                true, 
                true)
            {
                drawElementCallback = (rect, index, isActive, isFocused) => 
                    DrawListElement(rect, index, isActive, isFocused, list),
                elementHeightCallback = index => 
                    GetElementHeight(index, list),
                drawElementBackgroundCallback = (rect, index, active, focused) => 
                    DrawElementBackground(rect, index, active, focused, list),
                onAddCallback = AddCallback,
                onRemoveCallback = RemoveCallback
            };
                
            reorderableListsByPropertyPath[propertyPath] = list;
            return list;
        }
        
        
        #region ReorderableList Methods
        private static void DrawListElement(Rect rect, int index, bool isActive, bool isFocused, ReorderableList list)
        {
            var methods = list.serializedProperty;
            if (methods.arraySize <= index) return;

            var element = methods.GetArrayElementAtIndex(index);
            if (element == null) return;

            rect.y += Spacing;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element
            );
            if (EditorGUI.EndChangeCheck())
            {
                
            }

            element.serializedObject.ApplyModifiedProperties();
        }

        private static float GetElementHeight(int index, ReorderableList list)
        {
            var methods = list.serializedProperty;
            if (methods.arraySize <= index) return EditorGUIUtility.singleLineHeight;

            var element = methods.GetArrayElementAtIndex(index);
            if (element == null) return EditorGUIUtility.singleLineHeight;

            return EditorGUI.GetPropertyHeight(element) + EditorGUIUtility.standardVerticalSpacing*2 + PartingLineHeight;
        }

        private static void DrawElementBackground(Rect rect, int index, bool active, bool focused, ReorderableList list)
        {
            var methods = list.serializedProperty;
            if (focused)
            {
                EditorGUI.DrawRect(rect, new Color(0.17f, 0.36f, 0.53f));
            }
            if (index == methods.arraySize - 1) return;

            var partingRect = new Rect(
                rect.x,
                rect.yMax - (EditorGUIUtility.singleLineHeight * PartingLineHeight),
                rect.width,
                EditorGUIUtility.singleLineHeight * PartingLineHeight
            );

            EditorGUI.DrawRect(partingRect, PartingLineColor);
        }

        private static void AddCallback(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.InsertArrayElementAtIndex(index);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();

        }

        private static void RemoveCallback(ReorderableList list)
        {
            if (list.index < 0) return;
            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
        }
        
        
        
        #endregion
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var methods = property.FindPropertyRelative("methodData");
            if (methods == null) return EditorGUIUtility.singleLineHeight;

            var list = GetReorderableList(property, methods);
            return EditorGUIUtility.singleLineHeight + list.GetHeight() + EditorGUIUtility.singleLineHeight*2;
        }
    }
}