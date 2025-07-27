using UnityEngine;

namespace binc.PixelAnimator.Editor.Windows
{
    public partial class CanvasWindow : Window
    {
        private Texture2D cachedGridTexture;
        private Vector2 lastGridSize;
        private Color lastBlackColor;
        private Color lastWhiteColor;
        
        private Rect spriteRect;
        private Rect canvasRect;
        private Texture2D spritePreview;
        private int spriteScale;

        private BoxHandleType UsingBoxHandle { get; set; }
        private Vector2 clickedMousePos; 
        private Color blackColor;
        private Color whiteColor; 
        private Vector2 previousMousePosition;
        private Vector2 screenSpriteOrigin, viewOffset = new (0, 0);

        private TimelineWindow timelineWindow;
        private bool isDraggable;

        public override void Initialize(int id){
            Id = id;
            SetTextures();
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            spriteScale = 1;
            UsingBoxHandle = BoxHandleType.None;
            timelineWindow = animatorWindow.GetPixelWindow<TimelineWindow>();
            blackColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            whiteColor = new Color(1, 1, 1, 0.5f);
        }
        private void SetTextures()
        {
            spriteScale = 1;
            UsingBoxHandle = BoxHandleType.None;
            spritePreview = new Texture2D(0,0);
        }
        public override void Dispose()
        {
        }
    }
}

