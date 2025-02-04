using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
#endif
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace binc.PixelAnimator
{
    public static class PixelAnimatorUtility
    {
        public delegate void PixelAnimationListener(object userData);

        public static Rect MapBoxRectToTransform(Rect rect, Sprite sprite)
        {
            var offset = new Vector2(rect.x + rect.width * 0.5f - sprite.pivot.x,
                sprite.rect.height - rect.y - rect.height * 0.5f - sprite.pivot.y) / sprite.pixelsPerUnit;
            var size = new Vector2(rect.width, rect.height) / sprite.pixelsPerUnit;
            return new Rect(offset, size);
        }

        private static Texture2D _sCheckerTexture;

        public static Texture2D CheckerTexture
        {
            get
            {
                if (!_sCheckerTexture)
                {
                    _sCheckerTexture = CreateCheckerTexture(
                        dark: new Color(0.4f, 0.4f, 0.4f, 1.0f),
                        light: new Color(0.6f, 0.6f, 0.6f, 1.0f)
                    );
                }

                return _sCheckerTexture;
            }
        }

        private static Texture2D CreateCheckerTexture(Color dark, Color light)
        {
            const int squareAmount = 4;
            var tex = new Texture2D(squareAmount, squareAmount)
            {
                hideFlags = HideFlags.DontSave
            };

            var colors = new Color[squareAmount * squareAmount];
            for (var y = 0; y < squareAmount; y++)
            {
                for (var x = 0; x < squareAmount; x++)
                {
                    var color = ((x + y) % 2 == 0) ? light : dark;
                    colors[y * squareAmount + x] = color;
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Point;

            return tex;
        }

#if UNITY_EDITOR
        
        public static void AddCursorCondition(Rect area, bool condition, MouseCursor icon)
        {
            if (!condition) return;
            var rect = new Rect(Vector2.zero, area.size);
            EditorGUIUtility.AddCursorRect(rect, icon);
        }
        
        public static bool IsClickedRect(this Rect rect, params int[] ints)
        {
            
            return Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition) &&
                   ints.Contains(Event.current.button);
        }

        public static Rect DrawSpriteThumb(Rect position, int id, Sprite sprite)
        {
            var objectFieldThumb = EditorStyles.objectFieldThumb;
            // objectFieldThumb.Draw(position, GUIContent.none, id, DragAndDrop.activeControlID == id, position.Contains(Event.current.mousePosition));
            var padding = new RectOffset(2, 2, 2, 2);
            var spriteRect = padding.Remove(position);
            if (sprite)
            {
                var content = new GUIContent(sprite.texture);
                var assetPreview = AssetPreview.GetAssetPreview(sprite);
                GUI.DrawTexture(spriteRect, CheckerTexture, ScaleMode.StretchToFill, true);
                if (assetPreview)
                {
                    GUI.DrawTexture(spriteRect, assetPreview, ScaleMode.ScaleToFit, true);

                }
                else
                {
                    GUI.DrawTexture(spriteRect, content.image, ScaleMode.StretchToFill, true);
                    HandleUtility.Repaint();
                }
            }
            else
            {
                GUIStyle guiStyle = objectFieldThumb.name + "Overlay";
                guiStyle.Draw(position, GUIContent.none, id);
                GUI.DrawTexture(spriteRect, CheckerTexture, ScaleMode.StretchToFill, false);

            }

            return spriteRect;
        }
        
        
        /// <summary>
        /// Gets the object reference that a SerializedProperty points to.
        /// Handles nested properties and arrays.
        /// </summary>
        public static object GetReference(this SerializedProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var path = property.propertyPath;
            object obj = property.serializedObject.targetObject;

            var parts = path.Split('.');
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                if (part == "Array")
                {
                    if (i + 1 >= parts.Length || !parts[i + 1].StartsWith("data["))
                        break;

                    var nextPart = parts[i + 1];
                    var indexStart = nextPart.IndexOf('[') + 1;
                    var indexEnd = nextPart.IndexOf(']');
                    var index = int.Parse(nextPart.Substring(indexStart, indexEnd - indexStart));

                    if (obj is IList list)
                    {
                        obj = list[index];
                    }
                    else
                    {
                        obj = null;
                        break;
                    }

                    i++;
                }
                else
                {
                    var type = obj.GetType();
                    FieldInfo field = null;

                    while (field == null && type != null)
                    {
                        field = type.GetField(part, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        type = type.BaseType;
                    }

                    if (field == null)
                    {
                        obj = null;
                        break;
                    }

                    obj = field.GetValue(obj);
                }

                if (obj == null)
                    break;
            }

            return obj;
        }
            
            
        public static void DropAreaGUI(Rect rect, ReorderableList list, PixelAnimationListener listener)
        {
            var evt = Event.current;
            var dropArea = rect;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (dropArea.Contains(evt.mousePosition))
                    {

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            foreach (var draggedObject in DragAndDrop.objectReferences)
                            {
                                list.index = list.count - 1;
                                if (draggedObject is object obj)
                                {
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

