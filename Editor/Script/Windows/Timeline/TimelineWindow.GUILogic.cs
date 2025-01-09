using System;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.Common;


namespace binc.PixelAnimator.Editor.Windows{

    public partial class TimelineWindow{

        
        public override void ProcessWindow(){
            SetRect();
            ClampTimelinePosition();
            SetMouseIconState();
            SetReSizingState();
            RenderWindow();
            if (!SelectedAnim) return;
            if(IsPlaying) Play();
        }

        private void ClampTimelinePosition(){
            var height = PixelAnimatorWindow.AnimatorWindow.position.height;
            if(windowRect.y < 200) windowRect.position = new Vector2(windowRect.x, 200);
            if(windowRect.y > height) windowRect.position = new Vector2(windowRect.x,400);
        }

        public override void FocusFunctions(){
            if(!SelectedAnim) return;
            SetShortcuts();
        }
        public void SetShortcuts(){
            var eventCurrent = Event.current;
            if(eventCurrent.type != EventType.KeyDown) return;
            var keyCode = eventCurrent.keyCode;
            switch (keyCode){
                case KeyCode.Return:
                    playPauseButton.DownClick();
                    break;
                case KeyCode.LeftArrow:
                    previousNextSpriteButton.DownClick(true);
                break;
                case KeyCode.RightArrow:
                    previousNextSpriteButton.DownClick(false);
                break;
            }
        }

        private void FrameButton(ValueTuple<int, int, int> data){
            var groupIndex = data.Item1;
            var layerIndex = data.Item2;
            var frameIndex = data.Item3;
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var groups = SelectedAnim.BoxGroups;
            var layers = groups[groupIndex].boxes;
            var frames = layers[layerIndex].frames;
            var isSameFrameIndex = frameIndex == animatorWindow.IndexOfSelectedSprite;
            var isSameLayerIndex = layerIndex == animatorWindow.IndexOfSelectedLayer;
            var isSameGroupIndex = groupIndex == animatorWindow.IndexOfSelectedGroup;
            var frame = frames[frameIndex];            
            
            if(isSameLayerIndex && isSameGroupIndex){
                var previousType = BoxFrameType.None;
                if(frameIndex > 0){
                    previousType = frames[frameIndex-1].GetFrameType();
                }
                frame.SetType(ChangeFrameType(frame.GetFrameType(), previousType, frameIndex));
            } 
            
            if(isSameFrameIndex){ 
                 animatorWindow.SelectGroup(groupIndex);
                 animatorWindow.SelectLayer(layerIndex);
            } 
            PixelAnimatorWindow.AnimatorWindow.Repaint();

        }
        private BoxFrameType ChangeFrameType(BoxFrameType type, BoxFrameType previousType, int index){
            switch (type){
                case BoxFrameType.KeyFrame:
                    var isValid = index > 0 && previousType != BoxFrameType.EmptyFrame && previousType != BoxFrameType.None;
                    if(isValid)return BoxFrameType.CopyFrame;
                    goto case BoxFrameType.CopyFrame;
                case BoxFrameType.CopyFrame:
                    return BoxFrameType.EmptyFrame;
                case BoxFrameType.EmptyFrame:
                    return BoxFrameType.KeyFrame;
                default:
                    return BoxFrameType.None;
            }
        }
        private void BoxButton(ValueTuple<BoxGroup, Box> data){
            layerMenu = new GenericMenu();
            
            layerMenu.AddItem(new GUIContent("Delete Box"), false, ()=>{data.Item1.boxes.Remove(data.Item2);});
            layerMenu.ShowAsContext();
        }

        private void GroupButton(BoxGroup boxGroup){
            groupMenu = new GenericMenu();
            groupMenu.AddItem(new GUIContent("Delete BoxGroup"), false, ()=>{SelectedAnim.RemoveGroup(boxGroup.BoxDataGuid);});
            groupMenu.AddItem(new GUIContent("Add Box"), false, ()=>{boxGroup.AddBox(SelectedAnim.PixelSprites);});
            groupMenu.AddItem(new GUIContent("Expand"), boxGroup.isExpanded, () => {boxGroup.isExpanded = !boxGroup.isExpanded;});
            groupMenu.AddItem(new GUIContent("Visible"), boxGroup.isVisible, () => {boxGroup.isVisible = !boxGroup.isVisible;});
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
                burgerMenu.AddItem(new GUIContent($"Add BoxGroup/{boxData[i].boxName}"), false, 
                obj=>{SelectedAnim.AddGroup(boxData[(int)obj].Guid);},i);
            }
            burgerMenu.AddItem(new GUIContent("Go to preferences"),false, () => { EditorGUIUtility.PingObject(PixelAnimatorWindow.AnimatorWindow.AnimationPreferences); });
        }

