using System;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Common;
using binc.PixelAnimator.Preferences;
 



namespace binc.PixelAnimator.Editor.Window{

    
    
    public class CanvasWindow{
        
        private Rect canvasRect;
        private Rect CanvasRect => canvasRect;
        private PixelAnimation pixelAnim;
        private PixelAnimatorPreferences preferences;
        private PixelAnimatorWindow window;
        
        private Sprite sprite;
        private Vector2 spriteOrigin;
        private Texture2D spritePreview;
        private int spriteScale;
        private Vector2 viewOffset;

        private HandleTypes editingHandle;
        private Vector2 _clickedMousePos;
        private Color blackColor = new Color(0.5f, 0.5f, 0.5f);
        private Color whiteColor = new Color(0.75f, 0.75f, 0.75f);
        private Texture2D gridWhiteTex = new Texture2D(1,1);
        private Texture2D gridBlackTex = new Texture2D(1,1);
        

        public CanvasWindow(PixelAnimatorPreferences preferences, PixelAnimatorWindow window){
            this.preferences = preferences;
            this.window = window;

            gridWhiteTex.SetPixel(0, 0, whiteColor);
            gridWhiteTex.Apply();

            gridBlackTex.SetPixel(0, 0, blackColor);
            gridBlackTex.Apply();
        }

        public void SetCanvas(Event eventCurrent, Sprite sprite, Rect editorRect){
            spritePreview = AssetPreview.GetAssetPreview(sprite);
            SetZoom(eventCurrent, editorRect);
            GUI.Window(1, canvasRect, _=>{DrawCanvas();}, GUIContent.none, GUIStyle.none);
            UpdateScale(editorRect);

        }


        private void SetZoom(Event eventCurrent, Rect editorRect){
            if (eventCurrent.button == 2) viewOffset += eventCurrent.delta * 0.5f; // <== Middle Click Move.
            if (eventCurrent.type == EventType.ScrollWheel) {
                var inversedDelta = Mathf.Sign(eventCurrent.delta.y) < 0 ? 1 : -1;
                spriteScale += inversedDelta;
            }

            spriteScale = Mathf.Clamp(spriteScale, 1, (int)(editorRect.height / spritePreview.height));

            canvasRect.position = new Vector2(viewOffset.x + spriteOrigin.x, viewOffset.y + spriteOrigin.y);
            canvasRect.size = new Vector2(spritePreview.width * spriteScale, spritePreview.height * spriteScale);
            
        }


        private void UpdateScale(Rect editorRect){
            var adjustedSpriteWidth = spritePreview.width * spriteScale;
            var adjustedSpriteHeight = spritePreview.height * spriteScale;
            var adjustedPosition = new Rect(Vector2.zero, editorRect.size);
            adjustedPosition.width += 10;
            adjustedPosition.height -= adjustedPosition.yMax ; //- timelineRect.y
            spriteOrigin.x = adjustedPosition.width * 0.5f - spritePreview.width * 0.5f * spriteScale;
            spriteOrigin.y = adjustedPosition.height * 0.5f - spritePreview.height * 0.5f * spriteScale;

            //handle the canvas view bounds X
            if (viewOffset.x > adjustedSpriteWidth * 0.5f)
                viewOffset.x = adjustedSpriteWidth * 0.5f;
            if (viewOffset.x < -adjustedSpriteWidth * 0.5f)
                viewOffset.x = -adjustedSpriteWidth * 0.5f;

            //handle the canvas view bounds Y
            if (viewOffset.y > adjustedSpriteHeight * 0.5f)
                viewOffset.y = adjustedSpriteHeight * 0.5f;
            if (viewOffset.y < -adjustedSpriteHeight * 0.5f)
                viewOffset.y = -adjustedSpriteHeight * 0.5f;
        }

        private void DrawCanvas(){
            var spriteRect = new Rect(0, 0, spritePreview.width * spriteScale, spritePreview.height * spriteScale);

            DrawGrid(spriteRect, spritePreview, spriteScale);
            GUI.DrawTexture(spriteRect, spritePreview, ScaleMode.ScaleToFit); //our sprite
            spritePreview.filterMode = FilterMode.Point;

            // if (pixelAnim.Groups.Count > 0)
            foreach (var group in window.SelectedAnim.Groups) {
                // DrawBox(group, preferences.GetBoxData(group.BoxDataGuid), spriteScale, 
                //    new Vector2(spritePreview.width, spritePreview.height),
                    // editingHandle);
            }
        }


