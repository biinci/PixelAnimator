using System;
using UnityEditor;
using UnityEngine;
using binc.PixelAnimator.Editor.Window;

namespace binc.PixelAnimator.Editor.Window
{

    public abstract class CustomWindow{
        
        protected Rect windowRect;
        public Rect WindowRect => windowRect;
        public Vector2 GlobalMousePos => animatorWindow.EventCurrent.mousePosition + windowRect.position;
        
        protected PixelAnimatorWindow animatorWindow;

        private WindowEnum windowType;
        public bool IsFocusChangeable { get; internal set; }
        public bool IsCursorOnWindow { get; internal set; }
        

        public CustomWindow(PixelAnimatorWindow animatorWindow, WindowEnum windowFocusType) {
            this.animatorWindow = animatorWindow;
            windowType = windowFocusType;

        }

        public abstract void UIOperations();
        public virtual void FocusFunctions() {
            if (animatorWindow.WindowFocus != windowType) return;
            
        }

        public virtual void SetWindow(Event eventCurrent) {
            if (windowRect.Contains(GlobalMousePos)) IsCursorOnWindow = true;

        }



    }
}

