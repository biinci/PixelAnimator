using System;
using UnityEngine;
using binc.PixelAnimator.DataProvider;
using System.Runtime.InteropServices.WindowsRuntime;


namespace binc.PixelAnimator.Common{
    
    public enum FrameType{ KeyFrame, CopyFrame, EmptyFrame, None }
    
    [Serializable]
    public class Frame{
        [ReadOnly, SerializeField]
        private string guid;
        
        [SerializeField] private FrameType type;
        
        public FrameData FrameData{get; private set;}
        
        public Frame(string guid){
            this.guid = guid;
            FrameData.SetHitBoxRect(new Rect(0,0,16,16));

        }

        public void SetType(FrameType type){
            this.type = type;
        }

        public void SetFrameData(FrameData frameData){
            FrameData = frameData;
        }
        public FrameType GetFrameType() => type;
        

    }

    public struct FrameData{
        public Rect hitBoxRect;
        [SerializeField] private PropertyData spriteData;
        public PropertyData SpriteData => spriteData;

        public void SetHitBoxData(PropertyData data){
            spriteData = data;
        }  
        public void SetHitBoxRect(Rect rect){
            hitBoxRect = rect;
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
