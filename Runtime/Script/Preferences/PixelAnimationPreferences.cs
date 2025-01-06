using System.Collections.Generic;
using System.Linq;
using binc.PixelAnimator.Common;
using UnityEngine;


namespace binc.PixelAnimator.Preferences{
    
    [CreateAssetMenu(menuName = "Pixel Animation/ New Animation Preferences")]
    public class PixelAnimationPreferences : ScriptableObject{
        [SerializeField] private List<BoxData> boxData;
        
        
        public List<BoxData> BoxData => boxData;
        
        
        public BoxData GetBoxData(string guid){
            return boxData.First(x => x.Guid == guid);
        }
        
        
    }
    
}