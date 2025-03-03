using System;
using UnityEngine;
using binc.PixelAnimator.DataManipulations;

namespace binc.PixelAnimator.AnimationData{
    
    public enum BoxFrameType{ KeyFrame, CopyFrame, EmptyFrame, None }
    
    [Serializable]
    public class BoxFrame{
        [ReadOnly, SerializeField]
        private string guid;
        
        public BoxFrameType Type => type;
        [SerializeField] private BoxFrameType type;
        
        public Rect boxRect;
        public MethodStorage methodStorage;
        
        
        
        public MethodStorage<Collider2D> triggerEnterMethodStorage;
        public MethodStorage<Collider2D> triggerStayMethodStorage;
        public MethodStorage<Collider2D> triggerExitMethodStorage;

        public MethodStorage<Collision2D> collisionEnterMethodStorage;
        public MethodStorage<Collision2D> collisionStayMethodStorage;
        public MethodStorage<Collision2D> collisionExitMethodStorage;
        
        public BoxFrame(string guid){
            this.guid = guid;
        }

        public void SetType(BoxFrameType boxFrameType)
        {
            type = boxFrameType;
        }
    }
    
    
}
