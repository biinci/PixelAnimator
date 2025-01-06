using System;
using UnityEngine;

namespace binc.PixelAnimator.Common{
    
    public enum FrameType{ KeyFrame, CopyFrame, EmptyFrame, None }
    
    [Serializable]
    public class Frame{
        [ReadOnly, SerializeField]
        private string guid;
        
        [SerializeField] private FrameType type;
        
        public Rect hitBoxRect;
        public MethodStorage methodStorage;
        public Frame(string guid){
            this.guid = guid;

        }

        public void SetType(FrameType frameType){
            type = frameType;
        }


        public FrameType GetFrameType() => type;
        

    }




    [Serializable]
    public class PixelSprite{
        [ReadOnly] [SerializeField]
        public string spriteId;
        
        public Sprite sprite;
        public MethodStorage methodStorage;

        public PixelSprite(Sprite sprite, string guid){
            methodStorage = new MethodStorage();
            this.sprite = sprite;
            spriteId = guid;
        }
    }

    
}
