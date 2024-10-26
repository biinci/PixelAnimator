using System;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Common;



namespace binc.PixelAnimator.Editor.Windows
{


    public partial class CanvasWindow
    {

        private void MoveOperations(){
            if(EditingHandle != HandleType.None) return;
            var eventCurrent = Event.current;
            if(eventCurrent.button == 2)
            {
                MiddleClicked();
            }
            if(eventCurrent.type == EventType.ScrollWheel)
            {
                ScrollWheeled();
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
                    windowRect.position += delta;
                    previousMousePosition = eventCurrent.mousePosition;
                    PixelAnimatorWindow.AnimatorWindow.Repaint();
                    break;
                }
            }
        }
        private void ScrollWheeled()
        {
            var eventCurrent = Event.current;
            var scaleDelta = Mathf.Sign(eventCurrent.delta.y) > 0 ? -1 : 1;
            var previousScale = spriteScale;
            spriteScale += scaleDelta;
            if (spriteScale <= 0) spriteScale = 1;
            var ratio = spriteScale / (float)previousScale;
            var relativePosition = windowRect.position - eventCurrent.mousePosition;
            var newPosition = relativePosition * ratio;
            var deltaVector = newPosition - relativePosition;
            windowRect.position += deltaVector;
            PixelAnimatorWindow.AnimatorWindow.Repaint();
        }


        private void SetBox()
        {
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var selectedAnim = animatorWindow.SelectedAnimation;
            if (!selectedAnim) return;
            var groups = selectedAnim.Groups;
            if (!selectedAnim || groups.Count == 0 ||
                groups[animatorWindow.IndexOfSelectedGroup].layers.Count == 0) return;
            var eventCurr = Event.current;


            for (var g = 0; g < groups.Count; g++)
            {
                var group = selectedAnim.Groups[g];
                for (var l = 0; l < group.layers.Count; l++)
                {
                    var layer = group.layers[l];
                    var boxRect = layer.frames[animatorWindow.IndexOfSelectedFrame].FrameData.hitBoxRect;

                    var adjustedRect = new Rect(boxRect.position * spriteScale, boxRect.size * spriteScale);

                    boxRect.width = Mathf.Clamp(boxRect.width, 0, float.MaxValue);
                    boxRect.height = Mathf.Clamp(boxRect.height, 0, float.MaxValue);

                    var mousePos = eventCurr.mousePosition;
                    var changeActiveBox = eventCurr.button == 0 && eventCurr.clickCount == 2 && group.isVisible &&
                                          adjustedRect.Contains(mousePos);

                    if (!changeActiveBox) continue;
                    animatorWindow.SelectGroup(g);
                    animatorWindow.SelectLayer(l);
                    group.isExpanded = true;


                }
            }
        }
        
        private void SetRect(){
            var canvasSize = new Vector2(spritePreview.width, spritePreview.height) * spriteScale;
            windowRect.size = canvasSize;
            spriteRect = new Rect(Vector2.zero, windowRect.size);
        }
        
        private void SetSpritePreview(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var sprites = animatorWindow.SelectedAnimation.GetSpriteList();
            var index = animatorWindow.IndexOfSelectedFrame;
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
    }
}
