using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace binc.PixelAnimator.Editor
{
    [CustomPropertyDrawer(typeof(MethodData))]
    public class MethodDataDrawer : PropertyDrawer
    {
        
        private static Texture2D _functionIcon;
        private const float Padding = 5;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Event.current.type == EventType.Layout) return;

            position.y += 2;
            var instanceProperty = property.FindPropertyRelative("instance");
            var methodProperty = property.FindPropertyRelative("method");
            var parametersProperty = property.FindPropertyRelative("parameters");
            
            
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
                foldoutRect.xMax+Padding/2, 
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
                DrawInstance(objectRect, instanceProperty);
                DrawMethod(methodRect, instanceProperty, functionTexRect,functionLabelRect, content);
                DrawParameters(property, parametersProperty, position, methodProperty); 
            }
            
        }

        private void DrawInstance(Rect objectRect, SerializedProperty instanceProperty)
        {
            EditorGUI.BeginChangeCheck();
            
            EditorGUI.PropertyField(objectRect, instanceProperty, GUIContent.none);
            if (!EditorGUI.EndChangeCheck()) return;

            ResetMethod(instanceProperty);
            instanceProperty.serializedObject.ApplyModifiedProperties();
            instanceProperty.serializedObject.Update();

        }

        private void DrawMethod(Rect methodRect, SerializedProperty instanceProperty, Rect functionTexRect, Rect functionLabelRect, string content)
        {
            if (EditorGUI.DropdownButton(methodRect, GUIContent.none, FocusType.Keyboard))
            {
                SelectMethod(instanceProperty);
            }
            
            GUI.DrawTexture(functionTexRect, _functionIcon);
            GUI.Label(functionLabelRect, content);
        }

        private void DrawParameters(SerializedProperty property, SerializedProperty parametersProperty, Rect position, SerializedProperty methodProperty)
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
        
        
        #region SetData

        private static void SelectMethod(SerializedProperty instanceProperty)
        {
            
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("No function"), false, ()=>ResetMethod(instanceProperty));

            // var allMethods = typeof(Test).GetMethods(BindingFlags.Instance | BindingFlags.Public);
            var allMethods = instanceProperty.objectReferenceValue.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
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

            foreach (var VARIABLE in allMethods)
            {
                Debug.Log(VARIABLE.Name);
            }
                
            if (methods != null)
            {
                var data = (MethodData)GetParent(instanceProperty);
                foreach (var method in methods)
                {
                    menu.AddItem(new GUIContent(method.Name), false, userData =>
                    {
                        var methodInfo = userData as MethodInfo;
                        data.SelectMethod(methodInfo);
                        instanceProperty.serializedObject.Update();

                    }, method);
                }
            }

            menu.ShowAsContext();
            
        }
        
        
        private static void ResetMethod(SerializedProperty property)
        {
            if (GetParent(property) is not MethodData data)
            {
                Debug.LogWarning("MethodData is null. Cannot reset method.");
                return;
            }

            data.SelectMethod(null);

            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
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

