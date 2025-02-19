using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Object = UnityEngine.Object;
using binc.PixelAnimator.DataManipulations;

namespace binc.PixelAnimator.Editor
{
    [CustomPropertyDrawer(typeof(BaseMethodData), true)]
    public class BaseMethodDataDrawer : PropertyDrawer
    {
        private static Texture2D _functionIcon;
        private const float Padding = 0;
        private static readonly Dictionary<string, Tuple<Object, string>> ObjectListByPropertyPath = new();
        private static readonly Dictionary<string, BaseMethodData> CachedPropertyReference = new();
        private static readonly Dictionary<string, long> CachedManagedReferenceValueId = new();
        private const string NoFunctionLabel = "No function";
        private const string EmptyReferenceTip = "Reference is empty";
    
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SetPropertyReference(property);
            
            if (CachedPropertyReference[property.propertyPath] == null) return;
            _functionIcon ??= Resources.Load<Texture2D>("Sprites/function-icon");
            position.y += 2;

            var serializedMethod = property.FindPropertyRelative("method");
            var serializedParameters = property.FindPropertyRelative("parameters");
            var serializedId = property.FindPropertyRelative("globalId");
            
            var functionLabel = GetFunctionLabel(serializedMethod);
            
            var height = EditorGUIUtility.singleLineHeight;

            var foldoutRect = new Rect(position.x, position.y, 16, height);
            var restOfWidth = position.width - foldoutRect.width - Padding;
            
            var objectRect = new Rect(
                foldoutRect.xMax+Padding, 
                position.y,
                restOfWidth/2, 
                height
                );
            
            var methodRect = new Rect(
                objectRect.xMax+ Padding, 
                position.y, 
                restOfWidth/2, 
                objectRect.height
                );
            
            var functionTexRect = new Rect(
                methodRect.x + Padding, 
                methodRect.y+1, 
                _functionIcon.width,
                _functionIcon.height
                );
            
            var functionLabelRect = new Rect(
                functionTexRect.xMax, 
                functionTexRect.y, 
                methodRect.width-(functionTexRect.xMax-methodRect.x)-10,
                _functionIcon.height
                );
            
            var propertyScope = new EditorGUI.PropertyScope(position, label, property); 
            using (propertyScope)
            {
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none);
                DrawObjectReference(objectRect, serializedId, property);
                DrawMethod(methodRect, serializedId, functionTexRect, functionLabelRect, functionLabel, property);
                var showParameters = functionLabel != NoFunctionLabel && property.isExpanded;
                if(showParameters)DrawParameters(property, serializedParameters, position); 
            }

