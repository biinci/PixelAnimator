using System;
using System.Collections.Generic;
using binc.PixelAnimator.Common;
using UnityEngine;

namespace binc.PixelAnimator{

    public enum ColliderTypes{NoTrigger = 1, Trigger = 2}

    [Serializable]
    public class Group{
        public string BoxDataGuid => boxDataGuid;
        [SerializeField, ReadOnly] private string boxDataGuid;
        
        public ColliderTypes colliderTypes = ColliderTypes.Trigger;
        public List<Box> layers;

        public Group(string boxDataGuid){
            layers = new List<Box>();
            this.boxDataGuid = boxDataGuid;
        }
        public void AddBox(List<PixelSprite> pixelSprites){
            layers.Add(new Box());
            var index = layers.Count -1;
            foreach (var pixelSprite in pixelSprites) {
                layers[index].frames.Add(new Frame(pixelSprite.spriteId){hitBoxRect = new Rect(0,0,16,16)});
            }
        }

#if UNITY_EDITOR
        public bool isVisible = true, isExpanded = true;
#endif
    }
    [Serializable]
    public class Box{
        public List<Frame> frames;
        
        public Box(){
            frames = new List<Frame>();
        }
    }
}






