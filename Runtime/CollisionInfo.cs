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
            frame = boxLayer.frames[index];
        }
        

        private void OnCollisionEnter2D(Collision2D other)
        {
            ((MethodStorage<Collision2D>)frame.enterMethodStorage).methods.Invoke(other);
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            Debug.Log(boxLayer + "    " +frame);
            ((MethodStorage<Collision2D>)frame.stayMethodStorage).methods.Invoke(other);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            ((MethodStorage<Collision2D>)frame.exitMethodStorage).methods.Invoke(other);

        }
    }
}