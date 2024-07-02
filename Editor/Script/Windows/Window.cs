using System;
using UnityEditor;
using UnityEngine;

namespace binc.PixelAnimator.Editor.Windows
{
    [Serializable]
    public abstract class Window{
        
        protected Rect windowRect;
        public Rect WindowRect => windowRect;
        public Vector2 GlobalMousePos => PixelAnimatorWindow.AnimatorWindow.EventCurrent.mousePosition + windowRect.position;
        public PixelAnimation SelectedAnim => PixelAnimatorWindow.AnimatorWindow.SelectedAnimation;
        public bool FocusChangeable { get; internal set; }
    
        public Window() {
        }

        /// <summary>
        /// This function works when the window focus is on the window from which you inherited this class.
        /// </summary>
        public abstract void FocusFunctions();  
                    
    
        public abstract void DrawWindow(Event eventCurrent);


    }

}

