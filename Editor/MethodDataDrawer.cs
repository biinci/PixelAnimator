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
        private const float Padding = 2;
        private static readonly Dictionary<string, Object> ObjectListByPropertyPath = new();
        private static readonly Dictionary<string, BaseMethodData> CachedPropertyReference = new();
        private static readonly Dictionary<string, long> CachedManagedReferenceValueId = new();
        private const string NoFunctionLabel = "No function";
        private const string EmptyReferenceTip = "Reference is empty";
    
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SetPropertyReference(property);
            var reference = CachedPropertyReference[property.propertyPath];
            if (reference == null) return;
            _functionIcon ??= Resources.Load<Texture2D>("Sprites/function-icon");
            position.y += 2;

            var serializedMethod = property.FindPropertyRelative("method");
            var serializedParameters = property.FindPropertyRelative("parameters");
            var serializedComponentType = property.FindPropertyRelative("componentType");
                
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
                DrawComponentType(objectRect, serializedComponentType, property);
                DrawMethod(methodRect, serializedComponentType, functionTexRect, functionLabelRect, functionLabel, property);
                
                var showParameters = functionLabel != NoFunctionLabel && property.isExpanded;
                if(showParameters)DrawParameters(property, serializedParameters, position); 
            }

            UpdateReference(property, serializedComponentType);

            
        }

        private void UpdateReference(SerializedProperty serializedMethodData,SerializedProperty serializedComponentType)
        {
            if (ObjectListByPropertyPath.TryGetValue(serializedComponentType.propertyPath, out var obj) && obj?.GetType() != CachedPropertyReference[serializedMethodData.propertyPath].componentType.SystemType)
            {
                OnComponentTypeChanged(serializedMethodData,serializedComponentType, CachedPropertyReference[serializedMethodData.propertyPath].componentType.SystemType);
            }
        }
        

        #region Drawing Methods
        private void DrawComponentType(Rect objectRect, SerializedProperty serializedComponentType, SerializedProperty serializedMethodData)
        {
            var reference = CachedPropertyReference[serializedMethodData.propertyPath];
            var type = reference.componentType.SystemType;
            var content = EditorGUIUtility.ObjectContent(null, type);
            content.text = content.text.Replace("None", "").Replace("(", "").Replace(")", "");

            var style = new GUIStyle(GUI.skin.GetStyle("ObjectField"))
            {
                imagePosition = ImagePosition.ImageLeft,
                clipping = TextClipping.Ellipsis
            };

                
            GUI.Box(objectRect, content, style);
            var id = GUIUtility.GetControlID(content, FocusType.Passive, objectRect);
            var evt = Event.current;
        
            if (evt.type == EventType.MouseDown && objectRect.Contains(evt.mousePosition))
            {
                GUIUtility.hotControl = id;
                evt.Use();
            }
                
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!objectRect.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                    
                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            var objType = obj.GetType();
                            if (!objType.IsSubclassOf(typeof(Component)))
                            {
                                continue;
                            }
                            reference.componentType = new SerializableType(objType);
                            ResetMethod(serializedMethodData);
                            break;
                        }
                    }
                    Event.current.Use();
                    break;
            }
            
            // var obj = GetUnityObject(serializedComponentType, serializedMethodData);//TODO: performance's sake, this should be fixed
            // EditorGUI.BeginChangeCheck();
            // obj = EditorGUI.ObjectField(objectRect, obj, typeof(Component), true);
            // if (EditorGUI.EndChangeCheck())
            // {
            //     OnObjectReferenceChanged(serializedMethodData,serializedComponentType, obj);   
            // }
            //
            // var name = CachedPropertyReference.TryGetValue(serializedMethodData.propertyPath, out var data) ? data.componentType.Name : string.Empty;
            // var serializedType = string.IsNullOrEmpty(name)
            //     ? EmptyReferenceTip
            //     : name;
            // EditorGUI.LabelField(objectRect, new GUIContent("", serializedType));

        }

        private void DrawMethod(Rect methodRect, SerializedProperty serializedComponentType, Rect functionTexRect, Rect functionLabelRect, string content, SerializedProperty serializedMethodData)
        {
            if (EditorGUI.DropdownButton(methodRect, GUIContent.none, FocusType.Keyboard))
            {
                SelectMethod(serializedMethodData,serializedComponentType);
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
                    EditorGUI.PropertyField(paramRect, paramProperty, new GUIContent(name,"The parameter will be passed at runtime."),true);
                    EditorGUI.EndDisabledGroup();
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
        
        #region Set Data

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
        
        // private void OnObjectReferenceChanged(SerializedProperty serializedMethodData, SerializedProperty serializedComponentType, Object obj)
        // {
        //     Debug.Log("object is changed");
        //     ObjectListByPropertyPath[serializedComponentType.propertyPath] = obj;
        //     CachedPropertyReference[serializedMethodData.propertyPath].componentType =new SerializableType(obj.GetType());
        //     ResetMethod(serializedMethodData);
        // }

        private void OnComponentTypeChanged(SerializedProperty serializedMethodData, SerializedProperty serializedComponentType, Type type)
        {
            Debug.Log("component type is changed");
            ObjectListByPropertyPath[serializedComponentType.propertyPath] = type != null ? Activator.CreateInstance(type) as Object : null;
            ResetMethod(serializedMethodData);
        }
        
        private void SelectMethod(SerializedProperty serializedMethodData, SerializedProperty serializedComponentType)
        {
            serializedMethodData.serializedObject.Update();
            var functionMenu = new GenericMenu();
            functionMenu.AddItem(new GUIContent("No function"), false, ()=>ResetMethod(serializedMethodData));
            var componentType = GetComponentType(serializedMethodData);

            if (componentType == null)
            {
                return;
            }
            
            var allMethods = componentType.GetMethods(BindingFlags.Instance | BindingFlags.Public);

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
                     (methodInfo.Name.StartsWith("get_") /*|| methodInfo.Name.StartsWith("set_")*/)) &&
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

        private Object GetUnityObject(SerializedProperty serializedComponentType, SerializedProperty serializedMethodData)
        {
            var propertyPath = serializedComponentType.propertyPath;
            
            var isFound = ObjectListByPropertyPath.TryGetValue(propertyPath, out var obj);
            if (isFound) return obj;
            var type = GetComponentType(serializedMethodData);
            obj = type != null ? Activator.CreateInstance(type) as Object : null; // The way of accessing reference must be changed.
            ObjectListByPropertyPath[propertyPath] = obj; 
            return obj; 
        }
        
        private Type GetComponentType(SerializedProperty serializedMethodData)
        {
            // var propertyPath = serializedComponentType.propertyPath;
            //
            // var isFound = ObjectListByPropertyPath.TryGetValue(propertyPath, out var tuple);
            // if (isFound) return tuple.Item1;
            // var obj = GlobalObjectId.TryParse(serializedComponentType.stringValue, out var objectId) ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(objectId) : null;
            // ObjectListByPropertyPath[propertyPath] = new Tuple<Object, string>(obj, serializedComponentType.stringValue); 
            // return obj;
            
            return CachedPropertyReference[serializedMethodData.propertyPath].componentType.SystemType;
            
        }
        
        #endregion
    }
}

