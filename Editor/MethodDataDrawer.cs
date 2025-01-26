using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Object = UnityEngine.Object;

namespace binc.PixelAnimator.Editor
{
    [CustomPropertyDrawer(typeof(MethodData))]
    public class MethodDataDrawer : PropertyDrawer
    {
        private static Texture2D _functionIcon;
        private const float Padding = 5;
        private readonly Dictionary<string, Object> objectListByPropertyPath = new();
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Event.current.type == EventType.Layout) return;

            position.y += 2;
            var methodProperty = property.FindPropertyRelative("method");
            var parametersProperty = property.FindPropertyRelative("parameters");
            var idProperty = property.FindPropertyRelative("globalId");
            var methodName = methodProperty?.FindPropertyRelative("methodName").stringValue;
            
            var content = "No Function";
            
            if (!string.IsNullOrEmpty(methodName)) 
            {
                content = methodName;
            }
            
            _functionIcon ??= Resources.Load<Texture2D>("Sprites/function-icon");
            
            var height = EditorGUIUtility.singleLineHeight;

            var foldoutRect = new Rect(position.x, position.y, 16, EditorGUIUtility.singleLineHeight);

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
            
            property.serializedObject.UpdateIfRequiredOrScript();
            var propertyScope = new EditorGUI.PropertyScope(position, label, property); 
            using (propertyScope)
            {
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none);
                DrawInstance(objectRect, idProperty);
                DrawMethod(methodRect, idProperty, functionTexRect,functionLabelRect, content);
                DrawParameters(property, parametersProperty, position, methodProperty); 
            }
            
        }

        #region DrawMethods
        private void DrawInstance(Rect objectRect, SerializedProperty idProperty)
        {
            EditorGUI.BeginChangeCheck();
            var obj = GetUnityObject(idProperty);//TODO: performance's sake, this should be fixed
            obj = EditorGUI.ObjectField(objectRect, obj, typeof(Object), true);
            if (!EditorGUI.EndChangeCheck()) return;
            idProperty.stringValue = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            if (!objectListByPropertyPath.TryAdd(idProperty.propertyPath, obj))
            {
                objectListByPropertyPath[idProperty.propertyPath] = obj;
            }
            ResetMethod(idProperty);
        }

        private void DrawMethod(Rect methodRect, SerializedProperty idProperty, Rect functionTexRect, Rect functionLabelRect, string content)
        {
            if (EditorGUI.DropdownButton(methodRect, GUIContent.none, FocusType.Keyboard))
            {
                SelectMethod(idProperty);
            }
            
            GUI.DrawTexture(functionTexRect, _functionIcon);
            GUI.Label(functionLabelRect, content);
        }

        private static void DrawParameters(SerializedProperty property, SerializedProperty parametersProperty, Rect position, SerializedProperty methodProperty)
        {
            if (!property.isExpanded) return;
            EditorGUI.indentLevel++;
            var parameterCount = parametersProperty.arraySize;
            var yPos = position.y + EditorGUIUtility.standardVerticalSpacing+EditorGUIUtility.singleLineHeight;
            var methodData = (MethodData)GetParent(methodProperty);
            for (var i = 0; i < parameterCount; i++)
            {
                var paramProperty = parametersProperty.GetArrayElementAtIndex(i);
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

        private void SelectMethod(SerializedProperty idProperty)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("No function"), false, ()=>ResetMethod(idProperty));
            var referenceValue = GetUnityObject(idProperty);

            MethodInfo[] allMethods;

            if (referenceValue is MonoScript monoScript)
            {
                allMethods = monoScript.GetClass().GetMethods(BindingFlags.Instance | BindingFlags.Public);
            }
            else
            {
                allMethods = referenceValue.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
            }
            
            var methods = allMethods.Where(m => 
                m.ReturnType == typeof(void) &&
                !m.GetParameters().Any(p => 
                    p.ParameterType.IsGenericType || 
                    p.ParameterType.IsByRef || 
                    p.IsDefined(typeof(ParamArrayAttribute), false)
                ) &&
                !(m.IsSpecialName && (m.Name.StartsWith("get_") || m.Name.StartsWith("set_"))) &&
                !m.IsGenericMethod
            ).ToArray();
            
            var data = (MethodData)GetParent(idProperty);
            foreach (var method in methods)
            {
                menu.AddItem(new GUIContent(method.Name), false, userData =>
                {
                    var methodInfo = userData as MethodInfo;
                    data.SelectMethod(methodInfo);
                    idProperty.serializedObject.Update();

                }, method);

                menu.ShowAsContext();
            }
        }
        
        private static void ResetMethod(SerializedProperty property)
        {
            if (GetParent(property) is not MethodData data)
            {
                Debug.LogWarning("MethodData is null. Cannot reset method.");
                return;
            }
            data.SelectMethod(null);
            
        }
        #endregion
        
        #region Height
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded) return height;
            height += GetParametersHeight(property.FindPropertyRelative("parameters"));
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
        
        #region GetValues

        private Object GetUnityObject(SerializedProperty idProperty)
        {
            var propertyPath = idProperty.propertyPath;

            var isFound = objectListByPropertyPath.TryGetValue(propertyPath, out var obj);
            if (isFound) return obj;
            var trueObject = GlobalObjectId.TryParse(idProperty.stringValue, out var objectId)
                ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(objectId)
                : null;
            objectListByPropertyPath[propertyPath] = trueObject; 
            return trueObject;
        }

        public static object GetParent(SerializedProperty prop)
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

