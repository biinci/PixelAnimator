using System.Collections.Generic;
using UnityEngine;
using binc.PixelAnimator.Common;
using binc.PixelAnimator.Preferences;
using UnityEditor;

namespace binc.PixelAnimator{

    [CreateAssetMenu(menuName = "Pixel Animation/ New Animation")]
    public class PixelAnimation : ScriptableObject
    {
        public bool loop = true;
        public int fps = 12;
        [SerializeField] private List<PixelSprite> pixelSprites;
        public List<PixelSprite> PixelSprites => pixelSprites;
        
        
        [SerializeField] private List<Group> groups;
        public List<Group> Groups => groups;
        

        
        
        public void AddGroup(string boxDataGuid){
            groups.Add(new Group(boxDataGuid));
        }

        public void RemoveGroup(string boxDataGuid){
            var group = Groups.Find(x => x.BoxDataGuid == boxDataGuid);
            if (group == null){
                Debug.LogWarning("The group you want to delete is not already on the list.");
                return;
            }
            Groups.Remove(group);

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

        public List<string> GetGroupsName(PixelAnimationPreferences preferences){
            var names = new List<string>();
            foreach (var group in groups) {
                names.Add(preferences.GetBoxData(group.BoxDataGuid).boxType);
            }
            return names;

        }
        
        
    }
    
    
}



