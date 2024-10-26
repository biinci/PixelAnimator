using System;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Common;


namespace binc.PixelAnimator.Editor.Windows{

    public partial class CanvasWindow{
        
        private void DrawCanvas() => GUI.Window(Id, windowRect, _=>{WindowFunction();}, GUIContent.none, GUIStyle.none);


        private void WindowFunction(){
            DrawGrid();
            DrawSprite();
            DrawBoxes();
        }
        
        private void DrawSprite() => GUI.DrawTexture(spriteRect, spritePreview, ScaleMode.ScaleToFit);

        private void DrawBoxes(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            if (animatorWindow.SelectedAnimation?.Groups == null) return;
            
            var spriteSize = new Vector2(spritePreview.width, spritePreview.height);
            
            var groups = animatorWindow.SelectedAnimation.Groups;
            var animationPreferences = animatorWindow.AnimationPreferences;
            
            foreach (var group in groups) {
                var boxData = animationPreferences.GetBoxData(group.BoxDataGuid);
                DrawBox(group, boxData, spriteSize, EditingHandle);
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
                //iterate over Y
                GUI.DrawTexture(grid, gridWhiteTex);
                grid.y += grid.height;
                GUI.DrawTexture(grid, gridBlackTex); 
                grid.y += grid.height;
            }

            grid.height = rect.y + rect.height - grid.y;
            if (rect.y + rect.height - grid.y > 0) GUI.DrawTexture(grid, gridBlackTex); 
        }

        

