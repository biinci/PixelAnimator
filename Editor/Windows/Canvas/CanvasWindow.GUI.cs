using System;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.AnimationData;
using PlasticPipe.PlasticProtocol.Messages;

namespace binc.PixelAnimator.Editor.Windows{

    public partial class CanvasWindow
    {
        private void DrawSprite() => GUI.DrawTexture(spriteRect, spritePreview, ScaleMode.ScaleToFit);

        private void DrawBoxes(){
            var groups = SelectedAnim.BoxGroups;
            if (groups == null) return;
            var animationPreferences = PixelAnimatorWindow.AnimatorWindow.AnimationPreferences;
            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                var boxData = animationPreferences.GetBoxData(group.BoxDataGuid);
                if (!group.isVisible) continue;
                
                ProcessBoxes(i, group, boxData);
            }
        }

        private void DrawGrid()
        {
            var size = Vector2.one * 64;
            if(spritePreview) size = new Vector2(spritePreview.width, spritePreview.height);
            var rect = spriteRect;
            var grid = new Rect(rect.x, rect.y, 16 * spriteScale, 16 * spriteScale);
        
            var maxGridsX = Mathf.Min((int)(size.x / 16), (int)(rect.width / grid.width) + 2);
            var maxGridsY = Mathf.Min((int)(size.y / 16), (int)(rect.height / grid.height) + 2);
        
            for (var i = 0; i < maxGridsX; i++) {
                for (var j = 0; j < maxGridsY; j += 2) {
                    var color = i % 2 == 0 ? blackColor : whiteColor;
                    EditorGUI.DrawRect(grid, color);
                    grid.y += grid.height;

                    if (j + 1 >= maxGridsY) continue;
                    var contrastColor = color == blackColor ? whiteColor : blackColor;
                    EditorGUI.DrawRect(grid, contrastColor);
                    grid.y += grid.height;
                }
                grid.y = rect.y;
                grid.x += grid.width;
            }
        }
        private void ProcessBoxes(int groupIndex, BoxGroup boxGroup, BoxData boxData){
            
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var isActiveGroup = groupIndex == animatorWindow.IndexOfSelectedBoxGroup;

            var selectedLayerIndex = animatorWindow.IndexOfSelectedBox;
            for (var i = 0; i < boxGroup.boxes.Count; i++)
            {
                var isBoxActive = isActiveGroup && boxGroup.isExpanded &&
                                  selectedLayerIndex == i;

                var frame = boxGroup.boxes[i].GetFrame(animatorWindow.IndexOfSelectedSprite);
                if (frame == null) continue;
                var scaledRect = frame.boxRect; 
                scaledRect.position *= spriteScale;
                scaledRect.size *= spriteScale;
                
                if (isBoxActive) {
                    var eventCurrent = Event.current;
                    var isDragging = eventCurrent.type == EventType.MouseDrag && eventCurrent.type != EventType.MouseUp;
                    
                    scaledRect.width = Mathf.Clamp(scaledRect.width, 0, int.MaxValue);
                    scaledRect.height = Mathf.Clamp(scaledRect.height, 0, int.MaxValue);
                    
                    DrawBox(scaledRect, boxData.color);
                    if (isDragging)
                    {
                        var xPos = (int)eventCurrent.mousePosition.x/spriteScale;
                        var yPos = (int)eventCurrent.mousePosition.y/spriteScale;
                        var position = new Vector2(xPos, yPos);
                        EditBox(frame, scaledRect, position);
                        animatorWindow.Repaint();
                    }
                    if (eventCurrent.type == EventType.MouseUp) {
                        UsingBoxHandle = BoxHandleType.None;
                    }

                    
                }else{
                    Handles.DrawSolidRectangleWithOutline(scaledRect, new Color(0.5f,0.5f,0.5f,0f), boxData.color );
                    var mousePos = Event.current.mousePosition;
                    var isClickedRect = scaledRect.IsClickedRect(0);
                    if (isClickedRect && UsingBoxHandle==BoxHandleType.None && !GetActiveGUIBoxRect().Contains(mousePos) && boxGroup.isVisible)
                    {
                        animatorWindow.SelectBoxGroup(groupIndex);
                        animatorWindow.SelectBox(i);
                        boxGroup.isExpanded = true;
                        Event.current.Use();
                    }
                }
            }
        }

