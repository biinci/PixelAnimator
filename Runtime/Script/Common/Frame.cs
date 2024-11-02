using System;
using UnityEngine;
using binc.PixelAnimator.DataProvider;


namespace binc.PixelAnimator.Common{
    
    public enum FrameType{ KeyFrame, CopyFrame, EmptyFrame, None }
    
    [Serializable]
    public class Frame{
        [ReadOnly, SerializeField]
        private string guid;
        
        [SerializeField] private FrameType type;
        
        public Rect hitBoxRect;
        public PropertyData hitBoxData;
        
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

        [SerializeField] private PropertyData spriteData;
        public PropertyData SpriteData => spriteData;
        
        public PixelSprite(Sprite sprite, string gUid){
            this.sprite = sprite;
            spriteData = new PropertyData();
            spriteId = gUid;
        }
    }

    
}
