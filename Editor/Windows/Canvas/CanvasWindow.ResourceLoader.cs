using UnityEditor;
using UnityEngine;

namespace binc.PixelAnimator.Editor.Windows
{
    public partial class CanvasWindow : Window
    {
        private Rect spriteRect;
        private Rect canvasRect;
        private Texture2D spritePreview;
        private int spriteScale;

        private BoxHandleType UsingBoxHandle { get; set; }
        private Vector2 clickedMousePos; 
        private Color blackColor;
        private Color whiteColor; 
        private Texture2D gridWhiteTex;
        private Texture2D gridBlackTex;
        private Vector2 previousMousePosition;
        

        private TimelineWindow timelineWindow;
        private bool isDraggable;

        public override void Initialize(int id){
            Id = id;
            SetTextures();
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            spriteScale = 1;
            UsingBoxHandle = BoxHandleType.None;
            timelineWindow = animatorWindow.GetWindow<TimelineWindow>();
        }

        private void SetTextures()
        {
            SetGridTexture();
            spriteScale = 1;
            UsingBoxHandle = BoxHandleType.None;
            spritePreview = new Texture2D(0,0);
        }

        private void SetGridTexture()
        {
            blackColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            whiteColor = new Color(1, 1, 1, 0.5f);
            gridWhiteTex = new Texture2D(1, 1);
            gridWhiteTex.SetPixel(0, 0, whiteColor);
            gridWhiteTex.Apply();
            gridBlackTex = new Texture2D(1, 1);
            gridBlackTex.SetPixel(0, 0, blackColor);
            gridBlackTex.Apply();
        }
        public override void Dispose()
        {
        }


    }
    
}

