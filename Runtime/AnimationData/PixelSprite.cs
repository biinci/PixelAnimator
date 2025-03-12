using System;
using UnityEngine;
using binc.PixelAnimator.DataManipulations;
namespace binc.PixelAnimator.AnimationData
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