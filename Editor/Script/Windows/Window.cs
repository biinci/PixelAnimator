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

        public bool IsFocusChangeable { get; internal set; }
        public bool IsCursorOnWindow { get; internal set; }
        

        public Window() {
        }

        public abstract void UIOperations();



        /// <summary>
        /// This function works when the window focus is on the window from which you inherited this class.
        /// </summary>
        public abstract void FocusFunctions();  
            
            
    
        public virtual void SetWindow(Event eventCurrent) {
            if (windowRect.Contains(GlobalMousePos)) IsCursorOnWindow = true;

        }


    }
}

