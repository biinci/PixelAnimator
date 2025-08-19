using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using binc.PixelAnimator.AnimationData;
using binc.PixelAnimator.Preferences;

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
        
        
        // public void AddPixelSprite(Sprite sprite){
        //     pixelSprites.Add(new PixelSprite(sprite, GUID.Generate().ToString()));
        // }

        public List<string> GetBoxGroupsName(PixelAnimationPreferences preferences)
        {
            return boxGroups.Select(boxGroup => preferences.GetBoxData(boxGroup.BoxDataGuid).boxName).ToList();
        }
    }
}