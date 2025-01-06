using System.Collections.Generic;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.Common;
using UnityEngine;
using System;
using System.Linq;



namespace binc.PixelAnimator{

    public delegate void CollisionEvent(Collider2D col);
    public class PixelAnimator : MonoBehaviour{
        
        public PixelAnimationPreferences preferences;
        public PixelAnimationPreferences Preferences => preferences;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [SerializeField, Tooltip("Automatically determines the animation direction according to the flipX in the SpriteRenderer component.")] 
        private bool autoFlip;
        
        private PixelAnimation currAnim; 
        private PixelAnimation nextAnim;
        public PixelAnimation PlayingAnim => currAnim;
        private int FPS => currAnim  ? currAnim.fps : 0;
        private int frameIndex;
        private float clock;
        public bool isPlaying;

        private GameObject titleObject;


        public Dictionary<string, Action<object>> SpriteProperty{ get; private set; }
        public Dictionary<string, PixelAnimatorListener> SpriteEvents{ get; private set; }

        // public Dictionary<Layer, ColliderInfo> layerToColliderInfo;
        // public Dictionary<BoxCollider2D, ColliderInfo> colliderToInfo;
        public readonly Dictionary<string, GameObject> groupObjects = new();


        public CollisionEvent collisionEvent;
        public Dictionary<string, Action<object>> ColliderEvents{get; private set;}


        public int Direction => spriteRenderer.flipX && autoFlip ? -1 : 1;

        private List<Sprite> currSprites;
        private void Awake(){
            
            // preferences = Resources.Load<PixelAnimationPreferences>("../../Editor/Resources/PixelAnimationPreferences");
            titleObject = new GameObject("---PixelAnimator Colliders---"){ transform ={
                    parent = transform,
                    localPosition = Vector3.zero
                }
            };

            ColliderEvents = new Dictionary<string, Action<object>>();
            SpriteProperty = new Dictionary<string, Action<object>>();
        }

        private void Update(){
            NextFrame();

 

        }


        private void NextFrame(){
            if (!isPlaying) return;
            clock += Time.deltaTime;
            var secondsPerFrame = 1 / (float) FPS;
            while (clock >= secondsPerFrame) {
                if(frameIndex > 0)UpdateLateFrame();

                if (frameIndex == currSprites.Count && !currAnim.loop) {
                    isPlaying = false;
                    return;
                }
                clock -= secondsPerFrame;
                frameIndex = (frameIndex + 1) % currSprites.Count;
                spriteRenderer.sprite = currSprites[frameIndex];
                UpdateFrame();

                
            }
        }
        
        
        // This function is called when the current frame time has completely elapsed.
        private void UpdateFrame(){
            ApplyBox();
        }
        
        private void UpdateLateFrame(){
            ApplySpritePropValue();
        }
        

        private void ApplySpritePropValue(){
            if (!currAnim) return;
            var pixelSprite = currAnim.PixelSprites[frameIndex];
            

            // foreach (var eventName in pixelSprite.SpriteData.eventNames) {
            // foreach (var eventName in pixelSprite.eventNames) {
            //     if (SpriteEvents.ContainsKey(eventName)) {
            //         SpriteEvents[eventName].Invoke();
            //     }
            //     else {
            //         Debug.LogWarning($"Method name '{eventName}' does not exist");
            //     }
            // }
        }
        
        

        
        //When the current animation changes, the collider info component and the box collider component are added. The size and position of the box collider are adjusted.
        private void SetLayer(List<Group> groups){
            foreach (var group in groups) {
                var groupObj = GetAddedGroupObject(group);
                // var colInfo = groupObj.AddComponent<ColliderInfo>();
                foreach (var layer in group.layers) {
                    var col = groupObj.AddComponent<BoxCollider2D>();
                    // colliderToInfo.Add(col, colInfo);
                    col.enabled = false;
                    var rect = GetAdjustedRect(layer);
                    col.isTrigger = group.colliderTypes == ColliderTypes.Trigger;
                    col.size = rect.size;
                    col.offset = new Vector2(rect.position.x * Direction, rect.position.y);
                    col.enabled = true;
                    // colInfo.Setup(this, col);
                    // layerToColliderInfo.Add(layer, colInfo);
                    
                    
                }
            }

        }

