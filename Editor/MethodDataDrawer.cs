using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Object = UnityEngine.Object;
using binc.PixelAnimator.DataManipulations;
using PlasticPipe.Server;
using UnityEditor.Profiling;

namespace binc.PixelAnimator.Editor
{



    [CustomPropertyDrawer(typeof(BaseMethodData))]
    public class BaseMethodDataDrawer : PropertyDrawer
    {
        private static Texture2D _functionIcon;
        private const float Padding = 5;
        private readonly Dictionary<string, Object> objectListByPropertyPath = new();
        private const string NoFunctionLabel = "No function";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Event.current.type == EventType.Layout) return;
            if (property.managedReferenceValue == null) return;

            _functionIcon ??= Resources.Load<Texture2D>("Sprites/function-icon");
            position.y += 2;
            
            var serializedMethod = property.FindPropertyRelative("method");
            var serializedParameters = property.FindPropertyRelative("parameters");
            var serializedId = property.FindPropertyRelative("globalId");
            
            var functionLabel = GetFunctionLabel(serializedMethod);
            
            var height = EditorGUIUtility.singleLineHeight;

            var foldoutRect = new Rect(position.x, position.y, 16, height);

            var objectRect = new Rect(
                foldoutRect.xMax+Padding/3, 
                position.y,
                position.width/2.2f, 
                height
                );
            
            var methodRect = new Rect(
                objectRect.xMax+ Padding, 
                position.y, 
                position.width/2.2f, 
                objectRect.height
                );
            
