using System;
using UnityEngine;

namespace binc.PixelAnimator.Common
{
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