        public void DrawGrid(Rect rect, Texture2D spritePreview, int spriteScale){
            var grid = new Rect(rect.x, rect.y, 16 * spriteScale, 16 * spriteScale); //define a single 16x16 tile

            for (var i = 0; i < spritePreview.width / 16; i++) {


                for (var j = 0; j < spritePreview.height / 16; j += 2) {

                    var tex = i % 2 == 0 ? gridWhiteTex : gridBlackTex;
                    GUI.DrawTexture(grid, tex);
                    grid.y += grid.height; 
                    var texTwo = tex == gridWhiteTex ? gridBlackTex : gridWhiteTex;
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
                GUI.DrawTexture(grid, gridBlackTex); 
                grid.y += grid.height;
                GUI.DrawTexture(grid, gridWhiteTex);
                grid.y += grid.height;
            }

            grid.height = rect.y + rect.height - grid.y;
            if (rect.y + rect.height - grid.y > 0) GUI.DrawTexture(grid, gridBlackTex); 
        }


        private void DrawBox(Group group, BoxData boxData, int scale, Vector2 spriteSize,
            HandleTypes handleTypes){
            if (!group.isVisible) return;
            if (window.PropertyFocus != Window.PropertyFocusEnum.HitBox) return;
            var eventCurrent = Event.current;
            var rectColor = boxData.color;
            
            var isActiveGroup = window.SelectedAnim.Groups.IndexOf(group) == window.ActiveGroupIndex;
            for(var l  = 0; l < group.layers.Count; l++){
                
                var isActiveLayer = l == window.ActiveLayerIndex;
                var isBoxActive = isActiveGroup && isActiveLayer && group.isExpanded;//TODO: get the active group/layer data
                

                var frame = group.layers[l].frames[window.ActiveFrameIndex];// Getting the active frame on all layers
                if(frame.frameType == FrameType.EmptyFrame) continue;
                var rect = frame.hitBoxRect; //Getting frame rect for the drawing.
                rect.position *= scale; //Changing rect position and size for zoom.
                rect.size *= scale;
                

                if (isBoxActive) {

                    const float handleSize = 8.5f; // r = rect
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

                    if (eventCurrent.button == 0 && eventCurrent.type == EventType.MouseDown) {
                        if (rTopLeft.Contains(eventCurrent.mousePosition))
                            editingHandle = HandleTypes.TopLeft;
                        else if (rTopCenter.Contains(eventCurrent.mousePosition))
                            editingHandle = HandleTypes.TopCenter;
                        else if (rTopRight.Contains(eventCurrent.mousePosition))
                            editingHandle = HandleTypes.TopRight;
                        else if (rRightCenter.Contains(eventCurrent.mousePosition))
                            editingHandle = HandleTypes.RightCenter;
                        else if (rBottomRight.Contains(eventCurrent.mousePosition))
                            editingHandle = HandleTypes.BottomRight;
                        else if (rBottomCenter.Contains(eventCurrent.mousePosition))
                            editingHandle = HandleTypes.BottomCenter;
                        else if (rBottomLeft.Contains(eventCurrent.mousePosition))
                            editingHandle = HandleTypes.BottomLeft;
                        else if (rLeftCenter.Contains(eventCurrent.mousePosition))
                            editingHandle = HandleTypes.LeftCenter;
                        else if (rAdjustedMiddle.Contains(eventCurrent.mousePosition)) {
                            editingHandle = HandleTypes.Middle;
                            _clickedMousePos = eventCurrent.mousePosition;
                        }
                        else {
                            editingHandle = HandleTypes.None;
                        }
                    }

                    if (eventCurrent.type == EventType.MouseDrag && eventCurrent.type != EventType.MouseUp) {
                        switch (editingHandle) {
                            case HandleTypes.TopLeft:
                                frame.hitBoxRect.xMin = (int)eventCurrent.mousePosition.x / scale;
                                frame.hitBoxRect.yMin = (int)eventCurrent.mousePosition.y / scale;
                                break;
                            case HandleTypes.TopCenter:
                                frame.hitBoxRect.yMin = (int)eventCurrent.mousePosition.y / scale;
                                break;
                            case HandleTypes.TopRight:
                                frame.hitBoxRect.xMax = (int)eventCurrent.mousePosition.x / scale;
                                frame.hitBoxRect.yMin = (int)eventCurrent.mousePosition.y / scale;
                                break;
                            case HandleTypes.RightCenter:
                                frame.hitBoxRect.xMax = (int)eventCurrent.mousePosition.x / scale;
                                break;
                            case HandleTypes.BottomRight:
                                frame.hitBoxRect.xMax = (int)eventCurrent.mousePosition.x / scale;
                                frame.hitBoxRect.yMax = (int)eventCurrent.mousePosition.y / scale;
                                break;
                            case HandleTypes.BottomCenter:
                                frame.hitBoxRect.yMax = (int)eventCurrent.mousePosition.y / scale;
                                break;
                            case HandleTypes.BottomLeft:
                                frame.hitBoxRect.xMin = (int)eventCurrent.mousePosition.x / scale;
                                frame.hitBoxRect.yMax = (int)eventCurrent.mousePosition.y / scale;
                                break;
                            case HandleTypes.LeftCenter:
                                frame.hitBoxRect.xMin = (int)eventCurrent.mousePosition.x / scale;
                                break;
                            case HandleTypes.Middle:
                                var deltaX = (_clickedMousePos.x - rect.xMin) / scale;
                                var deltaY = (_clickedMousePos.y - rect.yMin) / scale;
                                
                                var posX = (int)eventCurrent.mousePosition.x / scale - (int)deltaX;
                                var posY = (int)eventCurrent.mousePosition.y / scale - (int)deltaY;

                                var sizeX = (int)rect.size.x / scale;
                                var sizeY = (int)rect.size.y / scale;

                                frame.hitBoxRect.position = new Vector2(posX, posY);
                                frame.hitBoxRect.size = new Vector2(sizeX, sizeY);
                                _clickedMousePos = eventCurrent.mousePosition;
                                break;
                            case HandleTypes.None:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(handleTypes), handleTypes, null);
                        }
                    }

                    EditorGUI.DrawRect(rTopLeft, rectColor);
                    EditorGUI.DrawRect(rTopCenter, rectColor);
                    EditorGUI.DrawRect(rTopRight, rectColor);
                    EditorGUI.DrawRect(rRightCenter, rectColor);
                    EditorGUI.DrawRect(rBottomRight, rectColor);
                    EditorGUI.DrawRect(rBottomCenter, rectColor);
                    EditorGUI.DrawRect(rBottomLeft, rectColor);
                    EditorGUI.DrawRect(rLeftCenter, rectColor);


                    EditorGUIUtility.AddCursorRect(rTopLeft, MouseCursor.ResizeUpLeft);
                    EditorGUIUtility.AddCursorRect(rTopCenter, MouseCursor.ResizeVertical);
                    EditorGUIUtility.AddCursorRect(rTopRight, MouseCursor.ResizeUpRight);
                    EditorGUIUtility.AddCursorRect(rRightCenter, MouseCursor.ResizeHorizontal);
                    EditorGUIUtility.AddCursorRect(rBottomRight, MouseCursor.ResizeUpLeft);
                    EditorGUIUtility.AddCursorRect(rBottomCenter, MouseCursor.ResizeVertical);
                    EditorGUIUtility.AddCursorRect(rBottomLeft, MouseCursor.ResizeUpRight);
                    EditorGUIUtility.AddCursorRect(rLeftCenter, MouseCursor.ResizeHorizontal);
                    EditorGUIUtility.AddCursorRect(rAdjustedMiddle, MouseCursor.MoveArrow);

                    rect.width = Mathf.Clamp(rect.width, 0, int.MaxValue);
                    rect.height = Mathf.Clamp(rect.height, 0, int.MaxValue);

                    frame.hitBoxRect.x = Mathf.Clamp(frame.hitBoxRect.x, 0, spriteSize.x - frame.hitBoxRect.width);
                    frame.hitBoxRect.y = Mathf.Clamp(frame.hitBoxRect.y, 0, spriteSize.y - frame.hitBoxRect.height);
                    frame.hitBoxRect.width = Mathf.Clamp(frame.hitBoxRect.width, 0, spriteSize.x - frame.hitBoxRect.x);
                    frame.hitBoxRect.height = Mathf.Clamp(frame.hitBoxRect.height, 0, spriteSize.y - frame.hitBoxRect.y);

                    if (eventCurrent.type == EventType.MouseUp) {
                        editingHandle = HandleTypes.None;
                    }
                    
                }   
                var color = isBoxActive ? new Color(rectColor.r, rectColor.g, rectColor.b, 0.2f) : Color.clear;
                Handles.DrawSolidRectangleWithOutline(rect, color, rectColor);
                
            }
        }

        private void SetHandle(HandleTypes handleType){
            editingHandle = handleType;
        }

    
    }
}