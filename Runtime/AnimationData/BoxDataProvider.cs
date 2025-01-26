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

        public void SetFrameType(int index){
            if(index < 0 || index >= frames.Count) return;
            var currentFrame = frames[index];
            switch (currentFrame.Type)
            {
                case BoxFrameType.KeyFrame:
                    if (index == 0)
                    {
                        currentFrame.SetType(BoxFrameType.EmptyFrame);
                    }
                    else if(frames[index-1].Type != BoxFrameType.EmptyFrame)
                    {
                        currentFrame.SetType(BoxFrameType.CopyFrame);
                    }
                    else
                    {
                        currentFrame.SetType(BoxFrameType.EmptyFrame);
                        if (index+1 < frames.Count && frames[index + 1].Type == BoxFrameType.CopyFrame)
                        {
                            frames[index+1].SetType(BoxFrameType.KeyFrame);
                        }
                    }
                    break;
                case BoxFrameType.CopyFrame:
                    currentFrame.SetType(BoxFrameType.EmptyFrame);
                    if(index+2 < frames.Count && frames[index+1].Type == BoxFrameType.CopyFrame)
                    {
                        frames[index+1].SetType(BoxFrameType.KeyFrame);
                    }
                    break;
                case BoxFrameType.EmptyFrame:
                    currentFrame.SetType(BoxFrameType.KeyFrame);
                    break;
                case BoxFrameType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
            
        
    }
}






