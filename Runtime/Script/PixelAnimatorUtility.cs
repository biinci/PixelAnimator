using System;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEditorInternal;



#if UNITY_EDITOR
using UnityEditor;

#endif


namespace binc.PixelAnimator.Utility{

    
    public static class PixelAnimatorUtility{

        public static Texture2D GetTexture2DForColor(Color color)
        {   
            var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }
        
        
#if UNITY_EDITOR
        
        
        public static void CreateTooltip(Rect rect, string tooltip, Vector2 containsPosition){
            if (rect.Contains(containsPosition)) {
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
        
        public delegate void PixelAnimationListener(object userData);

        

        

    }

 
}


