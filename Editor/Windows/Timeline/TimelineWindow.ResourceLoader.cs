using UnityEngine;
using UnityEditor;
using System;
using binc.PixelAnimator.AnimationData;
namespace binc.PixelAnimator.Editor.Windows
{
    public partial class TimelineWindow : Window
    {
        #region Variables
        
        private float playTimer;
        private double lastPlayTime;
        private bool isPlaying;
        public bool IsPlaying 
        { 
            get => isPlaying;
            private set
            {
                if (isPlaying == value) return;
                isPlaying = value;
                
                if (isPlaying)
                {
                    EditorApplication.update += UpdatePlayback;
                    lastPlayTime = EditorApplication.timeSinceStartup;
                    playTimer = 0f;
                }
                else
                {
                    EditorApplication.update -= UpdatePlayback;
                }
            }
        }
        
        private Rect handleRect,
            columnRect,
            rowRect,
            bottomAreaRect,
            thumbnailPanelRect,
            toolPanelRect,
            handleShadowRect,
            frameAreaRect;

        private Texture2D prevFrameTex,
            playTex,
            nextFrameTex,
            mainMenuTex,
            targetAnimTex,
            pauseTex,
            playPauseTex,
            selectedFrameTex,
            keyFrameTex,
            copyFrameTex,
            emptyFrameTex,
            visibleTex,
            invisibleTex,
            groupOptionsTex,
            upTex,
            downTex,
            triggerTex,
            colliderTex,
            linkFrameTex;
        

        public static Color windowPlaneColor = new(0.2f, 0.2f, 0.2f);
        public static Color darkColor = new(0.16f, 0.16f, 0.16f);
        public static Color accentColor = new(0.17f, 0.36f, 0.53f);
        public static Color insideAccentColor = new(0.14f, 0.14f, 0.14f);

        private GenericMenu burgerMenu, boxGroupMenu, boxMenu;

        private const float HandleHeight = 3f;

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

        private int loopBoxGroupIndex, loopBoxIndex, loopFrameIndex;
        
        private ButtonData<Tuple<int, Sprite>> thumbnailButton;
        private ButtonData<BoxGroup> groupButton;
        private ButtonData<ValueTuple<BoxGroup, BoxLayer>> layerButton;
        private ButtonData<ValueTuple<int, int, int>> frameButton;
        private ButtonData mainMenuButton, playPauseButton, pingAnimationButton;
        private ButtonData<bool> previousNextSpriteButton;
        private ButtonData<BoxGroup> visibilityButton;
        private ButtonData<BoxGroup> expandGroupButton;
        private ButtonData<BoxGroup> colliderButton;

        #endregion

        #region Init
        public override void Initialize(int id)
        {
            Id = id;
            LoadInitResources();
            InitRect();
            LoadStyles();
            IsPlaying = false;
        }
        
        private void LoadInitResources()
        {
            LoadTextures();
            LoadStyles();
            LoadButtonMethods();
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
            targetAnimTex = Resources.Load<Texture2D>("Sprites/TargetAnim");
            visibleTex = Resources.Load<Texture2D>("Sprites/visible");
            invisibleTex = Resources.Load<Texture2D>("Sprites/invisible");
            groupOptionsTex = Resources.Load<Texture2D>("Sprites/groupOptions");
            upTex = Resources.Load<Texture2D>("Sprites/up");
            downTex = Resources.Load<Texture2D>("Sprites/drop");
            triggerTex = Resources.Load<Texture2D>("Sprites/trigger");
            colliderTex = Resources.Load<Texture2D>("Sprites/collider");
            linkFrameTex = Resources.Load<Texture2D>("Sprites/linkframe");

            playPauseTex = playTex;
        }

        private void LoadStyles()
        {
            var mySkin = PixelAnimatorWindow.AnimatorWindow.PixelAnimatorSkin;
            groupStyle = mySkin.GetStyle("BoxGroup");
            layerStyle = mySkin.GetStyle("BoxLayer");
            animatorButtonStyle = mySkin.GetStyle("AnimatorButton");
        }
        
        private void InitRect()
        {
            var position = PixelAnimatorWindow.AnimatorWindow.position;
            windowRect.x = 0;
            windowRect.y = position.yMax - windowRect.height;
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
            visibilityButton.DownClick += VisibilityButton;
            expandGroupButton.DownClick += ExpandGroupButton;
            colliderButton.DownClick += ColliderButton;

        }

        #endregion
        
        private void UpdatePlayback()
        {
            if (!IsPlaying || !SelectedAnim) return;
        
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var fps = SelectedAnim.fps;
            var count = animatorWindow.GetSpriteCount();

            if (fps <= 0 || count <= 0)
            {
                IsPlaying = false; 
                return;
            }

            var currentTime = EditorApplication.timeSinceStartup;
            var deltaTime = (float)(currentTime - lastPlayTime);
            lastPlayTime = currentTime;
        
            playTimer += deltaTime;
            var frameInterval = 1f / fps;

            if (!(playTimer >= frameInterval)) return;
            playTimer -= frameInterval;
            
            var frame = (animatorWindow.IndexOfSelectedSprite + 1) % count;
            animatorWindow.SelectSprite(frame);
            
            animatorWindow.Repaint();
        }
        
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
            visibilityButton.DownClick -= VisibilityButton;
            expandGroupButton.DownClick -= ExpandGroupButton;
            colliderButton.DownClick -= ColliderButton;
            EditorApplication.update -= UpdatePlayback;

        }
        
        public void OnDisable()
        {
            thumbnailButton.DownClick -= ThumbnailButton;
            groupButton.DownClick -= GroupButton;
            layerButton.DownClick -= BoxButton;
            frameButton.DownClick -= FrameButton;
            mainMenuButton.DownClick -= BurgerMenuButton;
            playPauseButton.DownClick -= PlayPauseButton;
            previousNextSpriteButton.DownClick -= ChangeSpriteButton;
            pingAnimationButton.DownClick -= PingAnimationButton;
            visibilityButton.DownClick -= VisibilityButton;
            expandGroupButton.DownClick -= ExpandGroupButton;
            colliderButton.DownClick -= ColliderButton;

        }
        
    }
}