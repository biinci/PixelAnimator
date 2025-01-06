using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using Object = UnityEngine.Object;


namespace binc.PixelAnimator.Editor.Windows{
    [Serializable]
    public partial class PropertyWindow : Window{
<<<<<<< Updated upstream

        private Vector2 propertyScrollPos = Vector2.one;
        private Rect propertyWindowRect = new(10, 6, 120, 20);
        private bool eventFoldout;
        private ButtonData<SerializedProperty> addEventButton;
        private ButtonData<(SerializedProperty, int)> removeEventButton;
        private TimelineWindow timelineWindow;
        public override void Initialize(int id)
        {
            Id = id;
            LoadButtonsMethods();
            timelineWindow = PixelAnimatorWindow.AnimatorWindow.GetWindow<TimelineWindow>();
=======
        
        private TimelineWindow timelineWindow;
        
        private Vector2 scrollPos;
        private static string[] _tabTitles = { "Sprite", "Hitbox" };
        private int selectedTab;
        public override void Initialize(int id)
        {
            Id = id;
            InitList();
            timelineWindow = PixelAnimatorWindow.AnimatorWindow.GetUsingWindow<TimelineWindow>();
>>>>>>> Stashed changes
            
            
        }
        
        private void InitList()
        {
            

        }

        public override void Dispose()
        {

        }

<<<<<<< Updated upstream
        private void LoadButtonsMethods()
        {
            addEventButton.DownClick += AddEvent;
            removeEventButton.DownClick += RemoveEvent;
        }
=======
>>>>>>> Stashed changes

        

    }
    

    
}