        private void DrawBox(Group group, BoxData boxData, Vector2 spriteSize, HandleType handleTypes){
            if (!group.isVisible) return;
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var rectColor = boxData.color;
            
            var isActiveGroup = animatorWindow.SelectedAnimation.Groups.IndexOf(group) == animatorWindow.IndexOfSelectedGroup;
                
            var isBoxActive = isActiveGroup && group.isExpanded && animatorWindow.PropertyFocus == PropertyFocusEnum.HitBox;
            if(group.layers.Count == 0) return;
            var frame = group.layers[animatorWindow.IndexOfSelectedLayer].frames[animatorWindow.IndexOfSelectedFrame];// Getting the active frame on all layers
            if(frame.GetFrameType() == FrameType.EmptyFrame) return;
            var rect = frame.FrameData.hitBoxRect; //Getting frame rect for the drawing.
            rect.position *= spriteScale; //Changing rect position and size for zoom.
            rect.size *= spriteScale;
            

            if (isBoxActive) {

                float handleSize = 1*spriteScale; // r = rect
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
                var eventCurrent = Event.current;
                if (eventCurrent.button == 0 && eventCurrent.type == EventType.MouseDown) {
                    var mousePos = eventCurrent.mousePosition;
                    if (rTopLeft.Contains(mousePos))
                        EditingHandle = HandleType.TopLeft;
                    else if (rTopCenter.Contains(mousePos))
                        EditingHandle = HandleType.TopCenter;
                    else if (rTopRight.Contains(mousePos))
                        EditingHandle = HandleType.TopRight;
                    else if (rRightCenter.Contains(mousePos))
                        EditingHandle = HandleType.RightCenter;
                    else if (rBottomRight.Contains(mousePos))
                        EditingHandle = HandleType.BottomRight;
                    else if (rBottomCenter.Contains(mousePos))
                        EditingHandle = HandleType.BottomCenter;
                    else if (rBottomLeft.Contains(mousePos))
                        EditingHandle = HandleType.BottomLeft;
                    else if (rLeftCenter.Contains(mousePos))
                        EditingHandle = HandleType.LeftCenter;
                    else if (rAdjustedMiddle.Contains(mousePos)) {
                        EditingHandle = HandleType.Middle;
                        clickedMousePos = mousePos;
                    }
                    else {
                        EditingHandle = HandleType.None;
                    }
                }

                if (eventCurrent.type == EventType.MouseDrag && eventCurrent.type != EventType.MouseUp) {
                    var xPos = (int)eventCurrent.mousePosition.x/spriteScale;
                    var yPos = (int)eventCurrent.mousePosition.y/spriteScale;
                    // switch (EditingHandle) {
                    //     case HandleType.TopLeft:
                    //         frame.hitBoxRect.xMin = xPos;
                    //         frame.hitBoxRect.yMin = yPos;
                    //         break;
                    //     case HandleType.TopCenter:
                    //         frame.hitBoxRect.yMin = yPos;
                    //         break;
                    //     case HandleType.TopRight:
                    //         frame.hitBoxRect.xMax = xPos;
                    //         frame.hitBoxRect.yMin = yPos;
                    //         break;
                    //     case HandleType.RightCenter:
                    //         frame.hitBoxRect.xMax = xPos;
                    //         break;
                    //     case HandleType.BottomRight:
                    //         frame.hitBoxRect.xMax = xPos;
                    //         frame.hitBoxRect.yMax = yPos;
                    //         break;
                    //     case HandleType.BottomCenter:
                    //         frame.hitBoxRect.yMax = yPos;
                    //         break;
                    //     case HandleType.BottomLeft:
                    //         frame.hitBoxRect.xMin = xPos;
                    //         frame.hitBoxRect.yMax = yPos;
                    //         break;
                    //     case HandleType.LeftCenter:
                    //         frame.hitBoxRect.xMin = xPos;
                    //         break;
                    //     case HandleType.Middle:
                    //         var xDelta = (int)((clickedMousePos.x - rect.xMin) / scale);
                    //         var yDelta = (int)((clickedMousePos.y - rect.yMin) / scale);
                            
                    //         var newXPos = xPos - xDelta;
                    //         var newYPos = yPos - yDelta;

                    //         var xSize = (int)rect.size.x / scale;
                    //         var ySize = (int)rect.size.y / scale;

                    //         frame.hitBoxRect.position = new Vector2(newXPos, newYPos);
                    //         frame.hitBoxRect.size = new Vector2(xSize, ySize);
                    //         clickedMousePos = eventCurrent.mousePosition;
                    //         break;
                    //     case HandleType.None:
                    //         break;
                    //     default:
                    //         throw new ArgumentOutOfRangeException(nameof(handleTypes), handleTypes, null);
                    // }
                }

                EditorGUI.DrawRect(rTopLeft, rectColor);
                EditorGUI.DrawRect(rTopCenter, rectColor);
                EditorGUI.DrawRect(rTopRight, rectColor);
                EditorGUI.DrawRect(rRightCenter, rectColor);
                EditorGUI.DrawRect(rBottomRight, rectColor);
                EditorGUI.DrawRect(rBottomCenter, rectColor);
                EditorGUI.DrawRect(rBottomLeft, rectColor);
                EditorGUI.DrawRect(rLeftCenter, rectColor);


                AddCursorRect(rTopLeft, MouseCursor.ResizeUpLeft, HandleType.TopLeft);
                AddCursorRect(rTopCenter, MouseCursor.ResizeVertical, HandleType.TopCenter);
                AddCursorRect(rTopRight, MouseCursor.ResizeUpRight, HandleType.TopRight);
                AddCursorRect(rRightCenter, MouseCursor.ResizeHorizontal, HandleType.RightCenter);
                AddCursorRect(rBottomRight, MouseCursor.ResizeUpLeft, HandleType.BottomRight);
                AddCursorRect(rBottomCenter, MouseCursor.ResizeVertical, HandleType.BottomCenter);
                AddCursorRect(rBottomLeft, MouseCursor.ResizeUpRight, HandleType.BottomLeft);
                AddCursorRect(rLeftCenter, MouseCursor.ResizeHorizontal, HandleType.LeftCenter);
                AddCursorRect(rAdjustedMiddle, MouseCursor.MoveArrow, HandleType.Middle);

                rect.width = Mathf.Clamp(rect.width, 0, int.MaxValue);
                rect.height = Mathf.Clamp(rect.height, 0, int.MaxValue);

                // frame.hitBoxRect.x = Mathf.Clamp(frame.hitBoxRect.x, 0, spriteSize.x - frame.hitBoxRect.width);
                // frame.hitBoxRect.y = Mathf.Clamp(frame.hitBoxRect.y, 0, spriteSize.y - frame.hitBoxRect.height);
                // frame.hitBoxRect.width = Mathf.Clamp(frame.hitBoxRect.width, 0, spriteSize.x - frame.hitBoxRect.x);
                // frame.hitBoxRect.height = Mathf.Clamp(frame.hitBoxRect.height, 0, spriteSize.y - frame.hitBoxRect.y);

                if (eventCurrent.type == EventType.MouseUp) {
                    EditingHandle = HandleType.None;
                }
                    
                var color = new Color(rectColor.r, rectColor.g, rectColor.b, 0.2f);
                Handles.DrawSolidRectangleWithOutline(rect, color, rectColor);
                
            }else{
                Handles.DrawSolidRectangleWithOutline(rect, new Color(0,0,0,0), rectColor);
            }
        }

        private void AddCursorRect(Rect rect, MouseCursor cursor, HandleType type){
            EditorGUIUtility.AddCursorRect(rect, cursor);
            PixelAnimatorWindow.AddCursorBool(EditingHandle == type, cursor);

        }

        public void SetHandle(HandleType handleType){
            EditingHandle = handleType;
        }

        public override void FocusFunctions(){
            MoveOperations();
            DrawFocusOutline();
        }

        private void DrawFocusOutline()
        {
            var largestCanvas = new Rect(windowRect.x - 2, windowRect.y - 2, windowRect.width + 4,
                windowRect.height + 4);  
            EditorGUI.DrawRect(largestCanvas, Color.blue);
        }

    }
}