using System;
using System.Collections.Generic;
using binc.PixelAnimator.AnimationData;
using UnityEngine;
using UnityEngine.Serialization;

namespace binc.PixelAnimator{

    public enum CollisionTypes{Collider = 1, Trigger = 2}

    [Serializable]
    public class BoxGroup{
        public string BoxDataGuid => boxDataGuid;
        [SerializeField, ReadOnly] private string boxDataGuid;
        
        [FormerlySerializedAs("colliderTypes")] public CollisionTypes collisionTypes = CollisionTypes.Trigger;
        public List<Box> boxes;
#if UNITY_EDITOR
        public bool isVisible = true, isExpanded = true;
#endif
        public BoxGroup(string boxDataGuid){
            boxes = new List<Box>();
            this.boxDataGuid = boxDataGuid;
        }
        
        public void AddBox(List<PixelSprite> pixelSprites){
            boxes.Add(new Box());
            var index = boxes.Count -1;
            foreach (var pixelSprite in pixelSprites) {
                boxes[index].frames.Add(new BoxFrame(pixelSprite.spriteId){boxRect = new Rect(0,0,16,16)});
            }
        }
    }
    [Serializable]
    public class Box{
        public List<BoxFrame> frames;
        
        public Box(){
            frames = new List<BoxFrame>();
        }
    }
}






