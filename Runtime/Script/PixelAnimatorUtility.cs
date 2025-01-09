using UnityEngine;
using UnityEditorInternal;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace binc.PixelAnimator.Utility{
    public static class PixelAnimatorUtility{
        public delegate void PixelAnimationListener(object userData);
        public static Rect MapBoxRectToTransform(Rect rect, Sprite sprite){
            var offset = new Vector2(rect.x + rect.width * 0.5f - sprite.pivot.x, sprite.rect.height - rect.y - rect.height * 0.5f - sprite.pivot.y)/sprite.pixelsPerUnit;
            var size = new Vector2(rect.width, rect.height) / sprite.pixelsPerUnit;
            return new Rect(offset, size);
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


