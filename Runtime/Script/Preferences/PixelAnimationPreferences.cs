using System;
using System.Collections.Generic;
using System.Linq;
using binc.PixelAnimator.Common;
using binc.PixelAnimator.DataProvider;
using UnityEngine;


namespace binc.PixelAnimator.Preferences{
    
    
    public class PixelAnimationPreferences : ScriptableObject{
        [SerializeField] private List<BasicPropertyData> spriteProperties;
        [SerializeField] private List<BasicPropertyData> hitBoxProperties;
        [SerializeField] private List<BoxData> boxData;
        
        
        public List<BasicPropertyData> SpriteProperties => spriteProperties;
        public List<BasicPropertyData> HitBoxProperties => hitBoxProperties;
        public List<BoxData> BoxData => boxData;
        
        
        public BoxData GetBoxData(string guid){
            return boxData.First(x => x.Guid == guid);
        }

        public BasicPropertyData GetProperty(PropertyType propertyType, string guid){
            return propertyType switch{
                PropertyType.Sprite => spriteProperties.FirstOrDefault(x => x.Guid == guid),
                PropertyType.HitBox => hitBoxProperties.FirstOrDefault(x => x.Guid == guid),
                _ => throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null)
            };
        }
        
    }
    
    public enum PropertyType{Sprite, HitBox}
}