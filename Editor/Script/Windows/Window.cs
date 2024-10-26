using System;
using UnityEditor;
using UnityEngine;

namespace binc.PixelAnimator.Editor.Windows
{
    [Serializable]
    public abstract class Window{
        
        protected Rect windowRect;
        public Rect WindowRect => windowRect;
        public PixelAnimation SelectedAnim => PixelAnimatorWindow.AnimatorWindow.SelectedAnimation;
        public bool FocusChangeable { get; internal set; }
        public int Id{get; internal set;}


        protected Window() {
        }

        public abstract void Initialize(int id);


        /// <summary>
        /// This function works when the window focus is on the window from which you inherited this class.
        /// </summary>
        /// 
        public abstract void FocusFunctions();  
                    
    
        public abstract void ProcessWindow();


    }

    internal interface IUpdate{
        void InspectorUpdate();
    }

}