        private void ThumbnailButton(int index)
        {
            var animWindow = PixelAnimatorWindow.AnimatorWindow;
            animWindow.SelectSprite(index);
        }

        private void PlayPauseButton()
        {
            timer = 0;
            IsPlaying = !IsPlaying;
            playPauseTex = IsPlaying ? pauseTex : playTex;
            PixelAnimatorWindow.AnimatorWindow.Repaint();
        }

        private void Play(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var fps = SelectedAnim.fps;
            if(fps == 0) Debug.Log("BoxFrame rate is zero");
            var deltaTime = animatorWindow.EditorDeltaTime;
            timer += deltaTime;
            if(timer >= 1f/fps){
                timer -= 1f/fps;
                var frame = (  animatorWindow.IndexOfSelectedSprite +1 ) % SelectedAnim.GetSpriteList().Count;
                animatorWindow.SelectSprite(frame);
               
            }
            animatorWindow.Repaint();

        }
        
        private void ChangeSpriteButton(bool isPrevious)
        {
            if (!SelectedAnim) return;
            var factor = isPrevious ? -1 : 1;
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var mod = SelectedAnim.GetSpriteList().Count;
            var index = (animatorWindow.IndexOfSelectedSprite + factor) % mod;
            index = index == -1 ? mod-1 : index;
            animatorWindow.SelectSprite(index);
            PixelAnimatorWindow.AnimatorWindow.Repaint();
        }
        
        #region Rect
        private void SetRect(){
            var animatorWindowRect = PixelAnimatorWindow.AnimatorWindow.position;
            // windowRect.size = new Vector2(animatorWindowRect.size.x, animatorWindowRect.height - windowRect.position.y);
            handleRect = new Rect(0, 0, windowRect.width, HandleHeight);
            columnRect = new Rect(GroupPanelWidth, HandleHeight, ColumnWidth, windowRect.height);
            rowRect = new Rect(0, ToolPanelHeight + HandleHeight, windowRect.width, RowHeight);
            groupPlaneRect = new Rect(0, rowRect.yMax, windowRect.width, windowRect.height - rowRect.yMax);
            thumbnailPlaneRect = new Rect(columnRect.xMax, HandleHeight, windowRect.width - columnRect.xMax,ToolPanelHeight);
            toolPanelRect = new Rect(10,8+HandleHeight, columnRect.x, ToolPanelHeight);
            var availableSpace = new Rect(0,0, animatorWindowRect.width, windowRect.y);
            PixelAnimatorWindow.AnimatorWindow.SetAvailableRect(availableSpace);
            
            ReSizeWindowRect();
        }
        private void SetReSizingState(){
            var r = new Rect(windowRect.position, handleRect.size);
            var eventCurrent = Event.current;
            var eventType = eventCurrent.type;
            var leftCLicked = eventType == EventType.MouseDown && eventCurrent.button == 0;
            var inHandleRect = r.Contains(eventCurrent.mousePosition);
            if(leftCLicked && inHandleRect) reSizing = true;
            var isMouseUp = eventType == EventType.MouseUp;
            if(isMouseUp) reSizing = false;
        }
        private void ReSizeWindowRect(){
            if(!reSizing) return;
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var animatorRect = animatorWindow.position;
            if (Event.current.mousePosition.y > 100 && Event.current.mousePosition.y < animatorRect.height - 100)
            {
                windowRect.height = animatorRect.height - Event.current.mousePosition.y;
                animatorWindow.Repaint();
            }

        }
        private void SetMouseIconState(){
            var r = new Rect(windowRect.position, handleRect.size);
            EditorGUIUtility.AddCursorRect(r, MouseCursor.ResizeVertical);
            PixelAnimatorWindow.AddCursorBool(reSizing, MouseCursor.ResizeVertical);
        }
        #endregion
    }
}