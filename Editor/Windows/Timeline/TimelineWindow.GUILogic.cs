using System;
using UnityEngine;
using UnityEditor;
using binc.PixelAnimator.AnimationData;


namespace binc.PixelAnimator.Editor.Windows{

    public partial class TimelineWindow{
        public override void ProcessWindow(){
            SetRect();
            ClampTimelinePosition();
            SetMouseIconState();
            SetResizingState();
            RenderWindow();
            FocusWindowIfClicked();
            if (!SelectedAnim) return;
            SetShortcuts();
            if(IsPlaying) Play();
            
        }
        
        private void RenderWindow() => GUI.Window(Id, windowRect, _ => RenderWindowContent(), GUIContent.none, GUIStyle.none);
        
        private void RenderWindowContent()
        {


            DrawBackgrounds();
            DrawGridLines();
            DrawToolButtons();
            // DrawThumbnailPanelAndFrame();
            if(!IsPlaying)DrawGroupPanel();
            HandleDragScroll();

            if (windowRect.height == 0)
            {
                windowRect.height = 200;
            }

            if(Event.current.button == 0 && Event.current.type == EventType.MouseDown){
                GUI.FocusControl(null);
            }
        }
        
        private void HandleDragScroll()
        {
            if (Event.current.type != EventType.MouseDrag || Event.current.button != 2) return;
            scrollPosition += Event.current.delta*-1;
            if (scrollPosition.x < 0) scrollPosition.x = 0;
            if (scrollPosition.y < 0) scrollPosition.y = 0;
            Event.current.Use();
            PixelAnimatorWindow.AnimatorWindow.Repaint();
        }
        
        private void FocusWindowIfClicked()
        {
            if (!windowRect.IsClickedRect(0)) return;
            Event.current.Use();
            GUI.FocusWindow(Id);
        }

        private void ClampTimelinePosition(){
            var height = PixelAnimatorWindow.AnimatorWindow.position.height;
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
                    Event.current.Use();
                    playPauseButton.DownClick();
                    break;
                case KeyCode.LeftArrow:
                    Event.current.Use();
                    previousNextSpriteButton.DownClick(true);
                break;
                case KeyCode.RightArrow:
                    Event.current.Use();
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
            var isSameLayerIndex = layerIndex == animatorWindow.IndexOfSelectedBox;
            var isSameGroupIndex = groupIndex == animatorWindow.IndexOfSelectedBoxGroup;
            var frame = frames[frameIndex];            
            
            if(isSameLayerIndex && isSameGroupIndex){
                var previousType = BoxFrameType.None;
                if(frameIndex > 0){
                    previousType = frames[frameIndex-1].GetFrameType();
                }
                frame.SetType(ChangeFrameType(frame.GetFrameType(), previousType, frameIndex));
            } 
            
            if(isSameFrameIndex){ 
                 animatorWindow.SelectBoxGroup(groupIndex);
                 animatorWindow.SelectBox(layerIndex);
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
            if (Event.current.button == 1)
            {
                var menu = new GenericMenu();
                if(data.Item2)menu.AddItem(new GUIContent("Go to reference"), false, () => { EditorGUIUtility.PingObject(data.Item2); });
                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    animatorWindow.RemovePixelSprite(data.Item1);
                });

                menu.ShowAsContext();
            }
            else if (Event.current.button == 0)
            {
                animatorWindow.SelectSprite(data.Item1);
                animatorWindow.Repaint();      
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
            
            PixelAnimatorWindow.AnimatorWindow.Repaint();
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
            
            PixelAnimatorWindow.AnimatorWindow.Repaint();
        }
        
        #region Rect
        private void SetRect(){
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
            if (!(Event.current.mousePosition.y > 100) ||
                !(Event.current.mousePosition.y < animatorRect.height - 100)) return;
            windowRect.height = animatorRect.height - Event.current.mousePosition.y;
            animatorWindow.Repaint();
        }
        private void SetMouseIconState(){
            var r = new Rect(windowRect.position, handleRect.size);
            EditorGUIUtility.AddCursorRect(r, MouseCursor.ResizeVertical);
            PixelAnimatorWindow.AddCursorCondition(reSizing, MouseCursor.ResizeVertical);
        }
        #endregion

        private void VisibilityButton(BoxGroup boxGroup){
            GUI.FocusControl("VisibilityButton " + $"{boxGroup.BoxDataGuid}");
            boxGroup.isVisible = !boxGroup.isVisible;
            PixelAnimatorWindow.AnimatorWindow.Repaint();
        }

        private void ExpandGroupButton(BoxGroup boxGroup)
        {
            GUI.FocusControl("ExpandGroupButton " + $"{boxGroup.BoxDataGuid}");
            boxGroup.isExpanded = !boxGroup.isExpanded;
            PixelAnimatorWindow.AnimatorWindow.Repaint();
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
            PixelAnimatorWindow.AnimatorWindow.Repaint();
        }
    }
}