        private Rect GetActiveGUIBoxRect()
        {
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var boxGroup = animatorWindow.GetActiveBoxGroup();
            if (!boxGroup.isExpanded || !boxGroup.isVisible) return new Rect(-1, -1, -1, -1);
            var rect = animatorWindow.GetActiveFrame().boxRect;
            rect.position *= spriteScale;
            rect.size *= spriteScale;
            return rect;
        }
        
        private void EditBox(BoxFrame boxFrame, Rect rect, Vector2 position)
        {
            switch (UsingBoxHandle) {
                case BoxHandleType.TopLeft:
                    boxFrame.boxRect.xMin = position.x;
                    boxFrame.boxRect.yMin = position.y;
                    break;
                case BoxHandleType.TopCenter:
                    boxFrame.boxRect.yMin = position.y;
                    break;
                case BoxHandleType.TopRight:
                    boxFrame.boxRect.xMax = position.x;
                    boxFrame.boxRect.yMin = position.y;
                    break;
                case BoxHandleType.RightCenter:
                    boxFrame.boxRect.xMax = position.x;
                    break;
                case BoxHandleType.BottomRight:
                    boxFrame.boxRect.xMax = position.x;
                    boxFrame.boxRect.yMax = position.y;
                    break;
                case BoxHandleType.BottomCenter:
                    boxFrame.boxRect.yMax = position.y;
                    break;
                case BoxHandleType.BottomLeft:
                    boxFrame.boxRect.xMin = position.x;
                    boxFrame.boxRect.yMax = position.y;
                    break;
                case BoxHandleType.LeftCenter:
                    boxFrame.boxRect.xMin = position.x;
                    break;
                case BoxHandleType.Middle:
                    var xDelta = (int)((clickedMousePos.x - rect.xMin) / spriteScale);
                    var yDelta = (int)((clickedMousePos.y - rect.yMin) / spriteScale);
                    
                    var newXPos = position.x - xDelta;
                    var newYPos = position.y - yDelta;

                    var xSize = (int)rect.size.x / spriteScale;
                    var ySize = (int)rect.size.y / spriteScale;

                    boxFrame.boxRect.position = new Vector2(newXPos, newYPos);
                    boxFrame.boxRect.size = new Vector2(xSize, ySize);
                    clickedMousePos = Event.current.mousePosition;
                    break;
                case BoxHandleType.None:
                    break;
            }

            PixelAnimatorWindow.AnimatorWindow.SerializedSelectedAnimation?.ApplyModifiedProperties();
        }
        
        private void DrawBox(Rect rect, Color boxColor)
        {
            var transparentColor = new Color(boxColor.r, boxColor.g, boxColor.b, 0.2f);
            DrawHandles(boxColor, rect);
            Handles.DrawSolidRectangleWithOutline(rect, transparentColor, boxColor );
        }

