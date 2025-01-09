using System.Collections.Generic;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.Common;
using UnityEngine;
using System;
using System.Linq;
using System.Linq.Expressions;


namespace binc.PixelAnimator{

    public delegate void CollisionEvent(Collider2D col);
    public class PixelAnimator : MonoBehaviour{
        
        public PixelAnimationPreferences preferences;
        public PixelAnimationPreferences Preferences => preferences;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [SerializeField, Tooltip("Automatically determines the animation direction according to the flipX in the SpriteRenderer component.")] 
        private bool autoFlip;
        
        [SerializeField]private PixelAnimation currAnim; 
        private PixelAnimation nextAnim;
        public PixelAnimation PlayingAnim => currAnim;
        private int frameIndex;
        private float clock;
        public bool isPlaying;

        private GameObject titleObject;


        public readonly Dictionary<string, GameObject> groupObjects = new();
        
        public int Direction => spriteRenderer.flipX && autoFlip ? -1 : 1;

        private void Awake(){
            
            // preferences = Resources.Load<PixelAnimationPreferences>("../../Editor/Resources/PixelAnimationPreferences");
            CreateTitle();
            // currAnim.PixelSprites[0].methodStorage.methodData[0].instance
        }

        private void CreateTitle()
        {
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
            clock += Time.deltaTime;
            var secondPerFrame = 1 / (float) currAnim.fps;
            var sprites = currAnim.PixelSprites.Select(p => p.sprite).ToList();
            while (clock >= secondPerFrame) {
                // if(frameIndex > 0)LateUpdateFrame();


                frameIndex = (frameIndex + 1) % sprites.Count;
                spriteRenderer.sprite = sprites[frameIndex];
                UpdateFrame();

                clock -= secondPerFrame;
                if (frameIndex != sprites.Count - 1 || currAnim.loop) continue;
                isPlaying = false;
                break;

            }
        }
        
        
        // This function is called when the current frame time has completely elapsed.
        private void UpdateFrame(){
            ApplyBox();
        }
        
        private void LateUpdateFrame(){
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
                    var rect = GetAdjustedRect(layer, frameIndex);
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

        public static Action GetFunction(MethodData data, System.Object obj)
        {
            var instance = data.instance;
            var info = data.method.methodInfo;

            var parameters = data.parameters.Select(p => p.InheritData).ToArray();

            var lambdaParam = Expression.Parameter(typeof(object[]), "parameters");

            var methodParams = info.GetParameters();
            var convertedParams = new Expression[methodParams.Length];
            for (var i = 0; i < methodParams.Length; i++)
            {
                var paramAccess = Expression.ArrayIndex(lambdaParam, Expression.Constant(i));
                convertedParams[i] = Expression.Convert(paramAccess, methodParams[i].ParameterType);
            }

            var methodCall = Expression.Call(
                Expression.Constant(instance),  
                info,                    
                convertedParams                
            );

            var lambda = Expression.Lambda<Action<object[]>>(methodCall, lambdaParam);

            var compiledDelegate = lambda.Compile();
            return () => { compiledDelegate(parameters);};
        }

        private void RefreshGroupObject(){
            var names = currAnim.GetGroupsName(preferences);
            for (var g = 0; g < groupObjects.Count; g++) { // if group is not exist in changed animation, delete object
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
        

        public void Play(PixelAnimation animation){
            if (currAnim == animation) return;
            
            frameIndex = -1;
            clock = 0;
            clock += (float)1/animation.fps;
            isPlaying = true;
            currAnim = animation;
            RefreshGroupObject();
            SetLayer(currAnim.Groups);
            
            
        }
        
        private void ApplyBox(){
            SetBoxSize();

        }
        

        private void SetBoxSize(){
            try
            {
                var groups = currAnim.Groups;
                for (var g = 0; g < groups.Count; g++)
                {
                    var group = groups[g];
                    var groupObj = titleObject.transform.GetChild(g);
                    var cols = groupObj.GetComponents<BoxCollider2D>();
                    if (cols.Length != group.layers.Count) return;
                    for (var l = 0; l < group.layers.Count; l++)
                    {
                        var layer = group.layers[l];
                        var box = GetAdjustedRect(layer, frameIndex);
                        cols[l].offset = new Vector2(box.x * Direction, box.y);
                        cols[l].size = new Vector2(box.width, box.height);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            
        }


        
        private Rect GetAdjustedRect(Layer layer, int index){
            var f = index == -1 ? 0 : index;
            return MapBoxRectToTransform(layer.frames[f].hitBoxRect, currAnim.GetSpriteList()[f]);
        }
        
        private static Rect MapBoxRectToTransform(Rect rect, Sprite sprite) {
            var offset = new Vector2(rect.x + rect.width * 0.5f - sprite.pivot.x, sprite.rect.height - rect.y - rect.height * 0.5f - sprite.pivot.y)/sprite.pixelsPerUnit;
            var size = new Vector2(rect.width, rect.height) / sprite.pixelsPerUnit;
            return new Rect(offset, size);
        }
        

        
    }
}
