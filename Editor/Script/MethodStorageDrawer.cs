using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace binc.PixelAnimator.Editor
{
    [CustomPropertyDrawer(typeof(MethodStorage))]
    public class MethodStorageDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;
        private const float PartingLineHeight = 0.1f;
        private static readonly Color PartingLineColor = new(0.14f, 0.14f, 0.14f);

        private readonly Dictionary<string, ReorderableList> reorderableListsByPropertyPath = new();
        private readonly Dictionary<string, bool> editStates = new();
        
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // try
            // {
                property.serializedObject.UpdateIfRequiredOrScript();
                var methods = property.FindPropertyRelative("methodData");
                var list = GetReorderableList(property, methods);
                
                EditorGUI.BeginDisabledGroup(GetEditState(property));
                list.DoList(position);
                EditorGUI.EndDisabledGroup();
                
                if (GUI.Button(
                        new Rect(position.x, position.y + list.GetHeight(), 40, EditorGUIUtility.singleLineHeight),
                        GetLabel(property)))
                {
                    var state = !GetEditState(property);
                    var isAdded = editStates.TryAdd(property.propertyPath, state);
                    if(!isAdded) editStates[property.propertyPath] = state;
                    
                    var storage = fieldInfo.GetValue(MethodDataDrawer.GetParent(property)) as MethodStorage;

                    
                    // Debug.Log(storage);
                    if (state)
                    {
                        for (var i = 0; i < storage.methodData.Count; i++)
                        {
                            var t = storage.methodData[i];
                            var unityAction = new UnityAction(()=>MethodUtility.GetFunction(t).Invoke());
                            // UnityEditor.Events.UnityEventTools.AddPersistentListener(storage.methods, unityAction);
                            // storage.methods.AddListener(unityAction);
                        }
                        // storage.methods.Invoke();
                    }
                    else
                    {
                        Debug.Log("removed");
                        storage.methods.RemoveAllListeners();
                    }

                }
                
                property.serializedObject.ApplyModifiedProperties();
            // }
            // catch (Exception e)
            // {
            //     Debug.LogError($"Error in MethodStorageDrawer.OnGUI: {e.Message}");
            //     EditorGUI.LabelField(position, "Error drawing property. Check console for details.");
            // }
            // finally
            // {
            //     EditorGUI.EndProperty();
            // }
            EditorGUI.EndProperty();

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
                elementHeightCallback = (index) => 
                    GetElementHeight(index, list),
                drawElementBackgroundCallback = (rect, index, active, focused) => 
                    DrawElementBackground(rect, index, active, focused, list),
                onAddCallback = AddCallback,
                onRemoveCallback = RemoveCallback
            };
                
            reorderableListsByPropertyPath[propertyPath] = list;
            return list;
        }
        
        private bool GetEditState(SerializedProperty property)
        {
            var propertyPath = property.propertyPath;
            var isFound = editStates.TryGetValue(propertyPath, out var state);
            return isFound && state;
        }

        private string GetLabel(SerializedProperty property)
        {
            var isFound = editStates.TryGetValue(property.propertyPath, out var state);
            return isFound && state ? "Edit" : "Confirm";
        }
        
        #region Draw Methods
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

            return EditorGUI.GetPropertyHeight(element) + EditorGUIUtility.singleLineHeight;
        }

        private static void DrawElementBackground(Rect rect, int index, bool active, bool focused, ReorderableList list)
        {
            var methods = list.serializedProperty;
            if (active)
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