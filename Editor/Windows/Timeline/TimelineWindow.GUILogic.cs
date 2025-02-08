using System;
using UnityEngine;
using UnityEditor;


namespace binc.PixelAnimator.Editor.Windows{

    public partial class TimelineWindow{
        public override void ProcessWindow(){
            CalculateRects();
            ClampTimelinePosition();
            SetMouseIconState();
            SetResizingState();
            
            RenderTimeline();
            FocusWindowIfClicked();
            if (!SelectedAnim) return;
            SetShortcuts();
            if(IsPlaying) Play();
            
        }
        
        private void RenderTimeline() => GUI.Window(Id, windowRect, _ => RenderWindowContent(), GUIContent.none, GUIStyle.none);
        
        private void RenderWindowContent()
        {
            HandleDragScroll();

            DrawBackgrounds();
            DrawGridLines();
            DrawToolButtons();
            if (SelectedAnim)
            {
                DrawThumbnailPanelAndFrame();
                if(!IsPlaying )DrawGroupPanel();
            }


        }
        
        private void HandleDragScroll()
        {
            if (Event.current.type != EventType.MouseDrag || Event.current.button != 2) return;
            GUI.FocusControl(null);
            scrollPosition += Event.current.delta*-1;
            if (scrollPosition.x < 0) scrollPosition.x = 0;
            if (scrollPosition.y < 0) scrollPosition.y = 0;
            Event.current.Use();
        }
        
        private void FocusWindowIfClicked()
        {
            if (!windowRect.IsClickedRect(0)) return;
            Event.current.Use();
            GUI.FocusWindow(Id);
        }

        private void ClampTimelinePosition(){
            var height = PixelAnimatorWindow.AnimatorWindow.position.height;
            if (windowRect.height <= 0) windowRect.height = 200;
            if(windowRect.y < 200) windowRect.position = new Vector2(windowRect.x, 200);
            if(windowRect.y > height) windowRect.position = new Vector2(windowRect.x,400);
        }
        
