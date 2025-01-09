using System.Collections.Generic;
using System.Linq;
using binc.PixelAnimator.Common;
using UnityEngine;


namespace binc.PixelAnimator.Preferences{
    
    [CreateAssetMenu(menuName = "Pixel Animation/ New Animation Preferences")]
    public class PixelAnimationPreferences : ScriptableObject{
        public List<BoxData> BoxData => boxData;
        [SerializeField] private List<BoxData> boxData;
        
        public BoxData GetBoxData(string guid){
            return boxData.First(x => x.Guid == guid);
        }
    }
}