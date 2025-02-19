using System.Collections.Generic;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.AnimationData;
using UnityEngine;
using System;
using System.Linq;

namespace binc.PixelAnimator{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PixelAnimator : MonoBehaviour{
        private PixelAnimationPreferences preferences;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private PixelAnimationController animationController;
        public PixelAnimation PlayingAnimation { get; private set; }
        private PixelAnimation nextAnim;
        private int frameIndex;
        private float elapsedTime;
        private bool isPlaying;

        private GameObject titleObject;
        
        private readonly Dictionary<string, GameObject> colliderObjects = new();
        
        private void Awake(){
            preferences = Resources.Load<PixelAnimationPreferences>("Animation Preferences");
            CreateTitle();
        }

        private void CreateTitle(){
            titleObject = new GameObject("---PixelAnimator Colliders---"){ transform ={
                    parent = transform,
                    localPosition = Vector3.zero
                }
            };
        }
        
        private void Update(){
            NextFrame();
        }
        
        private void NextFrame(){
            if (!isPlaying) return;
            elapsedTime += Time.deltaTime;
            var secondsPerFrame = 1 / (float)PlayingAnimation.fps;
            var spriteCount = PlayingAnimation.PixelSprites.Count;
            
            while (elapsedTime >= secondsPerFrame){
                
                frameIndex = (frameIndex + 1) % spriteCount;
                ApplyFrame();
                elapsedTime -= secondsPerFrame;
                if (frameIndex != spriteCount - 1 || PlayingAnimation.loop)
                    continue;
                isPlaying = false;
                break;
            }
        }

        
        private void ApplyFrame()
        {
            spriteRenderer.sprite = PlayingAnimation.PixelSprites[frameIndex].sprite;
            CallEvent();
            SetBoxSize();
        }

        private void CallEvent()
        {
            var unityEvent = PlayingAnimation.PixelSprites[frameIndex].methodStorage.methods;
            unityEvent?.Invoke();
        }
        
        private void SetBoxSize(){
            try{
                var groups = PlayingAnimation.BoxGroups;
                for (var g = 0; g < groups.Count; g++){
                    var group = groups[g];
                    var groupObj = titleObject.transform.GetChild(g);
                    var cols = groupObj.GetComponents<BoxCollider2D>();
                    if (cols.Length != group.boxes.Count) return;
                    for (var l = 0; l < group.boxes.Count; l++)
                    {
                        var layer = group.boxes[l];
                        var box = GetAdjustedRect(layer, frameIndex);
                        cols[l].offset = new Vector2(box.x, box.y);
                        cols[l].size = new Vector2(box.width, box.height);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        private Rect GetAdjustedRect(Box box, int index){
            var f = index == -1 ? 0 : index;
            return PixelAnimatorUtility.MapBoxRectToTransform(box.frames[f].boxRect, PlayingAnimation.GetSpriteList()[f]);
        }

        private GameObject CreateColliderObject(BoxData boxData){
            var gameObj = new GameObject(boxData.boxName){
                transform ={
                    parent = titleObject.transform,
                    localPosition = Vector3.zero,
                    localScale = transform.localScale
                },
                layer =  boxData.layer
            };
            return gameObj;
        }
        
        public void Play(PixelAnimation nextAnimation){//TODO: need to fix
            frameIndex = 0;
            elapsedTime = 0;
            isPlaying = true;
            PlayingAnimation = nextAnimation;
            RefreshColliderObjects();
            SetBoxes(PlayingAnimation.BoxGroups);
            ApplyFrame();
        }
        
        private void RefreshColliderObjects(){
            var names = PlayingAnimation.GetBoxGroupsName(preferences);
            for (var g = 0; g < colliderObjects.Count; g++){
                if (names.Contains(colliderObjects.ElementAt(g).Key)) continue;

                var gameObj = titleObject.transform.GetChild(g).gameObject;
                colliderObjects.Remove(gameObj.name);
                Destroy(gameObj);
            }
        }
        
        //When the current animation changes, the collider info component and the box collider component are added. The size and position of the box collider are adjusted.
        private void SetBoxes(List<BoxGroup> boxGroups){
            foreach (var boxGroup in boxGroups) {
                var colliderObj = SetColliderObject(boxGroup);
                foreach (var box in boxGroup.boxes)
                {
                    SetCollider(colliderObj, box, boxGroup);
                }
            }
        }
        
        //When the current animation changes, the objects of the groups are checked. The collider object is refreshed depending on the situation.
        private GameObject SetColliderObject(BoxGroup boxGroup){
            GameObject colliderObj;
            var boxData = preferences.GetBoxData(boxGroup.BoxDataGuid);
            var isExist = colliderObjects.Keys.Contains(boxData.boxName);
            if (!isExist){
                colliderObj = CreateColliderObject(boxData);
                colliderObjects.Add(colliderObj.name, colliderObj);
            }
            else{
                colliderObj = colliderObjects[boxData.boxName].gameObject;
            }

            foreach (var component in colliderObj.GetComponents<Component>()){
                if(component is not Transform) Destroy(component);
            }

            return colliderObj;
        }
        
        private void SetCollider(GameObject colliderObj, Box box, BoxGroup boxGroup){
            var col = colliderObj.AddComponent<BoxCollider2D>();
            col.enabled = false;
            var rect = GetAdjustedRect(box, frameIndex);
            col.isTrigger = boxGroup.collisionTypes == CollisionTypes.Trigger;
            col.size = rect.size;
            col.offset = new Vector2(rect.position.x, rect.position.y);
            col.enabled = true;
        }
    }
}