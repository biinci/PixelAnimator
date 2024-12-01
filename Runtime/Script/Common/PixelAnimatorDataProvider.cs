using System;
using System.Collections.Generic;
using binc.PixelAnimator.Common;
using UnityEngine;

namespace binc.PixelAnimator{

    public enum ColliderTypes{NoTrigger = 1, Trigger = 2}

    [Serializable]
    public class Group{
        [SerializeField, ReadOnly] private string boxDataGuid;
        public string BoxDataGuid => boxDataGuid;        
        public ColliderTypes colliderTypes = ColliderTypes.Trigger;
        public List<Layer> layers;

        public Group(string boxDataGuid){
            layers = new List<Layer>();
            this.boxDataGuid = boxDataGuid;
        }
        
        
        public void AddLayer(List<PixelSprite> pixelSprites){
            layers.Add(new Layer());
            var index = layers.Count -1;
            
            


            foreach (var pixelSprite in pixelSprites) {
                layers[index].frames.Add(new Frame(pixelSprite.spriteId));
            }
        }

#if UNITY_EDITOR
        public bool isVisible, isExpanded;
#endif
    }
    
    
    [Serializable]
    public class Layer{
        public List<Frame> frames;
        
        public Layer(){
            frames = new List<Frame>();
        }

        public void link(){
            for(var i = 1 ; i < frames.Count; i ++){
                var frame = frames[i];
                var isCopyFrame = frame.GetFrameType() == FrameType.CopyFrame;
                if(!isCopyFrame) continue;
                // frame.
            }
        }


    }
    
    
}






