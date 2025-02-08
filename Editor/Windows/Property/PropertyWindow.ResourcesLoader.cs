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
            timelineWindow = PixelAnimatorWindow.AnimatorWindow.GetPixelWindow<TimelineWindow>();
        }
        public override void Dispose()
        {

        }
    }
}