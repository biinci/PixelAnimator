using System;
using UnityEngine;
using UnityEditor;



namespace binc.PixelAnimator.Editor.Window{

    
    
    public class CanvasWindow{
        private Rect canvasRect;
        private Rect CanvasRect => canvasRect;
        private Sprite sprite;
        private float spriteScale;




        public void DrawCanvas(Event eventCurrent, Sprite sprite){
            var spritePreview = AssetPreview.GetAssetPreview(sprite);
            SetZoom(eventCurrent);

        }


        private void SetZoom(Event eventCurrent){
            if (eventCurrent.button == 2) viewOffset += eventCurrent.delta * 0.5f; // <== Middle Click Move.
            if (eventCurrent.type == EventType.ScrollWheel) {
                var inversedDelta = Mathf.Sign(eventCurrent.delta.y) < 0 ? 1 : -1;
                spriteScale += inversedDelta;
            }

            spriteScale = Mathf.Clamp(spriteScale, 1, (int)(position.height / spritePreview.height));

            canvasRect.position = new Vector2(viewOffset.x + spriteOrigin.x, viewOffset.y + spriteOrigin.y);
            canvasRect.size = new Vector2(spritePreview.width * spriteScale, spritePreview.height * spriteScale);
            
        }



    }

    

}