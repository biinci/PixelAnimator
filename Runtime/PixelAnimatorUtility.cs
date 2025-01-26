using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using System;
using System.Linq.Expressions;

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

            var parsable = GlobalObjectId.TryParse(data.GlobalId, out var id);
            var value = parsable ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) : null;
            if (value == null)
            {
                Debug.LogError("Object not found");
                return () => { };
            }

            data.obj = value;
            var methodCall = Expression.Call(
                Expression.Constant(value),
                info,
                convertedParams
            );

            var lambda = Expression.Lambda<Action<object[]>>(methodCall, lambdaParam);
            var compiledDelegate = lambda.Compile();
            return () => compiledDelegate(parameters);
        }

#if UNITY_EDITOR
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

        public static void CreateTooltip(Rect rect, string tooltip)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                EditorGUI.LabelField(rect,
                    new GUIContent("", tooltip));
            }
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

