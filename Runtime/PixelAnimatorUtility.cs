using System;
using System.Linq;
using System.Linq.Expressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace binc.PixelAnimator{
    public static class PixelAnimatorUtility{
        public delegate void PixelAnimationListener(object userData);
        public static Rect MapBoxRectToTransform(Rect rect, Sprite sprite){
            var offset = new Vector2(rect.x + rect.width * 0.5f - sprite.pivot.x, sprite.rect.height - rect.y - rect.height * 0.5f - sprite.pivot.y)/sprite.pixelsPerUnit;
            var size = new Vector2(rect.width, rect.height) / sprite.pixelsPerUnit;
            return new Rect(offset, size);
        }
        public static Action MethodDataToAction(MethodData data)
        {
            var parameters = data.parameters.Select(p => p.InheritData).ToArray();
            var lambdaParam = Expression.Parameter(typeof(object[]), "parameters");
            data.method.LoadMethodInfo();
            var info = data.method.methodInfo;
            var methodParams = info.GetParameters();
            var convertedParams = new Expression[methodParams.Length];
            for (var i = 0; i < methodParams.Length; i++)
            {
                var paramAccess = Expression.ArrayIndex(lambdaParam, Expression.Constant(i));
                convertedParams[i] = Expression.Convert(paramAccess, methodParams[i].ParameterType);
            }

            var parsable= GlobalObjectId.TryParse(data.globalId, out var id);
            object value = parsable ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) : null;
            if (value == null)
            {
                Debug.LogError("Object not found");
                return () => { };
            }
            var methodCall = Expression.Call(
                Expression.Constant(value),  
                info,                    
                convertedParams                
            );

            var lambda = Expression.Lambda<Action<object[]>>(methodCall, lambdaParam);
            var compiledDelegate = lambda.Compile();
            return ()=>compiledDelegate(parameters);
        }

#if UNITY_EDITOR
        public static void CreateTooltip(Rect rect, string tooltip){
            if (rect.Contains(Event.current.mousePosition)) {
                EditorGUI.LabelField(rect,
                    new GUIContent("", tooltip));
            }
        }
        
        public static void DropAreaGUI (Rect rect, ReorderableList list, PixelAnimationListener listener){ 
            var evt = Event.current;
            var dropArea = rect;
        
            switch (evt.type) {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (dropArea.Contains(evt.mousePosition)) {
                    
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    
                    if (evt.type == EventType.DragPerform) {
                        DragAndDrop.AcceptDrag ();
                    
                        foreach (var draggedObject in DragAndDrop.objectReferences) {
                            list.index = list.count-1;
                            if(draggedObject is object obj){
                                Debug.Log(obj);
                                list.onAddCallback.Invoke(list);
                                listener(obj);
                            }
                        }
                    }
                }
                break;
            }
        }
#endif
    }
}


