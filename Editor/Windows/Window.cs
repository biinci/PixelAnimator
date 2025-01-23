using System;
using UnityEngine;

namespace binc.PixelAnimator.Editor.Windows
{
    [Serializable]
    public abstract class Window{
        
        [SerializeField] protected Rect windowRect;
        public Rect WindowRect => windowRect;
        public PixelAnimation SelectedAnim => PixelAnimatorWindow.AnimatorWindow.SelectedAnimation;
        public int Id{get; internal set;}

        public abstract void Initialize(int id);
        public abstract void Dispose();
        
        /// <summary>
        /// This function works when the window focus is on the window that you inherited this class from.
        /// </summary>
        /// 
        // public abstract void OnFocus();  
        
        public abstract void ProcessWindow();
        
    }
    

}




