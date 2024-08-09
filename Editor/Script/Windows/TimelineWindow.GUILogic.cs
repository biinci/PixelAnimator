using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Common;


namespace binc.PixelAnimator.Editor.Windows{

    public partial class TimelineWindow : Window, IUpdate{
        private void LinkWindowButtons(){
            LinkThumbnailButton();
            LinkPlayPauseButton();
            LinkChangeSpriteButton();
            LinkBurgerMenuButton();
            LinkGroupsButton();
            LinkLayersButton();
            LinkFramesButton();
        }

        private void SetShortcuts(){
            var eventCurrent = Event.current;
            if(eventCurrent.type != EventType.KeyDown) return;
            var keyCode = eventCurrent.keyCode;
            switch (keyCode){
                case KeyCode.Return:
                playPauseClick = !playPauseClick;
                break;
                case KeyCode.LeftArrow:
                previousSpriteClick = true;
                break;
                case KeyCode.RightArrow:
                nextSpriteClick = true;
                break;
            }
            

        }

        private void LinkFramesButton(){
            if(!frameButton.clicked) return;
            var groupIndex = frameButton.data.Item1;
            var layerIndex = frameButton.data.Item2;
            var frameIndex = frameButton.data.Item3;
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var groups = animatorWindow.SelectedAnimation.Groups;
            var layers = groups[groupIndex].layers;
            var frames = layers[layerIndex].frames;
            var isSameFrameIndex = frameIndex == animatorWindow.IndexOfSelectedFrame;
            var isSameLayerIndex = layerIndex == animatorWindow.IndexOfSelectedLayer;
            var isSameGroupIndex = groupIndex == animatorWindow.IndexOfSelectedGroup;
            var frame = frames[frameIndex];            
            
            if(isSameLayerIndex && isSameGroupIndex){
                var previousType = FrameType.None;
                if(frameIndex > 0){
                    previousType = frames[frameIndex-1].GetFrameType();
                }
                frame.SetType(ChangeFrameType(frame.GetFrameType(), previousType, frameIndex));
            } 

            if(isSameFrameIndex){
                animatorWindow.SelectGroup(groupIndex);
                animatorWindow.SelectLayer(layerIndex);
            } 

            frameButton.clicked = false;
        }
        private FrameType ChangeFrameType(FrameType type, FrameType previousType, int index){
            switch (type){
                case FrameType.KeyFrame:
                    var isValid = index > 0 && previousType != FrameType.EmptyFrame && previousType != FrameType.None;
                    if(isValid)return FrameType.CopyFrame;
                    goto case FrameType.CopyFrame;
                case FrameType.CopyFrame:
                    return FrameType.EmptyFrame;
                case FrameType.EmptyFrame:
                    return FrameType.KeyFrame;
                default:
                    return FrameType.None;
            }
        }
        private void LinkLayersButton(){
            if(!layerButton.clicked) return;
            layerMenu = new GenericMenu();
            
            layerMenu.AddItem(new GUIContent("Delete Layer"), false, ()=>{layerButton.data.Item1.layers.Remove(layerButton.data.Item2);});
            layerMenu.ShowAsContext();
            layerButton.clicked = false;
        }

        private void LinkGroupsButton(){
            if(!groupButton.clicked) return;
            var group = groupButton.data;
            groupMenu = new GenericMenu();
            groupMenu.AddItem(new GUIContent("Delete Group"), false, ()=>{SelectedAnim.RemoveGroup(group.BoxDataGuid);});
            groupMenu.AddItem(new GUIContent("Add Layer"), false, ()=>{group.AddLayer(SelectedAnim.PixelSprites);});
            groupMenu.AddItem(new GUIContent("Expand"), group.isExpanded, () => {group.isExpanded = !group.isExpanded;});
            groupMenu.AddItem(new GUIContent("Visible"), group.isVisible, () => {group.isVisible = !group.isVisible;});
            groupMenu.ShowAsContext();
            groupButton.clicked = false;

        }

        private void LinkBurgerMenuButton(){
            if(!burgerClick) return;
            burgerMenu = new GenericMenu();

            var boxData = PixelAnimatorWindow.AnimatorWindow.AnimationPreferences.BoxData;
            for(var i = 0 ; i < boxData.Count; i ++){
                burgerMenu.AddItem(new GUIContent($"Add Group/{boxData[i].boxType}"), false, 
                obj=>{SelectedAnim.AddGroup(boxData[(int)obj].Guid);},i);
 
            }   
            burgerMenu.ShowAsContext();
            burgerClick = true;
        }


        private void LinkThumbnailButton(){
            if(!thumbnailButton.clicked) return;
            PixelAnimatorWindow.AnimatorWindow.SelectFrame(thumbnailButton.data);
            
        }

        private void LinkPlayPauseButton(){
            if(!playPauseClick) return;
            isPlaying = !isPlaying ;
            playPauseTex = isPlaying ? pauseTex : playTex;
            playPauseClick = false;
        }

        private void LinkChangeSpriteButton(){
            if(!previousSpriteClick && !nextSpriteClick) return;
            var factor = previousSpriteClick ? -1 : 1;
            var animator = PixelAnimatorWindow.AnimatorWindow;
            var mod = SelectedAnim.GetSpriteList().Count;
            var index = (animator.IndexOfSelectedFrame + factor) % mod;
            index = index == -1 ? mod-1 : index;
            animator.SelectFrame(index); 
            previousSpriteClick = false;
            nextSpriteClick = false;
        }
    }
}