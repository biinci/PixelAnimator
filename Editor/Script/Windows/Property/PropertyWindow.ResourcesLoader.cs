using UnityEngine;
using UnityEditor;

namespace binc.PixelAnimator.Editor.Windows{
    public partial class PropertyWindow : Window{

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
            
        }

        public override void Dispose()
        {
            DisposeEvents();
        }

        private void LoadButtonsMethods()
        {
            addEventButton.DownClick += AddEvent;
            removeEventButton.DownClick += RemoveEvent;
        }

        private void DisposeEvents()
        {
            addEventButton.DownClick -= AddEvent;
            removeEventButton.DownClick -= RemoveEvent;
            
        }

    }
}