        public void SetShortcuts(){
            var eventCurrent = Event.current;

            if (windowRect.IsClickedRect(2))
            {
                Event.current.Use();
            }
            
            if(eventCurrent.type != EventType.KeyDown) return;
            var keyCode = eventCurrent.keyCode;
            switch (keyCode){
                case KeyCode.Return:
                    playPauseButton.DownClick();
                    Event.current.Use();
                    break;
                case KeyCode.LeftArrow:
                    previousNextSpriteButton.DownClick(true);
                    Event.current.Use();
                break;
                case KeyCode.RightArrow:
                    previousNextSpriteButton.DownClick(false);
                    Event.current.Use();
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
            var isSameLayerIndex = layerIndex == animatorWindow.IndexOfSelectedBox;
            var isSameGroupIndex = groupIndex == animatorWindow.IndexOfSelectedBoxGroup;
            
            if(isSameLayerIndex && isSameGroupIndex){
                layers[layerIndex].SetFrameType(frameIndex);
            } 
            
            if(isSameFrameIndex){ 
                 animatorWindow.SelectBoxGroup(groupIndex);
                 animatorWindow.SelectBox(layerIndex);
            } 
        }
        private void BoxButton(ValueTuple<BoxGroup, Box> data){
            boxMenu = new GenericMenu();
            
            boxMenu.AddItem(new GUIContent("Delete Box"), false, ()=>{data.Item1.boxes.Remove(data.Item2);});
            boxMenu.ShowAsContext();
        }

        private void GroupButton(BoxGroup boxGroup){
            boxGroupMenu = new GenericMenu();
            boxGroupMenu.AddItem(new GUIContent("Add Box"), false, ()=>{boxGroup.AddBox(SelectedAnim.PixelSprites);});
            boxGroupMenu.ShowAsContext();
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

        private void ThumbnailButton(Tuple<int, Sprite> data)
        {
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            switch (Event.current.button)
            {
                case 1:
                {
                    var menu = new GenericMenu();
                    if(data.Item2)menu.AddItem(new GUIContent("Go to reference"), false, () => { EditorGUIUtility.PingObject(data.Item2); });
                    menu.AddItem(new GUIContent("Delete"), false, () =>
                    {
                        animatorWindow.RemovePixelSprite(data.Item1);
                    });

                    menu.ShowAsContext();
                    break;
                }
                case 0:
                    animatorWindow.SelectSprite(data.Item1);
                    break;
            }
        }

        private void Play(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var fps = SelectedAnim.fps;
            var count = animatorWindow.GetSpriteCount();

            if (fps == 0)
            {
                Debug.LogWarning("FPS is 0");
                return;
            }
            if (count == 0)
            {
                Debug.LogWarning("No sprites in animation");
                return;
            }
            var deltaTime = animatorWindow.EditorDeltaTime;
            timer += deltaTime;
            if(timer >= 1f/fps){
                timer -= 1f/fps;
                var frame = (  animatorWindow.IndexOfSelectedSprite +1 ) % count;
                animatorWindow.SelectSprite(frame);
            }
            animatorWindow.Repaint();
        }
        
        private void PlayPauseButton()
        {
            GUI.FocusControl("PlayButton");
            timer = 0;
            IsPlaying = !IsPlaying;
            playPauseTex = IsPlaying ? pauseTex : playTex;
            
        }
        
        private void PingAnimationButton()
        {
            EditorGUIUtility.PingObject(SelectedAnim);
        }
        
        private void ChangeSpriteButton(bool isPrevious)
        {
            if (!SelectedAnim) return;
            int factor;
            if (isPrevious)
            {
                factor = -1;
                GUI.FocusControl("PreviousFrameButton");
            }
            else
            {
                factor = 1;
                GUI.FocusControl("NextFrameButton");
            }
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var mod = SelectedAnim.GetSpriteList().Count;
            var index = (animatorWindow.IndexOfSelectedSprite + factor) % mod;
            index = index == -1 ? mod-1 : index;
            animatorWindow.SelectSprite(index);
            
        }
        
        #region Rect
        private void CalculateRects(){
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            handleShadowRect = new Rect(0, HandleHeight, windowRect.width, RowHeight / 2);
            toolPanelRect = new Rect(0,HandleHeight+handleShadowRect.height, columnRect.x, toolBarSize.y);

            handleRect = new Rect(0, 0, windowRect.width, HandleHeight);
            columnRect = new Rect(groupStyle.fixedWidth, HandleHeight, ColumnWidth, windowRect.height);
            rowRect = new Rect(0, toolPanelRect.yMax, windowRect.width, RowHeight);
            bottomAreaRect = new Rect(0, rowRect.yMax, windowRect.width, windowRect.height - rowRect.yMax);
            
            frameAreaRect = new Rect(0, rowRect.yMax - HandleHeight, toolBarSize.x * animatorWindow.GetSpriteCount() , bottomAreaRect.height);
            thumbnailPanelRect = new Rect(columnRect.xMax, toolPanelRect.y, windowRect.width - columnRect.xMax, toolPanelRect.height);
            ResizeWindowRect();
            windowRect.y = animatorWindow.position.height - windowRect.height;
            windowRect.width = animatorWindow.position.width;
            
        }
        private void SetResizingState(){
            var r = new Rect(windowRect.position, handleRect.size);
            var eventCurrent = Event.current;
            var eventType = eventCurrent.type;
            var leftCLicked = eventType == EventType.MouseDown && eventCurrent.button == 0;
            var inHandleRect = r.Contains(eventCurrent.mousePosition);
            if (leftCLicked && inHandleRect)
            {
                reSizing = true;
                Event.current.Use();
            }
            var isMouseUp = eventType == EventType.MouseUp;
            if(isMouseUp) reSizing = false;
        }
        private void ResizeWindowRect(){
            if(!reSizing) return;
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var animatorRect = animatorWindow.position;
            var mouseYPos = Event.current.mousePosition.y;
            var isOutInterval = !(mouseYPos > 100) || !(mouseYPos < animatorRect.height - 100);
            if (isOutInterval) return;
            windowRect.height = animatorRect.height - Event.current.mousePosition.y;
            animatorWindow.Repaint();
        }
        private void SetMouseIconState(){
            var globalHandleRect = new Rect(windowRect.position, handleRect.size);
            EditorGUIUtility.AddCursorRect(globalHandleRect, MouseCursor.ResizeVertical);
            PixelAnimatorUtility.AddCursorCondition(PixelAnimatorWindow.AnimatorWindow.position, reSizing, MouseCursor.ResizeVertical);
        }
        #endregion

        private void VisibilityButton(BoxGroup boxGroup){
            GUI.FocusControl("VisibilityButton " + $"{boxGroup.BoxDataGuid}");
            boxGroup.isVisible = !boxGroup.isVisible;
        }

        private void ExpandGroupButton(BoxGroup boxGroup)
        {
            GUI.FocusControl("ExpandGroupButton " + $"{boxGroup.BoxDataGuid}");
            boxGroup.isExpanded = !boxGroup.isExpanded;
        }

        private void ColliderButton(BoxGroup boxGroup)
        {
            GUI.FocusControl("ColliderButton " + $"{boxGroup.BoxDataGuid}");
            boxGroup.collisionTypes = boxGroup.collisionTypes switch
            {
                CollisionTypes.Collider => CollisionTypes.Trigger,
                CollisionTypes.Trigger => CollisionTypes.Collider,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}