            var functionTexRect = new Rect(
                methodRect.x + Padding/2, 
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
                DrawObjectReference(objectRect, serializedId);
                DrawMethod(methodRect, serializedId, functionTexRect, functionLabelRect, functionLabel, property);
                var showParameters = functionLabel != NoFunctionLabel && property.isExpanded;
                if(showParameters)DrawParameters(property, serializedParameters, position, serializedMethod); 
            }
            
        }

        #region DrawingMethods
        private void DrawObjectReference(Rect objectRect, SerializedProperty serializedId)
        {
            var obj = GetUnityObject(serializedId);//TODO: performance's sake, this should be fixed
            EditorGUI.BeginChangeCheck();
            obj = EditorGUI.ObjectField(objectRect, obj, typeof(Object), true);
            if (EditorGUI.EndChangeCheck())
            {
                OnObjectReferenceChanged(serializedId, obj);   
            }
            EditorGUI.LabelField(objectRect, new GUIContent("", serializedId.stringValue));

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

        private static void DrawParameters(SerializedProperty serializedMethodData, SerializedProperty serializedParameters, Rect position, SerializedProperty methodProperty)
        {
            EditorGUI.indentLevel++;
            var parameterCount = serializedParameters.arraySize;
            var yPos = position.y + EditorGUIUtility.standardVerticalSpacing+EditorGUIUtility.singleLineHeight;
            var isGeneric = serializedMethodData.managedReferenceValue.GetType().IsGenericType;
            var startIndex = isGeneric ? 1 : 0;
            var methodData = (BaseMethodData)GetParent(methodProperty);
            for (var i = startIndex; i < parameterCount; i++)
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
                    name = methodData?.method.methodInfo.GetParameters()[i].Name;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                EditorGUI.PropertyField(paramRect, paramProperty, new GUIContent(name),true);
                yPos += paramHeight;
            }

            EditorGUI.indentLevel--;

        }
        
        #endregion
        
        #region SetData

        private void SelectMethod(SerializedProperty serializedMethodData,SerializedProperty serializedID)
        {
            var functionMenu = new GenericMenu();
            functionMenu.AddItem(new GUIContent("No function"), false, ()=>ResetMethod(serializedID));
            var referenceValue = GetUnityObject(serializedID);

            if (referenceValue == null)
            {
                return;
            }
            
            var allMethods = referenceValue.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
            
            var methods = allMethods.Where(m=>WhereMethods(serializedMethodData,m)).ToArray();
            
            var data = (BaseMethodData)GetParent(serializedID);
            foreach (var method in methods)
            {
                functionMenu.AddItem(new GUIContent(method.Name), false, userData =>
                {
                    var methodInfo = userData as MethodInfo;
                    data.SelectMethod(methodInfo);
                    serializedID.serializedObject.Update();

                }, method);

                functionMenu.ShowAsContext();
            }
        }

        private static bool WhereMethods(SerializedProperty serializedMethodData, MethodInfo methodInfo)
        {
            var type = serializedMethodData.managedReferenceValue.GetType();
            var parameters = methodInfo.GetParameters();
            bool methodKeeper;
            if (type.IsGenericType)
            {
                if (parameters.Length <= 0) return false;
                methodKeeper = parameters[0].ParameterType != type.GetGenericArguments()[0];
            }
            else
            {
                methodKeeper = false;
            }
            
            return methodInfo.ReturnType == typeof(void) &&
                   !parameters.Any(p =>
                       p.ParameterType.IsGenericType || p.ParameterType.IsByRef ||
                       p.IsDefined(typeof(ParamArrayAttribute), false)) &&
                   !(methodInfo.IsSpecialName &&
                     (methodInfo.Name.StartsWith("get_") || methodInfo.Name.StartsWith("set_"))) &&
                   !methodInfo.IsGenericMethod &&
                   !methodKeeper;
        }
        
        private static void ResetMethod(SerializedProperty property)
        {
            if (GetParent(property) is not BaseMethodData data)
            {
                Debug.LogWarning("BaseMethodData is null. Cannot reset method.");
                return;
            }
            data.SelectMethod(null);
        }

        private void OnObjectReferenceChanged(SerializedProperty serializedId, Object obj)
        {
            Debug.Log("object is changed");
            serializedId.stringValue = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            objectListByPropertyPath[serializedId.propertyPath] = obj;
            ResetMethod(serializedId);
        }

        private void OnObjectIdChanged(SerializedProperty serializedId)
        {
            Debug.Log("id is changed");
            var idString = serializedId.stringValue;
            var obj = GlobalObjectId.TryParse(idString, out var id) ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) : null;
            objectListByPropertyPath[serializedId.propertyPath] = obj;
            ResetMethod(serializedId);
        }
        
        
        #endregion
        
        #region Height
        public override float GetPropertyHeight(SerializedProperty serializedMethodData, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (!serializedMethodData.isExpanded || serializedMethodData.managedReferenceValue == null) return height;
            height += GetParametersHeight(serializedMethodData,serializedMethodData.FindPropertyRelative("parameters"));
            return height;
        }

        private static float GetParametersHeight(SerializedProperty serializedMethodData,SerializedProperty parametersProperty)
        {

            var height = EditorGUIUtility.standardVerticalSpacing;
            var isGeneric = serializedMethodData.managedReferenceValue.GetType().IsGenericType;
            var startIndex = isGeneric ? 1 : 0;
            for (var i = startIndex; i< parametersProperty.arraySize; i++)
            {
                var element = parametersProperty.GetArrayElementAtIndex(i);
                height += EditorGUI.GetPropertyHeight(element);
            }
            return height;                
        }
        #endregion
        
        #region GetValues

        private static string GetFunctionLabel(SerializedProperty serializedMethod)
        {
            var methodName = serializedMethod.FindPropertyRelative("methodName").stringValue;
            return !string.IsNullOrEmpty(methodName) ? methodName : NoFunctionLabel;
        }
        
        private Object GetUnityObject(SerializedProperty serializedID)
        {
            var propertyPath = serializedID.propertyPath;

            var isFound = objectListByPropertyPath.TryGetValue(propertyPath, out var obj);
            if (isFound) return obj;
            var trueObject = GlobalObjectId.TryParse(serializedID.stringValue, out var objectId)
                ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(objectId)
                : null;
            objectListByPropertyPath[propertyPath] = trueObject; 
            return trueObject;
        }

        private static object GetParent(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach(var element in elements.Take(elements.Length-1))
            {
                if(element.Contains("["))
                {
                    var elementName = element[..element.IndexOf("[", StringComparison.Ordinal)];
                    var index = Convert.ToInt32(element[element.IndexOf("[", StringComparison.Ordinal)..].Replace("[","").Replace("]",""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue(object source, string name)
        {
            if(source == null)
                return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null) return f.GetValue(source);
            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return p == null ? null : p.GetValue(source, null);
        }

        private static object GetValue(object source, string name, int index)
        {
            if (GetValue(source, name) is not IEnumerable enumerable) return null;
            var enm = enumerable.GetEnumerator();
            using var enm1 = enm as IDisposable;

            while(index-- >= 0)
                enm.MoveNext();
            return enm.Current;

        }
        #endregion
    }
}