            if (ObjectListByPropertyPath.TryGetValue(serializedId.propertyPath, out var tuple) && tuple.Item2 != serializedId.stringValue)
            {
                OnObjectIdChanged(property,serializedId);
            }

            
        }
        

        #region DrawingMethods
        private void DrawObjectReference(Rect objectRect, SerializedProperty serializedId, SerializedProperty serializedMethodData)
        {
            var obj = GetUnityObject(serializedId);//TODO: performance's sake, this should be fixed
            EditorGUI.BeginChangeCheck();
            obj = EditorGUI.ObjectField(objectRect, obj, typeof(Object), true);
            if (EditorGUI.EndChangeCheck())
            {
                OnObjectReferenceChanged(serializedMethodData,serializedId, obj);   
            }
            
            var idToolTip = string.IsNullOrEmpty(serializedId.stringValue)
                ? EmptyReferenceTip
                : serializedId.stringValue;
            EditorGUI.LabelField(objectRect, new GUIContent("", idToolTip));

        }

        private void DrawMethod(Rect methodRect, SerializedProperty idProperty, Rect functionTexRect, Rect functionLabelRect, string content, SerializedProperty serializedMethodData)
        {
            if (EditorGUI.DropdownButton(methodRect, GUIContent.none, FocusType.Keyboard))
            {
                SelectMethod(serializedMethodData,idProperty);
            }
            
            GUI.DrawTexture(functionTexRect, _functionIcon);
            GUI.Label(functionLabelRect, content);
        }

        private static void DrawParameters(SerializedProperty serializedMethodData, SerializedProperty serializedParameters, Rect position)
        {
            EditorGUI.indentLevel++;
            var yPos = position.y + EditorGUIUtility.standardVerticalSpacing+EditorGUIUtility.singleLineHeight;
            var methodData = CachedPropertyReference[serializedMethodData.propertyPath];
            var isGeneric = methodData.GetType().IsGenericType;
            for (var i = 0; i < serializedParameters.arraySize; i++)
            {
                var paramProperty = serializedParameters.GetArrayElementAtIndex(i);
                var paramHeight = EditorGUI.GetPropertyHeight(paramProperty, true);
                var paramRect = new Rect(
                    position.x, 
                    yPos, 
                    position.width, 
                    paramHeight
                );
                var name = "";
                try
                {
                    name = methodData.method.methodInfo.GetParameters()[i].Name;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                if (isGeneric && i == 0)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.PropertyField(paramRect, paramProperty, new GUIContent(name),true);
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.LabelField(paramRect, new GUIContent("", "This parameter will be passing at runtime."));
                }
                else
                {
                    EditorGUI.PropertyField(paramRect, paramProperty, new GUIContent(name),true);
                }
                    
                yPos += paramHeight;
            }

            EditorGUI.indentLevel--;
        }
        
        #endregion
        
        #region SetData

        private void SetPropertyReference(SerializedProperty serializedMethodData)
        {
            var key = serializedMethodData.propertyPath;

            if (serializedMethodData.propertyType == SerializedPropertyType.ManagedReference)
            {
                if (CachedManagedReferenceValueId.TryGetValue(key, out var id))
                {
                    if (id == serializedMethodData.managedReferenceId) return;
                    CachedManagedReferenceValueId[key] = serializedMethodData.managedReferenceId;
                    CachedPropertyReference[key] = serializedMethodData.GetReference() as BaseMethodData;
                }
                else
                {
                    CachedManagedReferenceValueId[key] = serializedMethodData.managedReferenceId;
                    CachedPropertyReference[key] = serializedMethodData.GetReference() as BaseMethodData;
                }
                return;
            }
            
            
            if (CachedPropertyReference.ContainsKey(key)) return;
            CachedPropertyReference[key] = serializedMethodData.GetReference() as BaseMethodData;
        }
        
        private void SelectMethod(SerializedProperty serializedMethodData, SerializedProperty serializedID)
        {
            serializedMethodData.serializedObject.Update();
            var functionMenu = new GenericMenu();
            functionMenu.AddItem(new GUIContent("No function"), false, ()=>ResetMethod(serializedMethodData));
            var referenceValue = GetUnityObject(serializedID);

            if (referenceValue == null)
            {
                return;
            }
            
            var allMethods = referenceValue.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);

            var data = CachedPropertyReference[serializedMethodData.propertyPath];
            var methods = allMethods.Where(m=>WhereMethods(m,data.GetType())).ToArray();

            foreach (var method in methods)
            {
                functionMenu.AddItem(new GUIContent(method.Name), false, userData =>
                {
                    var methodInfo = userData as MethodInfo;
                    data.SelectMethod(methodInfo);
                    serializedMethodData.serializedObject.Update();
                }, method);

                functionMenu.ShowAsContext();
            }
        }

        private static bool WhereMethods(MethodInfo methodInfo, Type type)
        {
            var parameters = methodInfo.GetParameters();
            if (type.IsGenericType)
            {
                if (parameters.Length <= 0) return false;
                if (parameters[0].ParameterType != type.GetGenericArguments()[0]) return false;
            }
            
            return methodInfo.ReturnType == typeof(void) &&
                   !parameters.Any(p =>
                       p.ParameterType.IsGenericType || p.ParameterType.IsByRef ||
                       p.IsDefined(typeof(ParamArrayAttribute), false)) &&
                   !(methodInfo.IsSpecialName &&
                     (methodInfo.Name.StartsWith("get_") || methodInfo.Name.StartsWith("set_"))) &&
                   !methodInfo.IsGenericMethod;
        }
        
        private static void ResetMethod(SerializedProperty serializedMethodData)
        {
            if (CachedPropertyReference[serializedMethodData.propertyPath] is not { } data)
            {
                Debug.LogWarning("BaseMethodData is null. Cannot reset method.");
                return;
            }
            data.SelectMethod(null);
        }

        
        private void OnObjectReferenceChanged(SerializedProperty serializedMethodData, SerializedProperty serializedId, Object obj)
        {
            Debug.Log("object is changed");
            serializedId.stringValue = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            ObjectListByPropertyPath[serializedId.propertyPath] = new Tuple<Object, string>(obj, serializedId.stringValue);
            ResetMethod(serializedMethodData);
        }

        private void OnObjectIdChanged(SerializedProperty serializedMethodData, SerializedProperty serializedId)
        {
            Debug.Log("id is changed");
            var idString = serializedId.stringValue;
            var obj = GlobalObjectId.TryParse(idString, out var id) ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) : null;
            ObjectListByPropertyPath[serializedId.propertyPath] = new Tuple<Object, string>(obj, serializedId.stringValue);
            ResetMethod(serializedMethodData);
        }
        
        
        #endregion
        
        #region Height
        public override float GetPropertyHeight(SerializedProperty serializedMethodData, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (!serializedMethodData.isExpanded) return height;
            var serializedParameters = serializedMethodData.FindPropertyRelative("parameters");
            height += GetParametersHeight(serializedParameters);
            return height;
        }

        private static float GetParametersHeight(SerializedProperty parametersProperty)
        {
            var height = EditorGUIUtility.standardVerticalSpacing;
            
            for (var i = 0; i< parametersProperty.arraySize; i++)
            {
                var element = parametersProperty.GetArrayElementAtIndex(i);
                height += EditorGUI.GetPropertyHeight(element);
            }
            return height;                
        }
        

        #endregion

        #region GetMethods

        private static string GetFunctionLabel(SerializedProperty serializedMethod)
        {
            var methodName = serializedMethod.FindPropertyRelative("methodName").stringValue;
            return !string.IsNullOrEmpty(methodName) ? methodName : NoFunctionLabel;
        }

        private Object GetUnityObject(SerializedProperty serializedID)
        {
            var propertyPath = serializedID.propertyPath;

            var isFound = ObjectListByPropertyPath.TryGetValue(propertyPath, out var tuple);
            if (isFound) return tuple.Item1;
            var obj = GlobalObjectId.TryParse(serializedID.stringValue, out var objectId) ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(objectId) : null;
            ObjectListByPropertyPath[propertyPath] = new Tuple<Object, string>(obj, serializedID.stringValue); 
            return obj;
        }
        
        #endregion
    }
}

