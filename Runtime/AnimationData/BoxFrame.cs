using System;
using UnityEngine;
using binc.PixelAnimator.DataManipulations;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace binc.PixelAnimator.AnimationData{
    
    public enum BoxFrameType{ KeyFrame, CopyFrame, EmptyFrame, None }
    
    [Serializable]
    public class BoxFrame{
        [ReadOnly, SerializeField]
        private string guid;
        
        public BoxFrameType Type => type;
        [SerializeField] private BoxFrameType type;
        
        public Rect boxRect;

        [SerializeReference] public BaseMethodStorage enterMethodStorage;
        [SerializeReference] public BaseMethodStorage stayMethodStorage;
        [SerializeReference] public BaseMethodStorage exitMethodStorage;

        public void ChangeMethodType(CollisionTypes collisionTypes)
        {
            var collideType = collisionTypes == CollisionTypes.Collider ? typeof(Collision2D) : typeof(Collider2D);
            var genericType = typeof(MethodStorage<>).MakeGenericType(collideType);
            enterMethodStorage = (BaseMethodStorage) Activator.CreateInstance(genericType);
            stayMethodStorage = (BaseMethodStorage) Activator.CreateInstance(genericType);
            exitMethodStorage = (BaseMethodStorage) Activator.CreateInstance(genericType);
        }
        

        public BoxFrame(string guid){
            this.guid = guid;
        }
        
        public BoxFrame(string guid, CollisionTypes collisionType){
            this.guid = guid;
            ChangeMethodType(collisionType);
        }

        public void SetType(BoxFrameType boxFrameType)
        {
            type = boxFrameType;
        }
    }
    
    
}
