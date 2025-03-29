using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace binc.PixelAnimator.AnimationData{

    public enum CollisionTypes{Collider = 1, Trigger = 2}

    [Serializable]
    public class BoxGroup{
        public string BoxDataGuid => boxDataGuid;
        [SerializeField, ReadOnly] private string boxDataGuid;
        
        [FormerlySerializedAs("colliderTypes")] public CollisionTypes collisionTypes = CollisionTypes.Trigger;
        public List<BoxLayer> boxes;
#if UNITY_EDITOR
        public bool isVisible = true, isExpanded = true;
#endif
        public BoxGroup(string boxDataGuid){
            boxes = new List<BoxLayer>();
            this.boxDataGuid = boxDataGuid;
        }
        
        public void AddBox(List<PixelSprite> pixelSprites){
            boxes.Add(new BoxLayer());
            var index = boxes.Count -1;
            foreach (var pixelSprite in pixelSprites) {
                boxes[index].frames.Add(new BoxFrame(pixelSprite.spriteId, collisionTypes){boxRect = new Rect(0,0,16,16)});
            }
        }
        
        public void ChangeCollisionType(CollisionTypes collisionType){
            collisionTypes = collisionType;
            foreach (var box in boxes)
            {
                box.ChangeFrameMethodType(collisionTypes);
                
            }
        }

        
        
    }
    [Serializable]
    public class BoxLayer{
        public List<BoxFrame> frames;
        
        public BoxLayer(){
            frames = new List<BoxFrame>();
        }

        #if UNITY_EDITOR
        public void AddFrame(CollisionTypes collisionTypes){
            frames.Add(new BoxFrame(GUID.Generate().ToString(),collisionTypes));
            
        }
        #endif
        
        public BoxFrame GetFrame(int index){
            if(index < 0 || index >= frames.Count) return null;
            var frame = frames[index];
            if (frame.Type == BoxFrameType.KeyFrame) return frame;
            if (frame.Type == BoxFrameType.EmptyFrame) return null;
            index--;
            while (frames[index].Type == BoxFrameType.CopyFrame)
            {
                index--;
            }

            return frames[index];
        }
        
        
        public void ChangeFrameMethodType(CollisionTypes collisionTypes){
            foreach (var frame in frames)
            {
                frame.ChangeMethodType(collisionTypes);
            }
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






