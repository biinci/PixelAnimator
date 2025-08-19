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
        
        private MethodStorage<Collider2D> enterMethodStorage;
        private MethodStorage<Collider2D> stayMethodStorage;
        private MethodStorage<Collider2D> exitMethodStorage;

        private bool isValidFrame = true;
        
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
            frame = boxLayer.GetFrame(index);
            if (frame == null)
            {
                isValidFrame = false;
                return;
            }
            isValidFrame = true;
            
            enterMethodStorage = (MethodStorage<Collider2D>)frame.enterMethodStorage;
            stayMethodStorage = (MethodStorage<Collider2D>)frame.stayMethodStorage;
            exitMethodStorage = (MethodStorage<Collider2D>)frame.exitMethodStorage;
        }

        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isValidFrame) return;
            enterMethodStorage.methods.Invoke(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!isValidFrame) return;
            stayMethodStorage.methods.Invoke(other);

        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!isValidFrame) return;
            exitMethodStorage.methods.Invoke(other);
        }
        
    }
}