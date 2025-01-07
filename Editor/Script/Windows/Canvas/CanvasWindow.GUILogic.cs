using System;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Common;



namespace binc.PixelAnimator.Editor.Windows
{


    public partial class CanvasWindow
    {

        private void MoveOperations(){
            if(EditingBoxHandle != BoxHandleType.None) return;
            var eventCurrent = Event.current;
            if(eventCurrent.button == 2)
            {
                MiddleClicked();
            }
            if(eventCurrent.type == EventType.ScrollWheel)
            {
                ZoomChange();
            }
            if(spriteScale <=0){
                spriteScale = 1;
            }
        }

        private void MiddleClicked()
        {
            var eventCurrent = Event.current;
            switch (eventCurrent.type)
            {
                case EventType.MouseDown:
                    previousMousePosition = eventCurrent.mousePosition;
                    break;
                case EventType.MouseDrag:
                {
                    var delta = eventCurrent.mousePosition - previousMousePosition;
                    // windowRect.position += delta;
                    viewOffset += delta;
                    previousMousePosition = eventCurrent.mousePosition;
                    PixelAnimatorWindow.AnimatorWindow.Repaint();
                    break;
                }
            }
        }
        private void ZoomChange()
        {
            var eventCurrent = Event.current;
            var scaleDelta = Mathf.Sign(eventCurrent.delta.y) > 0 ? -1 : 1;
            var previousScale = spriteScale;
            spriteScale += scaleDelta;
            Event.current.Use();
            spriteScale = spriteScale switch
            {
                <= 0 => 1,
                > 6 => 6,
                _ => spriteScale
            };
            var ratio = spriteScale / (float)previousScale;

            var mousePosForWindow = eventCurrent.mousePosition - windowRect.position;
            
            var spriteWindow = PixelAnimatorWindow.AnimatorWindow.AvailableSpace;
            var newOrigin = new Vector2
            ( 
                spriteWindow.width * 0.5f - spritePreview.width * 0.5f * spriteScale,
                spriteWindow.height * 0.5f - spritePreview.height * 0.5f * spriteScale
            );
            var mousePosForNewWindow = eventCurrent.mousePosition - (newOrigin + viewOffset);
            var scaledMousePos = mousePosForWindow * ratio;
            var deltaVector = mousePosForNewWindow - scaledMousePos;
            viewOffset += deltaVector;
            PixelAnimatorWindow.AnimatorWindow.Repaint();
        }


       
        
        private void SetRect(){
            var canvasSize = new Vector2(spritePreview.width, spritePreview.height) * spriteScale;
            windowRect.size = canvasSize;
            spriteRect = new Rect(Vector2.zero, windowRect.size);
        }
        
        private void SetSpritePreview(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var sprites = animatorWindow.SelectedAnimation.GetSpriteList();
            var index = animatorWindow.IndexOfSelectedSprite;
            var sprite = sprites[index];
            spritePreview = AssetPreview.GetAssetPreview(sprite);
            if(spritePreview) spritePreview.filterMode = FilterMode.Point;
        }
        
        private void FocusToCanvas(){
            var availableSpace = PixelAnimatorWindow.AnimatorWindow.AvailableSpace;
            var eventCurrent = Event.current;
            var isClicked = eventCurrent.type == EventType.MouseDown && availableSpace.Contains(eventCurrent.mousePosition);
            if(!isClicked) return;
            PixelAnimatorWindow.AnimatorWindow.SelectFocusWindow(this);
        }
        

        private void AddCursorRect(Rect rect, MouseCursor cursor, BoxHandleType type){
            EditorGUIUtility.AddCursorRect(rect, cursor);
            PixelAnimatorWindow.AddCursorBool(EditingBoxHandle == type, cursor);

        }

        public void SetHandle(BoxHandleType boxHandleType){
            EditingBoxHandle = boxHandleType;
        }

        public override void FocusFunctions(){
            MoveOperations();
            DrawFocusOutline();
            timelineWindow.SetShortcuts();
        }
        
        
    }
}