        private void DrawHandles(Color color, Rect rect)
        {
            var eventCurrent = Event.current;
            float handleSize = spriteScale; 
            var rTopLeft = new Rect(rect.xMin - handleSize / 2, rect.yMin - handleSize / 2, handleSize, handleSize);
            var rTopCenter = new Rect(rect.xMin + rect.width / 2 - handleSize / 2, rect.yMin - handleSize / 2, handleSize, handleSize);
            var rTopRight = new Rect(rect.xMax - handleSize / 2, rect.yMin - handleSize / 2, handleSize, handleSize);
            var rRightCenter = new Rect(rect.xMax - handleSize / 2, rect.yMin + (rect.yMax - rect.yMin) / 2 - handleSize / 2,
                handleSize, handleSize);
            var rBottomRight = new Rect(rect.xMax - handleSize / 2, rect.yMax - handleSize / 2, handleSize, handleSize);
            var rBottomCenter = new Rect(rect.xMin + rect.width / 2 - handleSize / 2, rect.yMax - handleSize / 2, handleSize, handleSize);
            var rBottomLeft = new Rect(rect.xMin - handleSize / 2, rect.yMax - handleSize / 2, handleSize, handleSize);
            var rLeftCenter = new Rect(rect.xMin - handleSize / 2, rect.yMin + (rect.yMax - rect.yMin) / 2 - handleSize / 2,
                handleSize, handleSize);
            var rAdjustedMiddle = new Rect(rect.x + handleSize / 2, rect.y + handleSize / 2, rect.width - handleSize,
                rect.height - handleSize);
            var leftClicked = eventCurrent.button == 0 && eventCurrent.type == EventType.MouseDown;
            if (leftClicked)
            {
                var previousBoxHandle = UsingBoxHandle;
                var mousePos = eventCurrent.mousePosition;
                if (rTopLeft.Contains(mousePos))
                    UsingBoxHandle = BoxHandleType.TopLeft;
                else if (rTopCenter.Contains(mousePos))
                    UsingBoxHandle = BoxHandleType.TopCenter;
                else if (rTopRight.Contains(mousePos))
                    UsingBoxHandle = BoxHandleType.TopRight;
                else if (rRightCenter.Contains(mousePos))
                    UsingBoxHandle = BoxHandleType.RightCenter;
                else if (rBottomRight.Contains(mousePos))
                    UsingBoxHandle = BoxHandleType.BottomRight;
                else if (rBottomCenter.Contains(mousePos))
                    UsingBoxHandle = BoxHandleType.BottomCenter;
                else if (rBottomLeft.Contains(mousePos))
                    UsingBoxHandle = BoxHandleType.BottomLeft;
                else if (rLeftCenter.Contains(mousePos))
                    UsingBoxHandle = BoxHandleType.LeftCenter;
                else if (rAdjustedMiddle.Contains(mousePos)) {
                    UsingBoxHandle = BoxHandleType.Middle;
                    clickedMousePos = mousePos;
                }
                else {
                    UsingBoxHandle = BoxHandleType.None;
                }
                if (UsingBoxHandle != previousBoxHandle)
                {
                    eventCurrent.Use();
                }
            }
            EditorGUI.DrawRect(rTopLeft, color);
            EditorGUI.DrawRect(rTopCenter, color);
            EditorGUI.DrawRect(rTopRight, color);
            EditorGUI.DrawRect(rRightCenter, color);
            EditorGUI.DrawRect(rBottomRight, color);
            EditorGUI.DrawRect(rBottomCenter, color);
            EditorGUI.DrawRect(rBottomLeft, color);
            EditorGUI.DrawRect(rLeftCenter, color);
            AddCursorRect(rTopLeft, MouseCursor.ResizeUpLeft, BoxHandleType.TopLeft);
            AddCursorRect(rTopCenter, MouseCursor.ResizeVertical, BoxHandleType.TopCenter);
            AddCursorRect(rTopRight, MouseCursor.ResizeUpRight, BoxHandleType.TopRight);
            AddCursorRect(rRightCenter, MouseCursor.ResizeHorizontal, BoxHandleType.RightCenter);
            AddCursorRect(rBottomRight, MouseCursor.ResizeUpLeft, BoxHandleType.BottomRight);
            AddCursorRect(rBottomCenter, MouseCursor.ResizeVertical, BoxHandleType.BottomCenter);
            AddCursorRect(rBottomLeft, MouseCursor.ResizeUpRight, BoxHandleType.BottomLeft);
            AddCursorRect(rLeftCenter, MouseCursor.ResizeHorizontal, BoxHandleType.LeftCenter);
            AddCursorRect(rAdjustedMiddle, MouseCursor.MoveArrow, BoxHandleType.Middle);
        }
    }
}