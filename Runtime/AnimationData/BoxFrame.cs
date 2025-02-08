using System;
using UnityEngine;

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

        
        
        public MethodStorage<Collider> triggerEnterMethodStorage;
        public MethodStorage<Collider> triggerStayMethodStorage;
        public MethodStorage<Collider> triggerExitMethodStorage;

        public MethodStorage<Collision> collisionEnterMethodStorage;
        public MethodStorage<Collision> collisionStayMethodStorage;
        public MethodStorage<Collision> collisionExitMethodStorage;
        
        public BoxFrame(string guid){
            this.guid = guid;
        }

        public void SetType(BoxFrameType boxFrameType)
        {
            type = boxFrameType;
        }
    }
    
    
}