        //When the current animation changes, the objects of the groups are checked. The group object is refreshed depending on the situation.
        private GameObject GetAddedGroupObject(Group group){
            
            
            var boxData = preferences.GetBoxData(group.BoxDataGuid);
            var isExist = groupObjects.Keys.Contains(boxData.boxType);
            GameObject groupObj;
            
            if (!isExist) {
                groupObj = CreateGroupObject(boxData);
                groupObjects.Add(groupObj.name, groupObj);
            }
            else {
                groupObj = groupObjects[boxData.boxType].gameObject;
            }

            foreach (var component in groupObj.GetComponents<Component>()) {
                if(component is not Transform) Destroy(component);
            }

            return groupObj;
        }


        private void RefreshGroupObject(){
            var names = currAnim.GetGroupsName(preferences);
            for (var g = 0; g < groupObjects.Count; g++) { // if not exist group in changed animation, delete object
                if (names.Contains(groupObjects.ElementAt(g).Key)) continue;

                var gameObj = titleObject.transform.GetChild(g).gameObject;
                groupObjects.Remove(gameObj.name);
                Destroy(gameObj);
            }
        }

        
        private GameObject CreateGroupObject(BoxData boxData){
            var gameObj = new GameObject(boxData.boxType){
                transform ={
                    parent = titleObject.transform,
                    localPosition = Vector3.zero,
                    localScale = transform.localScale
                },
                layer =  boxData.activeLayer
            };
            return gameObj;
        }
        

        public void Play(PixelAnimation willChange){
            if (currAnim == willChange) return;
            
            SpriteEvents = new Dictionary<string, PixelAnimatorListener>();
            SpriteProperty = new Dictionary<string, Action<object>>();
            // layerToColliderInfo = new Dictionary<Layer, ColliderInfo>();
            // colliderToInfo = new Dictionary<BoxCollider2D, ColliderInfo>();
            frameIndex = -1;
            clock = 0;
            clock += (float)1/willChange.fps;
            isPlaying = true;
            currAnim = willChange;
            RefreshGroupObject();
            SetLayer(currAnim.Groups);
            currSprites = currAnim.GetSpriteList();
            
            
        }
        
        private void ApplyBox(){
            SetBoxSize();
            // SetHitBoxProps();

        }

        private void SetHitBoxProps(){
            foreach (var group in currAnim.Groups) {
                foreach (var layer in group.layers) {
                    var frameIndex = this.frameIndex;
                    if (layer.frames[this.frameIndex].GetFrameType() != FrameType.KeyFrame) {
                        for (var i = this.frameIndex; i >= 0; i--) {
                            var frame = layer.frames[i];
                            if (frame.GetFrameType() != FrameType.KeyFrame) continue;
                            frameIndex = i;
                            break;
                        }
                    }
                    // layerToColliderInfo[layer].ChangeFrame(layer.frames[frameIndex]);
                }
            }
        }        

        private void SetBoxSize(){
            if (!currAnim || currAnim.Groups == null) return;
            var groups = currAnim.Groups;
            for (var g = 0; g < groups.Count; g++) {
                var group = groups[g];
                var groupObj = titleObject.transform.GetChild(g);
                var cols = groupObj.GetComponents<BoxCollider2D>();
                if (cols.Length != group.layers.Count) return;
                for (var l = 0; l < group.layers.Count; l++) {
                    var layer = group.layers[l];
                    var box = GetAdjustedRect(layer);
                    cols[l].offset = new Vector2(box.x * Direction, box.y);
                    cols[l].size = new Vector2(box.width, box.height);
                }
            }
            
        }


        
        private Rect GetAdjustedRect(Layer layer){
            var f = frameIndex == -1 ? 0 : frameIndex;
            return MapBoxRectToTransform(layer.frames[f].hitBoxRect, currAnim.GetSpriteList()[f]);
        }
        
        private static Rect MapBoxRectToTransform(Rect rect, Sprite sprite) {
            var offset = new Vector2((rect.x + rect.width * 0.5f - sprite.pivot.x) / sprite.pixelsPerUnit, (sprite.rect.height - rect.y - rect.height * 0.5f - sprite.pivot.y) / sprite.pixelsPerUnit);
            var size = new Vector2(rect.width / sprite.pixelsPerUnit, rect.height / sprite.pixelsPerUnit);
            return new Rect(offset, size);
        }
        

        
    }
}
