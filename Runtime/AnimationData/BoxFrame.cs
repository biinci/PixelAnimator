using System;
using UnityEngine;
using binc.PixelAnimator.DataManipulations;
using UnityEditor;

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

        public void ChangeMethodType<T>()
        {
            var genericType = typeof(MethodStorage<>).MakeGenericType(typeof(T));
            enterMethodStorage = (BaseMethodStorage) Activator.CreateInstance(genericType);
            stayMethodStorage = (BaseMethodStorage) Activator.CreateInstance(genericType);
            exitMethodStorage = (BaseMethodStorage) Activator.CreateInstance(genericType);
        }
        

        public BoxFrame(string guid){
            this.guid = guid;
        }

        public void SetType(BoxFrameType boxFrameType)
        {
            type = boxFrameType;
        }
    }
    
    
}
