using System;
using UnityEngine;
using binc.PixelAnimator.DataProvider;


namespace binc.PixelAnimator.Common{
    
    public enum FrameType{ KeyFrame, CopyFrame, EmptyFrame }
    
    [Serializable]
    public class Frame{
        
        [ReadOnly, SerializeField]
        private string guid;
        
        public FrameType frameType;
        public Rect hitBoxRect = new(16, 16, 16, 16);
        [SerializeField] public PropertyData hitBoxData;
        public PropertyData HitBoxData => hitBoxData;
        
        public Frame(string guid){
            this.guid = guid;
            hitBoxData = new PropertyData();

        }

        public void SetHitBoxData(PropertyData hitBoxData){
            this.hitBoxData = hitBoxData;
        }

    }

    [Serializable]
    public class PixelSprite{
        [ReadOnly] [SerializeField]
        public string spriteId;
        
        public Sprite sprite;

        [SerializeField] private PropertyData spriteData;
        public PropertyData SpriteData => spriteData;
        
        public PixelSprite(Sprite sprite, string gUid){
            this.sprite = sprite;
            spriteData = new PropertyData();
            spriteId = gUid;
        }
    }

    
}
