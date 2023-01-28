using UnityEngine;
using System;
using System.Collections.Generic;

namespace binc.PixelAnimator.Editor.Window {

    [Serializable]
    public class GroupEditorData{
        public Rect bodyRect;
        public List<Rect> layerRects;
        public Rect settingsRect;
        public Rect bottomLine;

        public Rect FoldoutRect => new (bodyRect.xMax - 40, bodyRect.yMax - 20, 20, 10);
        public Rect ColliderTypeRect => new (FoldoutRect.xMax - 45, bodyRect.yMax - 25, 16, 16);
        public Rect VisibleRect => new(ColliderTypeRect.xMax - 45, bodyRect.yMax - 20, 18, 12);

        public GroupEditorData(Rect bodyRect, Rect settingsRect, Rect bottomLine){
            this.bodyRect = bodyRect;
            this.settingsRect = settingsRect;
            this.bottomLine = bottomLine;
        }

        public GroupEditorData(){
            layerRects = new List<Rect>();
        }


    }

}