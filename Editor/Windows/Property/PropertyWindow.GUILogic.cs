using UnityEngine;

namespace binc.PixelAnimator.Editor.Windows
{
    public partial class PropertyWindow
    {

        public override void ProcessWindow()
        {
            if (!timelineWindow.IsPlaying) DrawPropertyWindow();
            if (windowRect.IsClickedRect(0))
            {
                Event.current.Use();
                GUI.FocusControl(null);
                GUI.FocusWindow(Id);
            }
        }
        
        
    }
}