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

        private BoxHandleType EditingBoxHandle { get; set; }
        private Vector2 clickedMousePos; 
        private Color blackColor;
        private Color whiteColor; 
        private Texture2D gridWhiteTex;
        private Texture2D gridBlackTex;
        private Vector2 previousMousePosition;
        

        private TimelineWindow timelineWindow;

        public override void Initialize(int id){
            Id = id;
            SetTextures();
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            spriteScale = 1;
            EditingBoxHandle = BoxHandleType.None;
            timelineWindow = animatorWindow.GetWindow<TimelineWindow>();
        }

        private void SetTextures()
        {
            blackColor = new Color(0.5f, 0.5f, 0.5f);
            whiteColor = new Color(0.75f, 0.75f, 0.75f);
            gridBlackTex = new Texture2D(1,1);
            gridWhiteTex = new Texture2D(1,1);
            gridBlackTex.SetPixel(0,0,blackColor);
            gridWhiteTex.SetPixel(0,0,whiteColor);
            gridBlackTex.Apply();
            gridWhiteTex.Apply();
            spriteScale = 1;
            EditingBoxHandle = BoxHandleType.None;
        }

        public override void Dispose()
        {
        }

        public override void ProcessWindow()
        {
            windowRect = new Rect(Vector2.zero,
                new Vector2(PixelAnimatorWindow.AnimatorWindow.position.width, timelineWindow.WindowRect.y));
            
            var isValid = SelectedAnim && SelectedAnim.GetSpriteList() != null;
            if(!isValid) return;
            if(SelectedAnim.GetSpriteList().Count > 0)SetSpritePreview();
            if(!spritePreview) return;
            DrawCanvas();
            SetRect();
            FocusToCanvas(); //Bad naming
        }
    }
    
}

