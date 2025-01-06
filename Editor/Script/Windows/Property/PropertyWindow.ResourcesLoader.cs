using System;
using UnityEngine;


namespace binc.PixelAnimator.Editor.Windows{
    [Serializable]
    public partial class PropertyWindow : Window{

        private TimelineWindow timelineWindow;
        
        private Vector2 scrollPos;
        private static string[] _tabTitles = { "Sprite", "Hitbox" };
        private int selectedTab;
        public override void Initialize(int id)
        {
            Id = id;
            InitList();
            timelineWindow = PixelAnimatorWindow.AnimatorWindow.GetWindow<TimelineWindow>();
            
            
        }
        
        private void InitList()
        {
            

        }

        public override void Dispose()
        {

        }

        

    }
    

    
}

