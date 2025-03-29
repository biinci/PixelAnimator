using System;
using UnityEngine;
using binc.PixelAnimator.AnimationData;
using binc.PixelAnimator.DataManipulations;
namespace binc.PixelAnimator
{
    
    public class CollisionInfo : MonoBehaviour
    {
        public BoxLayer boxLayer;
        public PixelAnimator animator;
        private BoxFrame frame;
        
        private MethodStorage<Collision2D> enterMethodStorage;
        private MethodStorage<Collision2D> stayMethodStorage;
        private MethodStorage<Collision2D> exitMethodStorage;
        public static void Create(GameObject obj, PixelAnimator animator, BoxLayer layer)
        {
            var collisionInfo = obj.AddComponent<CollisionInfo>();
            collisionInfo.animator = animator;
            collisionInfo.boxLayer = layer;
            animator.OnFrameChanged += collisionInfo.UpdateFrame;
        }
        
        
        private void OnDestroy()
        {
            animator.OnFrameChanged -= UpdateFrame;
        }
        
        
        private void UpdateFrame(int index)
        {
            frame = boxLayer.GetFrame(index);
            enterMethodStorage = (MethodStorage<Collision2D>)frame.enterMethodStorage;
            stayMethodStorage = (MethodStorage<Collision2D>)frame.stayMethodStorage;
            exitMethodStorage = (MethodStorage<Collision2D>)frame.exitMethodStorage;
        }
        

        private void OnCollisionEnter2D(Collision2D other)
        {
            enterMethodStorage.methods.Invoke(other);
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            stayMethodStorage.methods.Invoke(other);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            exitMethodStorage.methods.Invoke(other);

        }
    }
}