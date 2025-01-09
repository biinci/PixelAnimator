using System;
using UnityEngine;

namespace binc.PixelAnimator.Common{
    
    public enum BoxFrameType{ KeyFrame, CopyFrame, EmptyFrame, None }
    
    [Serializable]
    public class BoxFrame{
        [ReadOnly, SerializeField]
        private string guid;
        
        [SerializeField] private BoxFrameType type;
        
        public Rect boxRect;
        public MethodStorage methodStorage;
        public BoxFrame(string guid){
            this.guid = guid;
        }

        public void SetType(BoxFrameType boxFrameType){
            type = boxFrameType;
        }
        
        public BoxFrameType GetFrameType() => type;
    }
    
    
}
