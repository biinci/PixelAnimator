using System;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Common;


namespace binc.PixelAnimator.Editor.Windows{

    public partial class TimelineWindow{

        private void SetShortcuts(){
            var eventCurrent = Event.current;
            if(eventCurrent.type != EventType.KeyDown) return;
            var keyCode = eventCurrent.keyCode;
            switch (keyCode){
                case KeyCode.Return:
                    playPauseButton.DownClick();
                    break;
                case KeyCode.LeftArrow:
                // previousSpriteClick = true;
                break;
                case KeyCode.RightArrow:
                // nextSpriteClick = true;
                break;
            }
        }

        private void FrameButton(ValueTuple<int, int, int> data){
            var groupIndex = data.Item1;
            var layerIndex = data.Item2;
            var frameIndex = data.Item3;
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
        private void LayerButton(ValueTuple<Group, Layer> data){
            layerMenu = new GenericMenu();
            
            layerMenu.AddItem(new GUIContent("Delete Layer"), false, ()=>{data.Item1.layers.Remove(data.Item2);});
            layerMenu.ShowAsContext();
        }

        private void GroupButton(Group group){
            groupMenu = new GenericMenu();
            groupMenu.AddItem(new GUIContent("Delete Group"), false, ()=>{SelectedAnim.RemoveGroup(group.BoxDataGuid);});
            groupMenu.AddItem(new GUIContent("Add Layer"), false, ()=>{group.AddLayer(SelectedAnim.PixelSprites);});
            groupMenu.AddItem(new GUIContent("Expand"), group.isExpanded, () => {group.isExpanded = !group.isExpanded;});
            groupMenu.AddItem(new GUIContent("Visible"), group.isVisible, () => {group.isVisible = !group.isVisible;});
            groupMenu.ShowAsContext();

        }

        private void BurgerMenuButton(){
            burgerMenu = new GenericMenu();
            AddBurgerMenuItems();
            burgerMenu.ShowAsContext();
        }

        private void AddBurgerMenuItems(){
            var boxData = PixelAnimatorWindow.AnimatorWindow.AnimationPreferences.BoxData;
            for(var i = 0 ; i < boxData.Count; i ++){
                burgerMenu.AddItem(new GUIContent($"Add Group/{boxData[i].boxType}"), false, 
                obj=>{SelectedAnim.AddGroup(boxData[(int)obj].Guid);},i);
 
            }   
        }

        private void ThumbnailButton(int index){
            PixelAnimatorWindow.AnimatorWindow.SelectFrame(index);
        }

        private void PlayPauseButton(){
            
            isPlaying = !isPlaying ;
            playPauseTex = isPlaying ? pauseTex : playTex;
        }

        private void ChangeSpriteButton(bool isPrevious){
            var factor = isPrevious ? -1 : 1;
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var mod = SelectedAnim.GetSpriteList().Count;
            var index = (animatorWindow.IndexOfSelectedFrame + factor) % mod;
            index = index == -1 ? mod-1 : index;
            animatorWindow.SelectFrame(index); 
        }
    }
}