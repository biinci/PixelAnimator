using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Common;

namespace binc.PixelAnimator.Editor.Windows{

    public partial class CanvasWindow
    {

        private Vector2 screenSpriteOrigin, viewOffset = new (0, 0);

        private void DrawCanvas()
        {
            ClampPosition();
            var canvasSize = new Vector2(spritePreview.width, spritePreview.height) * spriteScale;
            canvasRect = new Rect(viewOffset+screenSpriteOrigin,canvasSize);
            GUI.Window(Id, canvasRect, _=>{WindowFunction();}, GUIContent.none, GUIStyle.none);
            
        }
        private void ClampPosition() { 
            screenSpriteOrigin.x = windowRect.width * 0.5f - spritePreview.width * 0.5f * spriteScale;
            screenSpriteOrigin.y = windowRect.height * 0.5f - spritePreview.height * 0.5f * spriteScale;
            
            if (viewOffset.x > spritePreview.width * spriteScale * 0.5f) viewOffset.x = spritePreview.width * spriteScale * 0.5f;
            if (viewOffset.x < -spritePreview.width * spriteScale * 0.5f) viewOffset.x = -spritePreview.width * spriteScale * 0.5f;
            
            if (viewOffset.y > spritePreview.height * spriteScale * 0.5f) viewOffset.y= spritePreview.height * spriteScale * 0.5f;
            if (viewOffset.y < -spritePreview.height * spriteScale * 0.5f) viewOffset.y = -spritePreview.height * spriteScale * 0.5f;

        }

        private void WindowFunction(){
            DrawGrid();//ok
            DrawSprite();//ok
            DrawBoxes();
        }
        
        private void DrawSprite() => GUI.DrawTexture(spriteRect, spritePreview, ScaleMode.ScaleToFit);

        private void DrawBoxes(){
            var groups = SelectedAnim.BoxGroups;
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
            var rect = spriteRect;
            var grid = new Rect(rect.x, rect.y, 16 * spriteScale, 16 * spriteScale); //define a single 16x16 tile

            for (var i = 0; i < spritePreview.width / 16; i++) {
                for (var j = 0; j < spritePreview.height / 16; j += 2) {
                    var tex = i % 2 == 0 ? gridBlackTex : gridWhiteTex;
                    GUI.DrawTexture(grid, tex);
                    grid.y += grid.height; 
                    var texTwo = tex == gridBlackTex ? gridWhiteTex : gridBlackTex;
                    GUI.DrawTexture(grid, texTwo);
                    grid.y += grid.height;
                }

                grid.y = rect.y;
                grid.x += grid.width;
            }

            if (!(rect.x + rect.width - grid.x > 0)) return;

            grid.width = rect.x + rect.width - grid.x; 

            for (var j = 0; j < spritePreview.height / 16; j += 2) {
                GUI.DrawTexture(grid, gridWhiteTex);
                grid.y += grid.height;
                GUI.DrawTexture(grid, gridBlackTex); 
                grid.y += grid.height;
            }

            grid.height = rect.y + rect.height - grid.y;
            if (rect.y + rect.height - grid.y > 0) GUI.DrawTexture(grid, gridBlackTex); 
        }

        


        private void ProcessBoxes(int groupIndex, BoxGroup boxGroup, BoxData boxData){
            
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var isActiveGroup = groupIndex == animatorWindow.IndexOfSelectedBoxGroup;

            var selectedLayerIndex = animatorWindow.IndexOfSelectedBox;
            for (var i = 0; i < boxGroup.boxes.Count; i++)
            {
                var isBoxActive = isActiveGroup && boxGroup.isExpanded &&
                                  selectedLayerIndex == i;
                var frame = boxGroup.boxes[i].frames[animatorWindow.IndexOfSelectedSprite];
                
                if(frame.GetFrameType() == BoxFrameType.EmptyFrame) continue;
                
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
                    }
                    if (eventCurrent.type == EventType.MouseUp) {
                        EditingBoxHandle = BoxHandleType.None;
                    }

                    
                }else{
                    Handles.DrawSolidRectangleWithOutline(scaledRect, new Color(0.5f,0.5f,0.5f,0f), boxData.color );
                    var mousePos = Event.current.mousePosition;
                    var isClickedRect = scaledRect.Contains(mousePos) && Event.current.button == 0 &&
                                        Event.current.type == EventType.MouseDown;

                    
                    if (isClickedRect && EditingBoxHandle==BoxHandleType.None && !GetActiveGUIBoxRect().Contains(mousePos))
                    {
                        Event.current.Use();
                        animatorWindow.SelectBoxGroup(groupIndex);
                        animatorWindow.SelectBox(i);
                        boxGroup.isExpanded = true;
                    }

                }

                
            }
                
        }

        private Rect GetActiveGUIBoxRect()
        {
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var rect =  SelectedAnim
                .BoxGroups[animatorWindow.IndexOfSelectedBoxGroup]
                .boxes[animatorWindow.IndexOfSelectedBox]
                .frames[animatorWindow.IndexOfSelectedSprite].boxRect;
            rect.position *= spriteScale;
            rect.size *= spriteScale;
            return rect;
        }
        
        private void EditBox(BoxFrame boxFrame, Rect rect, Vector2 position)
        {
            
            switch (EditingBoxHandle) {
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
            PixelAnimatorWindow.AnimatorWindow.Repaint();
        }
        
        private void DrawBox(Rect rect, Color boxColor)
        {
            var transparentColor = new Color(boxColor.r, boxColor.g, boxColor.b, 0.2f);
            DrawHandle(boxColor, rect);
            Handles.DrawSolidRectangleWithOutline(rect, transparentColor, boxColor );

        }

        private void DrawHandle(Color color, Rect rect)
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
                var temp = EditingBoxHandle;
                var mousePos = eventCurrent.mousePosition;
                if (rTopLeft.Contains(mousePos))
                    EditingBoxHandle = BoxHandleType.TopLeft;
                else if (rTopCenter.Contains(mousePos))
                    EditingBoxHandle = BoxHandleType.TopCenter;
                else if (rTopRight.Contains(mousePos))
                    EditingBoxHandle = BoxHandleType.TopRight;
                else if (rRightCenter.Contains(mousePos))
                    EditingBoxHandle = BoxHandleType.RightCenter;
                else if (rBottomRight.Contains(mousePos))
                    EditingBoxHandle = BoxHandleType.BottomRight;
                else if (rBottomCenter.Contains(mousePos))
                    EditingBoxHandle = BoxHandleType.BottomCenter;
                else if (rBottomLeft.Contains(mousePos))
                    EditingBoxHandle = BoxHandleType.BottomLeft;
                else if (rLeftCenter.Contains(mousePos))
                    EditingBoxHandle = BoxHandleType.LeftCenter;
                else if (rAdjustedMiddle.Contains(mousePos)) {
                    EditingBoxHandle = BoxHandleType.Middle;
                    clickedMousePos = mousePos;
                }
                else {
                    EditingBoxHandle = BoxHandleType.None;
                }

                if (EditingBoxHandle != temp)
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