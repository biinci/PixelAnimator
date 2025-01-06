using UnityEngine;

namespace binc.PixelAnimator.Editor.Windows
{
    public partial class CanvasWindow : Window
    {
        private Rect spriteRect;
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
            var availableSpace = animatorWindow.AvailableSpace;
            var x = availableSpace.x + availableSpace.width/2;
            var y = availableSpace.y + availableSpace.height/2;
            windowRect.position = new Vector2(x, y);
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
            var availableSpace = PixelAnimatorWindow.AnimatorWindow.AvailableSpace;
            var x = availableSpace.x + availableSpace.width/2;
            var y = availableSpace.y + availableSpace.height/2;
            windowRect.position = new Vector2(x, y);
            spriteScale = 1;
            EditingBoxHandle = BoxHandleType.None;


        }

        public override void Dispose()
        {
        }

        public override void ProcessWindow(){

            var anim = PixelAnimatorWindow.AnimatorWindow.SelectedAnimation;
            var isValid = anim && anim.GetSpriteList() != null;
            if(!isValid) return;
            if(anim.GetSpriteList().Count > 0)SetSpritePreview();
            if(!spritePreview) return;
            SetRect();
            FocusToCanvas(); //Bad naming
            DrawCanvas();
            // SetBox();
        }
    }
    

    
    
    
    
}

