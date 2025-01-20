using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Serialization;

namespace binc.PixelAnimator.Editor.Windows
{
    public partial class TimelineWindow : Window
    {
        #region Variables
        public bool IsPlaying { get; private set; }

        private Rect handleRect,
            columnRect,
            rowRect,
            groupPlaneRect,
            thumbnailPlaneRect,
            toolPanelRect;

        private Texture2D previousFrameTex,
            playTex,
            nextFrameTex,
            timelineBurgerTex,
            pauseTex,
            playPauseTex,
            selectedFrameTex;
        

        public static  Color WindowPlaneColor = new Color(0.2f, 0.2f, 0.2f);
        public static  Color DarkColor = new Color(0.16f, 0.16f, 0.16f);

        public static  Color AccentColor = new Color(0.17f, 0.36f, 0.53f);
        public static  Color InsideAccentColor = new Color(0.14f, 0.14f, 0.14f);

        private GenericMenu burgerMenu, boxGroupMenu, boxMenu;

        private const float HandleHeight = 4f;

        private float GroupPanelWidth =>
            PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.GetStyle("BoxGroup").fixedWidth;

        private float ToolPanelHeight =>
            PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.GetStyle("Tool").fixedWidth;

        private float ColumnWidth =>
            PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.GetStyle("TimelineLayout").fixedWidth;

        private float RowHeight =>
            PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.GetStyle("TimelineLayout").fixedHeight;

        private GUIStyle groupStyle,
            layerStyle,
            keyFrameStyle,
            emptyFrameStyle,
            copyFrameStyle,
            frameButtonStyle,
            spriteThumbnailStyle,
            animatorButtonStyle,
            timelineStyle;

        private float timer;
        private Vector2 scrollPos;
        private bool reSizing;

        private int loopGroupIndex, loopLayerIndex, loopFrameIndex;
        
        private ButtonData<int> thumbnailButton;
        private ButtonData<BoxGroup> groupButton;
        private ButtonData<ValueTuple<BoxGroup, Box>> layerButton;
        private ButtonData<ValueTuple<int, int, int>> frameButton;
        private ButtonData burgerButton, playPauseButton;
        private ButtonData<bool> previousNextSpriteButton;
        #endregion

        #region Init
        public override void Initialize(int id)
        {
            Id = id;
            LoadInitResources();
            InitRect();
            LoadStyles();
        }
        
        private void LoadInitResources()
        {
            LoadTextures();
            LoadStyles();
            LoadButtonMethods();
        }

        private void LoadButtonMethods()
        {
            thumbnailButton.DownClick += ThumbnailButton;
            groupButton.DownClick += GroupButton;
            layerButton.DownClick += BoxButton;
            frameButton.DownClick += FrameButton;
            burgerButton.DownClick += BurgerMenuButton;
            playPauseButton.DownClick += PlayPauseButton;
            previousNextSpriteButton.DownClick += ChangeSpriteButton;
        }

        private void LoadTextures()
        {
            previousFrameTex = Resources.Load<Texture2D>("Sprites/Back");
            nextFrameTex = Resources.Load<Texture2D>("Sprites/Front");
            timelineBurgerTex = Resources.Load<Texture2D>("Sprites/TimelineBurgerMenu");
            pauseTex = Resources.Load<Texture2D>("Sprites/Pause");
            playTex = Resources.Load<Texture2D>("Sprites/Play");
            selectedFrameTex = Resources.Load<Texture2D>("Sprites/SelectedFrame");
            playPauseTex = playTex;
        }

        private void LoadStyles()
        {
            var mySkin = PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin;
            groupStyle = new GUIStyle(mySkin.GetStyle("BoxGroup"));
            layerStyle = new GUIStyle(mySkin.GetStyle("Layer"));
            keyFrameStyle = new GUIStyle(mySkin.GetStyle("KeyFrame"));
            emptyFrameStyle = new GUIStyle(mySkin.GetStyle("EmptyFrame"));
            copyFrameStyle = new GUIStyle(mySkin.GetStyle("CopyFrame"));
            frameButtonStyle = new GUIStyle(mySkin.GetStyle("FrameButton"));
            spriteThumbnailStyle = new GUIStyle(mySkin.GetStyle("SpriteThumbnail"));
            animatorButtonStyle = new GUIStyle(mySkin.GetStyle("AnimatorButton"));
            timelineStyle = new GUIStyle(mySkin.GetStyle("Timeline"));
            hoverSpriteThumbnailStyle = new GUIStyle(spriteThumbnailStyle) { normal = spriteThumbnailStyle.hover };
        }


        private void InitRect()
        {
            var position = PixelAnimatorWindow.AnimatorWindow.position;
            windowRect.x = 0;
            windowRect.y = position.yMax - windowRect.height;
        }

        #endregion
        
        public override void Dispose()
        {
            new Color(0.1f, 0.1f, 0.1f);
            thumbnailButton.DownClick -= ThumbnailButton;
            groupButton.DownClick -= GroupButton;
            layerButton.DownClick -= BoxButton;
            frameButton.DownClick -= FrameButton;
            burgerButton.DownClick -= BurgerMenuButton;
            playPauseButton.DownClick -= PlayPauseButton;
            previousNextSpriteButton.DownClick -= ChangeSpriteButton;
        }
    }
}