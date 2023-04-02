using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace binc.PixelAnimator.Editor.Window
{

    public class TimelineWindow : CustomWindow{

        private bool timelineDragging;
        public bool TimelineDragging => timelineDragging;
        

        public TimelineWindow(PixelAnimatorWindow animatorWindow) : base(animatorWindow) {}

        public override void SetWindow(){
            
        }


    }
}