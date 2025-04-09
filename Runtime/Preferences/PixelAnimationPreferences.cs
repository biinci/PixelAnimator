using System.Collections.Generic;
using System.Linq;
using binc.PixelAnimator.AnimationData;
using UnityEngine;

namespace binc.PixelAnimator.Preferences{
    
    public class PixelAnimationPreferences : ScriptableObject{
        public List<BoxData> BoxData => boxData;
        [SerializeField] private List<BoxData> boxData;
        
        public BoxData GetBoxData(string guid){
            try
            {
                return boxData.First(x => x.Guid == guid);
            }
            catch
            {
                Debug.LogError($"BoxData with GUID {guid} not found.");
                return new BoxData();
            }
        }
    }
}