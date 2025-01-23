using UnityEngine;
using UnityEditor;
using System;

namespace binc.PixelAnimator.Editor.Windows
{
    public partial class TimelineWindow : Window
    {
        #region Variables
        public bool IsPlaying { get; private set; }

        private Rect handleRect,
            columnRect,
            rowRect,
            groupAreaRect,
            thumbnailPlaneRect,
            toolPanelRect,
            handleShadowRect,
            frameAreaRect;

        private Texture2D prevFrameTex,
            playTex,
            nextFrameTex,
            mainMenuTex,
            pauseTex,
            playPauseTex,
            selectedFrameTex,
            keyFrameTex,
            copyFrameTex,
            emptyFrameTex;
        

        public static Color windowPlaneColor = new(0.2f, 0.2f, 0.2f);
        public static Color darkColor = new(0.16f, 0.16f, 0.16f);
        public static Color accentColor = new(0.17f, 0.36f, 0.53f);
        public static Color insideAccentColor = new(0.14f, 0.14f, 0.14f);

        private GenericMenu burgerMenu, boxGroupMenu, boxMenu;

        private const float HandleHeight = 3f;

        private float GroupPanelWidth =>
            PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin.GetStyle("BoxGroup").fixedWidth;

        private Vector2Int toolBarSize = new(32,32);

        private const float ColumnWidth = 3;

        private const float RowHeight = 3;

        private GUIStyle
            groupStyle,
            layerStyle,
            animatorButtonStyle;
        
        private float timer;
        private Vector2 scrollPosition;
        private bool reSizing;

        private int loopGroupIndex, loopLayerIndex, loopFrameIndex;
        
        private ButtonData<int> thumbnailButton;
        private ButtonData<BoxGroup> groupButton;
        private ButtonData<ValueTuple<BoxGroup, Box>> layerButton;
        private ButtonData<ValueTuple<int, int, int>> frameButton;
        private ButtonData mainMenuButton, playPauseButton, pingAnimationButton;
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
            mainMenuButton.DownClick += BurgerMenuButton;
            playPauseButton.DownClick += PlayPauseButton;
            previousNextSpriteButton.DownClick += ChangeSpriteButton;
            pingAnimationButton.DownClick += PingAnimationButton;
        }

        private void LoadTextures()
        {
            prevFrameTex = Resources.Load<Texture2D>("Sprites/Back");
            nextFrameTex = Resources.Load<Texture2D>("Sprites/Front");
            mainMenuTex = Resources.Load<Texture2D>("Sprites/MainMenu");
            pauseTex = Resources.Load<Texture2D>("Sprites/Pause");
            playTex = Resources.Load<Texture2D>("Sprites/Play");
            selectedFrameTex = Resources.Load<Texture2D>("Sprites/SelectedFrame");
            keyFrameTex = Resources.Load<Texture2D>("Sprites/Key Frame");
            copyFrameTex = Resources.Load<Texture2D>("Sprites/Copy Frame");
            emptyFrameTex = Resources.Load<Texture2D>("Sprites/Empty Frame");
            playPauseTex = playTex;
        }

        private void LoadStyles()
        {
            var mySkin = PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin;
            groupStyle = new GUIStyle(mySkin.GetStyle("BoxGroup"));
            layerStyle = new GUIStyle(mySkin.GetStyle("Layer"));
            animatorButtonStyle = new GUIStyle(mySkin.GetStyle("AnimatorButton"));
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
            thumbnailButton.DownClick -= ThumbnailButton;
            groupButton.DownClick -= GroupButton;
            layerButton.DownClick -= BoxButton;
            frameButton.DownClick -= FrameButton;
            mainMenuButton.DownClick -= BurgerMenuButton;
            playPauseButton.DownClick -= PlayPauseButton;
            previousNextSpriteButton.DownClick -= ChangeSpriteButton;
            pingAnimationButton.DownClick -= PingAnimationButton;
        }
    }
}