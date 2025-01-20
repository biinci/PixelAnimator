using System;
using System.Collections.Generic;
using UnityEngine;
using binc.PixelAnimator.Common;
using binc.PixelAnimator.Preferences;
using UnityEditor;
using UnityEngine.Serialization;

namespace binc.PixelAnimator{

    [CreateAssetMenu(menuName = "Pixel Animation/ New Animation")]
    public class PixelAnimation : ScriptableObject
    {
        public bool loop = true;
        public int fps = 12;
        public List<PixelSprite> PixelSprites => pixelSprites;
        [SerializeField] private List<PixelSprite> pixelSprites;
        public List<BoxGroup> BoxGroups => boxGroups;
        [SerializeField] private List<BoxGroup> boxGroups;

        private void OnEnable()
        {
            if (pixelSprites != null)
            {
                foreach (var pixelSprite in pixelSprites)
                {
                    if(pixelSprite.methodStorage == null) continue;
                    pixelSprite.methodStorage.OnEnable();
                }
            }   
        }

        public void AddGroup(string boxDataGuid){
            boxGroups.Add(new BoxGroup(boxDataGuid));
        }

        public void RemoveGroup(string boxDataGuid){
            var group = BoxGroups.Find(x => x.BoxDataGuid == boxDataGuid);
            if (group == null){
                Debug.LogWarning("The box group you want to delete is not already on the list.");
                return;
            }
            BoxGroups.Remove(group);
        }
        
        public List<Sprite> GetSpriteList(){
            var sprites = new List<Sprite>();
            if (pixelSprites == null) return sprites;
            
            foreach (var pixelSprite in pixelSprites) {
                sprites.Add(pixelSprite.sprite);
            }
            return sprites;
        }

        public void AddPixelSprite(Sprite sprite){
            pixelSprites.Add(new PixelSprite(sprite, GUID.Generate().ToString()));
        }

        public List<string> GetBoxGroupsName(PixelAnimationPreferences preferences){
            var names = new List<string>();
            foreach (var boxGroup in boxGroups) {
                names.Add(preferences.GetBoxData(boxGroup.BoxDataGuid).boxName);
            }
            return names;
        }
    }

    [System.Serializable]
    public struct Reference
    {
        [ReadOnly] public string assetGUID;
        [ReadOnly] public string targetObjectId;
        
    }
    

    
    
}