using binc.PixelAnimator.Collider;
using binc.PixelAnimator.Common;
using binc.PixelAnimator.Preferences;
using UnityEngine;

namespace binc.PixelAnimator{
    public class ColliderInfo : MonoBehaviour, IPixelCollide{
        [SerializeField] private Frame frame;
        public BoxCollider2D col2D;
        
        private PixelAnimator anim;
        
        
        //I have a hitBox properties.
        //I have trigger; enter, stay, exit functions. // Maybe I make 3 different script.

        public void Setup(PixelAnimator anim, BoxCollider2D col2D){
            this.anim = anim;
            this.col2D = col2D;
        }


        
        private void Update(){
            
            if(col2D != null) //TODO fix the collider nullable
                col2D.enabled = frame.GetFrameType() != FrameType.EmptyFrame;
        }

        private void OnTriggerEnter2D(Collider2D col){
            var preferences = anim.Preferences;
            
            if (!col.transform.root.TryGetComponent<PixelAnimator>(out var otherAnim)) return;
            var otherColInfo = otherAnim.colliderToInfo[(BoxCollider2D)col];
            gameObject.SendMessageUpwards("OnBoxCollision", otherColInfo);
            otherAnim.gameObject.SendMessageUpwards("OnBoxCollision", this);
            
            //foreach (var hitBoxValue in frame.HitBoxData.genericData) {
            //    var prop = preferences.GetProperty(PropertyType.HitBox, hitBoxValue.baseData.Guid); // Get HitBox Property
            //    otherAnim.ColliderEvents[prop.Name]?.Invoke(hitBoxValue.GetInheritData());
            //}

            // otherAnim.collisionEvent?.Invoke(this, col);
            // anim.collisionEvent?.Invoke(otherColInfo, col);
            // anim.colliderToInfo[].


        }
        
        //TODO: According name collider info.
        public void ApplyHitBoxProperties(){
            // var f = anim.Frame; // active frame index.
            // var preferences = anim.Preferences;
            // if (layer.frames[f].HitBoxData == null || layer.frames[f].HitBoxData.genericData == null) return;
            //
            // foreach (var propertyValue in layer.frames[f].HitBoxData.genericData) { 
            //     var propName = preferences.GetProperty(PropertyType.HitBox, propertyValue.baseData.Guid).Name; // Getting hit box property name.
            //     if (anim.hitBoxProperty.ContainsKey(propName)) {
            //         anim.hitBoxProperty[propName]?.Invoke(propertyValue.baseData.InheritData); 
            //     }
            // }
        }
        
        public void ChangeFrame(Frame frame){
            this.frame = frame;

        }
        
        
    }
}
