using System;
using UnityEngine;
<<<<<<< Updated upstream
using binc.PixelAnimator.DataProvider;

=======
using UnityEngine.Events;
>>>>>>> Stashed changes

namespace binc.PixelAnimator.Common{
    
    public enum FrameType{ KeyFrame, CopyFrame, EmptyFrame, None }
    
    [Serializable]
    public class Frame{
        [ReadOnly, SerializeField]
        private string guid;
        
        [SerializeField] private FrameType type;
        
        public Rect hitBoxRect;
<<<<<<< Updated upstream
        public PropertyData hitBoxData;
        
=======
        public MethodStorage methodStorage;
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream

        [SerializeField] private PropertyData spriteData;
        public PropertyData SpriteData => spriteData;
        
        public PixelSprite(Sprite sprite, string gUid){
            this.sprite = sprite;
            spriteData = new PropertyData();
            spriteId = gUid;
=======
        public MethodStorage methodStorage;

        public PixelSprite(Sprite sprite, string guid){
            methodStorage = new MethodStorage();
            this.sprite = sprite;
            spriteId = guid;
>>>>>>> Stashed changes
        }
    }

    
}
