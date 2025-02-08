using UnityEngine;
using UnityEditor;

namespace binc.PixelAnimator.Editor.Windows
{
    public partial class CanvasWindow
    {
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
        private void RenderWindowContent(){
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
        
        private void SetSpritePreview(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var pixelSprites = animatorWindow.SelectedAnimation.PixelSprites;
            var index = animatorWindow.IndexOfSelectedSprite;
            var sprite = pixelSprites[index].sprite;
            spritePreview = AssetPreview.GetAssetPreview(sprite);
            if (spritePreview) spritePreview.filterMode = FilterMode.Point;
        }

        private void AddCursorRect(Rect rect, MouseCursor cursor, BoxHandleType type){
            EditorGUIUtility.AddCursorRect(rect, cursor);
            PixelAnimatorUtility.AddCursorCondition(canvasRect,UsingBoxHandle == type, cursor);
        }
    }
}
