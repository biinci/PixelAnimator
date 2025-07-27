using UnityEngine;
using UnityEditor;

namespace binc.PixelAnimator.Editor.Windows
{
    public partial class CanvasWindow
    {
        private Texture2D cachedSpritePreview;
        private Sprite lastCachedSprite;
        private int lastSpriteIndex = -1;
        
        public override void ProcessWindow()
        {
            windowRect = new Rect(Vector2.zero,
                new Vector2(PixelAnimatorWindow.AnimatorWindow.position.width, timelineWindow.WindowRect.y));
            
            var isValid = SelectedAnim && SelectedAnim.GetSpriteList() != null;
            if(!isValid) return;
            if(PixelAnimatorWindow.AnimatorWindow.GetSpriteCount() > 0) SetSpritePreview();
            if(!spritePreview) return;
            RenderCanvas();
            
            HandleMouseOperations();
            SetRect();
        }
        private void HandleMouseOperations(){
            if(UsingBoxHandle != BoxHandleType.None || !spritePreview) return;
            var eventCurrent = Event.current;
            if(eventCurrent.button == 2) HandleMiddleClick();
            if(eventCurrent.type == EventType.ScrollWheel) HandleZoom();
        }
        private void RenderCanvas()
        {
            ClampPosition();
            var canvasSize = new Vector2(spritePreview.width, spritePreview.height) * spriteScale;
            canvasRect = new Rect(viewOffset+screenSpriteOrigin,canvasSize);
            GUI.Window(Id, canvasRect, _=>{RenderWindowContent();}, GUIContent.none, GUIStyle.none);
        }
        private void ClampPosition() { 
            screenSpriteOrigin.x = windowRect.width * 0.5f - spritePreview.width * 0.5f * spriteScale;
            screenSpriteOrigin.y = windowRect.height * 0.5f - spritePreview.height * 0.5f * spriteScale;
            
            if (viewOffset.x > spritePreview.width * spriteScale * 0.5f) viewOffset.x = spritePreview.width * spriteScale * 0.5f;
            if (viewOffset.x < -spritePreview.width * spriteScale * 0.5f) viewOffset.x = -spritePreview.width * spriteScale * 0.5f;
            
            if (viewOffset.y > spritePreview.height * spriteScale * 0.5f) viewOffset.y= spritePreview.height * spriteScale * 0.5f;
            if (viewOffset.y < -spritePreview.height * spriteScale * 0.5f) viewOffset.y = -spritePreview.height * spriteScale * 0.5f;

        }
        private void RenderWindowContent()
        {
            // if (!PixelAnimatorWindow.AnimatorWindow.IsValidFrame()) return;
            DrawGrid();
            DrawSprite();
            DrawBoxes();
        }
        private void HandleMiddleClick()
        {
            var currentEvent = Event.current;
            switch (currentEvent.type)
            {
                case EventType.MouseDown when windowRect.Contains(currentEvent.mousePosition):
                    isDraggable = true;
                    previousMousePosition = currentEvent.mousePosition;
                    Event.current.Use();
                    break;
                case EventType.MouseDrag when isDraggable:
                {
                    var delta = currentEvent.mousePosition - previousMousePosition;
                    viewOffset += delta;
                    previousMousePosition = currentEvent.mousePosition;
                    PixelAnimatorWindow.AddCursorCondition(true, MouseCursor.MoveArrow);
                    Event.current.Use();
                    break;
                }
                case EventType.MouseUp:
                    isDraggable = false;
                    Event.current.Use();
                    break;
            }
        }
        
        private void HandleZoom()
        {
            var eventCurrent = Event.current;
            var scaleDelta = Mathf.Sign(eventCurrent.delta.y) > 0 ? -1 : 1;
            var previousScale = spriteScale;
            spriteScale += scaleDelta;
            spriteScale = spriteScale switch
            {
                <= 0 => 1,
                > 6 => 6,
                _ => spriteScale
            };
            var ratio = spriteScale / (float)previousScale;

            var mousePosForWindow = eventCurrent.mousePosition - canvasRect.position;
            var newOrigin = new Vector2
            ( 
                windowRect.width * 0.5f - spritePreview.width * 0.5f * spriteScale,
                windowRect.height * 0.5f - spritePreview.height * 0.5f * spriteScale
            );
            var mousePosForNewWindow = eventCurrent.mousePosition - (newOrigin + viewOffset);
            var scaledMousePos = mousePosForWindow * ratio;
            var deltaVector = mousePosForNewWindow - scaledMousePos;
            viewOffset += deltaVector;
            Event.current.Use();
            if(spriteScale <=0) spriteScale = 1;
        }

        private void SetRect(){
            spriteRect = new Rect(Vector2.zero, canvasRect.size);
        }
        
        private void SetSpritePreview()
        {
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var pixelSprites = animatorWindow.SelectedAnimation.PixelSprites;
            var index = animatorWindow.IndexOfSelectedSprite;
        
            if (index == lastSpriteIndex && cachedSpritePreview != null)
            {
                spritePreview = cachedSpritePreview;
                return;
            }
        
            var sprite = pixelSprites[index].sprite;
        
            if (sprite == lastCachedSprite && cachedSpritePreview != null)
            {
                spritePreview = cachedSpritePreview;
                lastSpriteIndex = index;
                return;
            }
        
            var preview = AssetPreview.GetAssetPreview(sprite);
            if (!preview) return;
            preview.filterMode = FilterMode.Point;
            cachedSpritePreview = preview;
            spritePreview = preview;
            lastCachedSprite = sprite;
            lastSpriteIndex = index;
        }

        private void AddCursorRect(Rect rect, MouseCursor cursor, BoxHandleType type){
            EditorGUIUtility.AddCursorRect(rect, cursor);
            PixelAnimatorUtility.AddCursorCondition(canvasRect,UsingBoxHandle == type, cursor);
        }
        
        private void GenerateGridTexture(Vector2 size)
        {
            var width = Mathf.FloorToInt(size.x);
            var height = Mathf.FloorToInt(size.y);
            if(width <= 0 || height <= 0) return;

            if (cachedGridTexture)
            {
                Object.DestroyImmediate(cachedGridTexture);
            }
        
            cachedGridTexture = new Texture2D(width, height, TextureFormat.RGB24, false)
            {
                filterMode = FilterMode.Point
            };

            var pixels = new Color32[width * height];
        
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var gridX = x / 16;
                    var gridY = y / 16;
                    var color = (gridX + gridY) % 2 == 0 ? blackColor : whiteColor;
                    pixels[y * width + x] = color;
                }
            }
        
            cachedGridTexture.SetPixels32(pixels);
            cachedGridTexture.Apply();
        }
        public override void Dispose()
        {
            if (cachedGridTexture == null) return;
            Object.DestroyImmediate(cachedGridTexture);
            cachedGridTexture = null;
        }
        
    }
}
