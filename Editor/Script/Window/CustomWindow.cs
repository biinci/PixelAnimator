using System;
using UnityEditor;
using UnityEngine;
using binc.PixelAnimator.Editor.Window;

namespace binc.PixelAnimator.Editor.Window
{

    public abstract class CustomWindow{
        
        protected Rect windowRect;
        public Rect WindowRect => windowRect;
        protected PixelAnimatorWindow animatorWindow;

        public CustomWindow(PixelAnimatorWindow animatorWindow) {
            this.animatorWindow = animatorWindow;
        }


    }
}

