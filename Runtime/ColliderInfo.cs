using System;
using UnityEngine;
using binc.PixelAnimator.AnimationData;
using binc.PixelAnimator.DataManipulations;

namespace binc.PixelAnimator
{
    
    public class ColliderInfo : MonoBehaviour
    {
        
        public BoxLayer boxLayer;
        public PixelAnimator animator;
        private BoxFrame frame;

        public static void Create(GameObject obj, PixelAnimator animator, BoxLayer layer)
        {
            var colliderInfo = obj.AddComponent<ColliderInfo>();
            colliderInfo.animator = animator;
            colliderInfo.boxLayer = layer;
            animator.OnFrameChanged += colliderInfo.UpdateFrame;
        } 
        private void OnDestroy()
        {
            animator.OnFrameChanged -= UpdateFrame;
        }

        private void UpdateFrame(int index)
        {
            frame = boxLayer.frames[index];
        }

        
        private void OnTriggerEnter2D(Collider2D other)
        {
            ((MethodStorage<Collider2D>)frame.enterMethodStorage).methods.Invoke(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            ((MethodStorage<Collider2D>)frame.stayMethodStorage).methods.Invoke(other);

        }

        private void OnTriggerExit2D(Collider2D other)
        {
            ((MethodStorage<Collider2D>)frame.exitMethodStorage).methods.Invoke(other);
        }
        
    }
}