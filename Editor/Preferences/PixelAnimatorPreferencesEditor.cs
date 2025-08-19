using System;
using binc.PixelAnimator.Editor.Windows;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
 

namespace binc.PixelAnimator.Editor.Preferences{

    [CustomEditor(typeof(PixelAnimatorPreferences))]
    
    public class PixelAnimatorPreferencesEditor : UnityEditor.Editor{
    
        private ReorderableList windowScriptList;

        private SerializedProperty serializedWindowScripts, serializedWindows;

        private void OnEnable() {
            serializedWindowScripts = serializedObject.FindProperty("windowScripts");
            serializedWindows = serializedObject.FindProperty("windows");
            InitWindowScriptList();
        }

        private void InitWindowScriptList(){
            windowScriptList = new ReorderableList(serializedObject, serializedWindowScripts, true, true, true, true)
            {
                drawElementCallback = (rect, index, _, _) => DrawWindowScriptsElement(rect, index),
                onAddCallback = _ => AddWindowScriptElement(),
                onRemoveCallback = _ => RemoveWindowScriptElement(),
                onReorderCallbackWithDetails = (_, oldIndex, newIndex) => ApplyChangesWindowScriptsList(oldIndex, newIndex),
                drawHeaderCallback = rect => {EditorGUI.LabelField(rect,"Windows");}
                
            };

        }
        
        private void ApplyChangesWindowScriptsList(int oldIndex, int newIndex){
            serializedObject.Update();
            serializedWindowScripts.MoveArrayElement(oldIndex, newIndex);
            serializedWindows.MoveArrayElement(oldIndex, newIndex);
            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveWindowScriptElement(){
            var deleteIndex = windowScriptList.index;
            if(deleteIndex <0 || deleteIndex >= serializedWindowScripts.arraySize) deleteIndex = serializedWindowScripts.arraySize;
            serializedWindowScripts.DeleteArrayElementAtIndex(deleteIndex);
            serializedWindows.DeleteArrayElementAtIndex(deleteIndex);

            serializedObject.ApplyModifiedProperties();
        }

        private void AddWindowScriptElement(){
            var insertIndex = windowScriptList.index;
            var isIndexValid = insertIndex >=0 && insertIndex <= serializedWindowScripts.arraySize;
            if(!isIndexValid) insertIndex = serializedWindowScripts.arraySize;
            serializedWindowScripts.InsertArrayElementAtIndex(insertIndex);
            serializedWindows.InsertArrayElementAtIndex(insertIndex);
            serializedWindowScripts.GetArrayElementAtIndex(insertIndex).objectReferenceValue = null;
            serializedWindows.GetArrayElementAtIndex(insertIndex).managedReferenceValue = null;
            serializedWindowScripts.serializedObject.ApplyModifiedProperties();
        }


        private void DrawWindowScriptsElement(Rect rect, int index){
            if(serializedWindowScripts.arraySize <= index) return;
            var serializedScript = serializedWindowScripts.GetArrayElementAtIndex(index);
            var propertyRect = new Rect(rect.x,rect.y,rect.width,rect.height);
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(propertyRect, serializedScript);
            if(EditorGUI.EndChangeCheck()){  
                ApplyWindowScriptElement(index, serializedScript);
            }   
        }
        private bool ApplyWindowScriptElement(int index, SerializedProperty serializedScript){
            var script =  serializedScript.objectReferenceValue as MonoScript;
            if (script == null) return false;
            var classType = script.GetClass();
            Debug.Log(script + "    " + classType);
            if(classType == null){
                serializedScript.objectReferenceValue = null;
                return false;
            }
            var preferences = (PixelAnimatorPreferences)target;
            var isWindow = classType.BaseType == typeof(Window);
                
            var isExist = preferences.windowScripts.Contains(script);
            if(!isWindow || isExist){
                serializedScript.objectReferenceValue = null;
                return false;
            }  
            serializedWindows.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(classType);
                    
            serializedObject.ApplyModifiedProperties();
            return true;

        }

        public override void OnInspectorGUI(){
            windowScriptList.DoLayoutList();
        }


    }
}