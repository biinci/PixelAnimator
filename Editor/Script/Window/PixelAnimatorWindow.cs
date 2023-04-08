using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using binc.PixelAnimator.DataProvider;
using binc.PixelAnimator.Common;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.Utility;


namespace binc.PixelAnimator.Editor.Window{

    [Serializable]
    public class PixelAnimatorWindow : EditorWindow{

        #region Variable
        
        private const string PixelAnimatorPath = "Assets/binc/PixelAnimator/";

        public Rect TimelineRect => timelineRect;
        private Rect timelineRect;
        private Rect partitionHandleRect,
            burgerRect,
            backRect,
            playRect,
            frontRect,
            columnLayout,
            rowLayout;

        private Rect propertyWindowRect = new(10, 6, 120, 20);
        private Rect[] columnRects;


        [SerializeField] private List<GroupEditorData> groupEditorData;


        public int ActiveFrameIndex{get; private set;}
        public int ActiveGroupIndex{get; private set;} 
        public int ActiveLayerIndex{get; private set;}
        public PixelAnimation SelectedAnim{get; private set;}

        //Editor Delta Time
        private float timer, editorDeltaTime;
        private float lastTimeSinceStartup;

        private bool isPlaying, dragTimeline;

        public PixelAnimatorPreferences Preferences{get; private set;}

        //Timeline textures.
        [SerializeField] private Texture2D backTex,
            playTex,
            frontTex,
            timelineBurgerTex,
            onMouseTimelineBurgerTex,
            singleFrameTex,
            linkedFrameTex,
            emptyFrameTex,
            keyFrameTex,
            stopTex,
            durationTex,
            closeTex,
            openTex,
            settingsTex,
            unvisibleTex,
            visibleTex,
            layerBurgerMenu,
            triggerTex,
            noTriggerTex;


        [SerializeField] private Texture2D spritePreview;

        public SerializedObject TargetAnimation{get; private set;}


        [SerializeField] private int spriteScale;

        private Vector2 biggerSpriteSize;
        private GenericMenu timelinePopup, settingsPopup, layerPopup;

        private Vector2 propertyXScrollPos = Vector2.one;
        private Vector2 propertyYScrollPos = Vector2.one;

        private bool eventFoldout;
        public bool RightClicked { get; private set; }
        public bool LeftClicked { get; private set; }
        public bool MiddleClicked { get; private set; }
        [SerializeField] private bool loadedResources;

        public Event EventCurrent => Event.current;


        public WindowEnum WindowFocus{get; set;}
        public PropertyFocusEnum PropertyFocus{get; set;}

        public CanvasWindow CanvasWindow{get; private set;}
        public TimelineWindow TimelineWindow{get; private set;}
        public CustomWindow FocusedWindow { get; private set; }

        public const int CanvasId = 1;
        public const int TimelineId = 2;

        

        #endregion

        #region Initialize

        [MenuItem("Window/Pixel Animator")]
        private static void InitWindow(){ //okey
            var window = GetWindow<PixelAnimatorWindow>("Pixel Animator");
            window.minSize = new Vector2(150, 450);
            window.Show();
            var icon = Resources.Load<Texture2D>("Sprites/PixelAnimatorIcon");
            window.titleContent = new GUIContent("Pixel Animator", icon);
        }

        private void OnEnable(){
            loadedResources = false;
            //Set Rect
            SetInitRect();
            //Check selected object
            SelectedObject();
            //Load textures
            LoadInitResources();


            SetLayerMenu();
            SetGroupMenu();

        }

        private void SetInitRect(){ //okey
            const float buttonSize = 24; // set button rect
            burgerRect = new Rect(15, 5, 32, 32);
            backRect = new Rect(200, 10, buttonSize, buttonSize);
            playRect = new Rect(backRect.width + backRect.xMin + 2, backRect.yMin, buttonSize, buttonSize);
            frontRect = new Rect(playRect.width + playRect.xMin + 2, backRect.yMin, buttonSize, buttonSize);

            groupEditorData ??= new List<GroupEditorData>();
        }

        private void LoadInitResources(){ //okey
            Preferences = Resources.Load<PixelAnimatorPreferences>("PixelAnimatorPreferences"); // load resources
            backTex = Resources.Load<Texture2D>("Sprites/Back");
            frontTex = Resources.Load<Texture2D>("Sprites/Front");
            timelineBurgerTex = Resources.Load<Texture2D>("Sprites/TimelineBurgerMenu");
            onMouseTimelineBurgerTex = Resources.Load<Texture2D>("Sprites/TimelineBurgerMenu2");
            singleFrameTex = Resources.Load<Texture2D>("Sprites/frame");
            linkedFrameTex = Resources.Load<Texture2D>("Sprites/linked frame");
            keyFrameTex = Resources.Load<Texture2D>("Sprites/key frame");
            emptyFrameTex = Resources.Load<Texture2D>("Sprites/empty frame");
            stopTex = Resources.Load<Texture2D>("Sprites/Stop");
            playTex = Resources.Load<Texture2D>("Sprites/Play");
            closeTex = Resources.Load<Texture2D>("Sprites/up");
            openTex = Resources.Load<Texture2D>("Sprites/down");
            settingsTex = Resources.Load<Texture2D>("Sprites/settings");
            layerBurgerMenu = Resources.Load<Texture2D>("Sprites/LayerBurgerMenu");
            visibleTex = Resources.Load<Texture2D>("Sprites/visible");
            unvisibleTex = Resources.Load<Texture2D>("Sprites/unvisible");
            triggerTex = Resources.Load<Texture2D>("Sprites/trigger");
            noTriggerTex = Resources.Load<Texture2D>("Sprites/notrigger");
            durationTex = playTex;

            CanvasWindow = new CanvasWindow(this, WindowEnum.Canvas);
            TimelineWindow = new TimelineWindow(this, WindowEnum.Timeline);
            FocusedWindow = TimelineWindow;
            WindowFocus = WindowEnum.Timeline;
            loadedResources = true;

        }

        #endregion
        
        private void OnGUI(){
            if(!loadedResources) LoadInitResources();

            LeftClicked = PixelAnimatorUtility.GetClicked(Event.current, 0);
            RightClicked = PixelAnimatorUtility.GetClicked(Event.current, 1);
            MiddleClicked = PixelAnimatorUtility.GetClicked(Event.current, 2);



            EditorGUI.DrawRect(new Rect(Vector2.zero, maxSize), new Color(0.13f, 0.13f, 0.15f));
            SelectedObject();
            SetWindows();
            SetBurgerMenu();

            
            if (SelectedAnim == null || SelectedAnim.Groups == null) return;


            if (isPlaying) PropertyFocus = PropertyFocusEnum.Sprite;
            if (SelectedAnim.Groups.Count <= 0) CanvasWindow.SetHandle(HandleTypes.None);
            SetFrameCopyPaste();
            EditorGUI.LabelField(new Rect(600, 300, 300, 200), ActiveGroupIndex + "   " + ActiveLayerIndex + "   " + ActiveFrameIndex);
            EditorGUI.LabelField(new Rect(50, 50, 300, 200), WindowFocus.ToString());

            SetWindowFocus(CanvasWindow.WindowRect, WindowEnum.Canvas);
            //SetWindowFocus(TimelineWindow.WindowRect, WindowEnum.Canvas, false);
            //SetWindowFocus(propertyWindowRect, WindowEnum.Canvas, false);
            TimelineWindow.IsFocusChangeable = true;

            SetWindowFocus(TimelineWindow.WindowRect, WindowEnum.Timeline);

            //Debug.Log(Event.current.type);
            //Debug.Log("Timeline: " + IsMouseOnWindow(WindowEnum.Timeline));
            //Debug.Log("Canvas: " + IsMouseOnWindow(WindowEnum.Canvas));
            SetEditorDeltaTime();
            if(isPlaying) Play();

        }

        private void SetWindows(){
            var eventCurrent = Event.current;

            BeginWindows();
            if (TargetAnimation != null) {

                if (SelectedAnim.GetSpriteList().Count > 0) {

                    if (!isPlaying) DrawPropertyWindow();
                    CanvasWindow.SetWindow(eventCurrent);
                    //DrawCanvas(eventCurrent);
                }
                else {
                    ZeroSpriteCount();
                }
            }
            else {
                NoAnimation();
            }


            // CreateTimeline(eventCurrent);
            TimelineWindow.SetWindow(eventCurrent);
            EndWindows();
            
            SetFocus();


            

            // DragTimeline(eventCurrent);

            //Window layout.            
            GUI.BringWindowToFront(4);
            GUI.BringWindowToFront(5);
            
            GUI.BringWindowToBack(7);
            GUI.BringWindowToBack(CanvasId);
            GUI.BringWindowToFront(TimelineId);


        }

        private void SetFocus(){
            switch (WindowFocus) {
                case WindowEnum.Timeline or WindowEnum.Canvas:
                    if (Event.current.type == EventType.MouseDown) {
                        GUI.FocusControl(null);
                    }
                    break;
            }
            
            if (GUI.GetNameOfFocusedControl() != "") return;
            SetGroupKeys();
            SetPlayKeys();
        }


        #region Property

        #region Drawing
        private void DrawProperties(PropertyType propertyType, string header){
            if (SelectedAnim == null || SelectedAnim.Groups.Count == 0) return;

            using var xScroll = new EditorGUILayout.ScrollViewScope(propertyXScrollPos);
            using var yScroll = new EditorGUILayout.ScrollViewScope(propertyYScrollPos);

            EditorGUI.LabelField(propertyWindowRect, header, EditorStyles.boldLabel); // Drawing header
            
            var rect = new Rect(7, 30, 300, 2f);
            var color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            EditorGUI.DrawRect(rect, color); //Drawing parting line.

            GUILayout.Space(30);
            propertyXScrollPos = xScroll.scrollPosition;

            using (new GUILayout.HorizontalScope()) {
                GUILayout.Space(20);

                using (new GUILayout.VerticalScope()) {
                    GUILayout.Space(20);
                    switch (propertyType) {
                        case PropertyType.Sprite:
                            DrawSpriteProp();
                            break;
                        case PropertyType.HitBox:
                            DrawHitBoxProp();
                            break;
                    }


                }
            }

        }

        private void DrawSpriteProp(){
            TargetAnimation.Update();
            var propPixelSprite = TargetAnimation.FindProperty("pixelSprites")
                .GetArrayElementAtIndex(ActiveFrameIndex);

            var propSpriteData = propPixelSprite.FindPropertyRelative("spriteData");
            var propSpriteEventNames = propSpriteData.FindPropertyRelative("eventNames");
            var propSpriteDataValues = propSpriteData.FindPropertyRelative("genericData");
            var spriteDataValues = SelectedAnim.PixelSprites[ActiveFrameIndex].SpriteData
                .genericData;

            foreach (var prop in Preferences.SpriteProperties) {
                var single = spriteDataValues.FirstOrDefault(x => x.baseData.Guid == prop.Guid)
                    .baseData;
                var selectedIndex = single == null
                    ? -1
                    : spriteDataValues.FindIndex(x => x.baseData == single);
                SetPropertyField(prop, propSpriteDataValues, single, selectedIndex);
            }

            DrawEventField(propSpriteEventNames);
            TargetAnimation.ApplyModifiedProperties();
        }

        private void DrawHitBoxProp(){
            if (SelectedAnim.Groups[ActiveGroupIndex].layers.Count <= 0) return;
            var group = TargetAnimation.FindProperty("groups").GetArrayElementAtIndex(ActiveGroupIndex);
            var layer = group.FindPropertyRelative("layers").GetArrayElementAtIndex(ActiveLayerIndex);
            var frames = layer.FindPropertyRelative("frames");
            var frame = frames.GetArrayElementAtIndex(ActiveFrameIndex);
            
            
            using (new GUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField("Box", GUILayout.Width(70));
                var propHitBoxRect = frame.FindPropertyRelative("hitBoxRect");
                EditorGUILayout.PropertyField(propHitBoxRect, GUIContent.none, GUILayout.Width(140),
                    GUILayout.MaxHeight(60));
            }
            
            TargetAnimation.ApplyModifiedProperties();
            
            var dataProps = frame.FindPropertyRelative("hitBoxData");
            var eventNamesProp = dataProps.FindPropertyRelative("eventNames");
            var dataValuesProp = dataProps.FindPropertyRelative("genericData");

            var hitBoxDataValues = SelectedAnim.Groups[ActiveGroupIndex].layers[ActiveLayerIndex].frames[ActiveFrameIndex].HitBoxData.genericData;
            foreach (var dataWareHouse in Preferences.HitBoxProperties) {
                var single = hitBoxDataValues.FirstOrDefault(x => x.baseData.Guid == dataWareHouse.Guid) // is property  exist?
                    .baseData;
                var selectedIndex = single == null
                    ? -1
                    : hitBoxDataValues.FindIndex(x => x.baseData == single);
                SetPropertyField(dataWareHouse, dataValuesProp, single, selectedIndex);
            }
            
            DrawEventField(eventNamesProp);
        }

        private void DrawPropertyWindow(){
            var tempColor = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.2f);
            var windowRect = new Rect(10, 10, 250, 250); // Background rect.

            switch (PropertyFocus) {
                case PropertyFocusEnum.HitBox:
                    if (SelectedAnim.Groups.Count == 0) {
                        PropertyFocus = PropertyFocusEnum.Sprite;
                        break;
                    }
                    if(ActiveGroupIndex >= SelectedAnim.Groups.Count) CheckAndFixVariable();
                    if (SelectedAnim.Groups[ActiveGroupIndex].layers[ActiveLayerIndex].frames[ActiveFrameIndex]
                            .frameType != FrameType.KeyFrame) break;
                    GUI.Window(4, windowRect,
                        _ => { DrawProperties(PropertyType.HitBox, "HitBox Properties"); }, GUIContent.none);
                    break;
                case PropertyFocusEnum.Sprite:
                    GUI.Window(5, windowRect,
                        _ => { DrawProperties(PropertyType.Sprite, "Sprite Properties"); }, GUIContent.none);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (propertyWindowRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown) {
                WindowFocus = WindowEnum.Property;
            }

            GUI.color = tempColor;

        }
        
        private void DrawEventField(SerializedProperty eventNames){

            eventFoldout = EditorGUILayout.Foldout(eventFoldout, "Event Names", true);
            if (eventFoldout == false) return;
            for (var i = 0; i < eventNames.arraySize; i++) {
                var methodName = eventNames.GetArrayElementAtIndex(i);
                using (new GUILayout.HorizontalScope()) {
                    EditorGUILayout.PropertyField(methodName, GUIContent.none, GUILayout.MaxWidth(100));
                    if (GUILayout.Button("X", GUILayout.MaxWidth(15), GUILayout.MaxHeight(15))) {
                        eventNames.DeleteArrayElementAtIndex(i);
                        eventNames.serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            if (GUILayout.Button("Add Event", GUILayout.MaxWidth(100))) {
                eventNames.arraySize++;
                eventNames.serializedObject.ApplyModifiedProperties();
            }

            eventNames.serializedObject.ApplyModifiedProperties();

        }
        
        #endregion

        
        private void SetPropertyField(BasicPropertyData propData, SerializedProperty propertyValues,
            BaseData baseData, int baseDataIndex){
            propertyValues.serializedObject.Update();
            var alreadyExist = baseData != null;


            using (new GUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField(propData.Name, GUILayout.MaxWidth(70));


                if (alreadyExist) {
                    var propertyData = propertyValues.GetArrayElementAtIndex(baseDataIndex).FindPropertyRelative("baseData")
                        .FindPropertyRelative("data");
                    EditorGUILayout.PropertyField(propertyData, GUIContent.none, GUILayout.Width(90));
                    propertyValues.serializedObject.ApplyModifiedProperties();
                }
                else {
                    PixelAnimatorUtility.SystemObjectPreviewField(
                        PixelAnimatorUtility.DataTypeToSystemObject(propData.dataType), GUILayout.Width(90));
                }


                GUILayout.Space(10);
                if (GUILayout.Button("X", GUILayout.MaxWidth(15), GUILayout.MaxHeight(15))) { // Drawing added or remove button.
                    if (alreadyExist) {
                        propertyValues.DeleteArrayElementAtIndex(baseDataIndex);
                    }
                    else {
                       PixelAnimatorUtility.AddPropertyValue(propertyValues, propData);

                    }
                    propertyValues.serializedObject.ApplyModifiedProperties();


                }
            }

        }



        #endregion

        #region Timeline

      

        private void DrawTimeline(int windowID){

            columnLayout = new Rect(frontRect.xMax + 15, 0, 3, timelineRect.height);

            EditorGUI.DrawRect(new Rect(0, 0, timelineRect.width, timelineRect.height), new Color(0.1f, 0.1f, 0.1f, 1));

            EditorGUI.DrawRect(new Rect(columnLayout.xMax, rowLayout.yMax, timelineRect.width, timelineRect.height),
                new Color(0.16f, 0.17f, 0.19f)
            );

            rowLayout = new Rect(0, biggerSpriteSize.y, timelineRect.width, 4);

            EditorGUI.DrawRect(rowLayout, Color.black);
            EditorGUI.DrawRect(columnLayout, Color.black);


            DrawTimelineButtons();
            SetPlayKeys();
            if (TargetAnimation == null) return;
            if (SelectedAnim.GetSpriteList().Count > 0) {
                SetSpriteThumbnail();
                DrawSelectedFrame();
                if (SelectedAnim.Groups.Count > 0) {
                    CreateGroupButtons();
                    if (SelectedAnim.Groups[ActiveGroupIndex].layers.Count > 0) {
                        if(!isPlaying)SetFrameRect();
                        
                    }       
                } 
            }
            SetGroupKeys();
        }

        private void DrawTimelineButtons(){
            using (new GUILayout.HorizontalScope()) {

                if (PixelAnimatorUtility.Button(timelineBurgerTex, onMouseTimelineBurgerTex, burgerRect)){
                    timelinePopup?.ShowAsContext();
                } 

                else if (SelectedAnim != null && SelectedAnim.GetSpriteList().Count > 0) {

                    if (PixelAnimatorUtility.Button(backTex, backRect)) {
                        if (SelectedAnim == null) return;
                        switch (ActiveFrameIndex) {
                            case 0:
                                ActiveFrameIndex = SelectedAnim.GetSpriteList().Count - 1;
                                break;
                            case > 0:
                                ActiveFrameIndex--;
                                break;
                        }

                        Repaint();
                    }

                    else if (PixelAnimatorUtility.Button(durationTex, playRect)) {
                        if (SelectedAnim == null) return;
                        isPlaying = !isPlaying;
                        if (isPlaying && SelectedAnim.fps == 0) Debug.Log("Frame rate is zero");
                        Repaint();
                    }
                    else if (PixelAnimatorUtility.Button(frontTex, frontRect)) {
                        if (SelectedAnim == null) return;
                        ActiveFrameIndex = (ActiveFrameIndex + 1) % SelectedAnim.GetSpriteList().Count;
                        Repaint();
                    }
                }

            }

            Repaint();

            durationTex = !isPlaying ? playTex : stopTex;
        }

        private void SetBurgerMenu(){
            timelinePopup = new GenericMenu{ allowDuplicateNames = true };

            timelinePopup.AddItem(new GUIContent("Go to Preferences"), false, TargetPreferences);
            timelinePopup.AddItem(new GUIContent("Update Animation"), false, UpdateAnimation);

            timelinePopup.AddSeparator("");
            var boxData = Preferences.BoxData;

            if (SelectedAnim == null) return;

            for (var i = 0; i < Preferences.BoxData.Count; i++) {
                timelinePopup.AddItem(new GUIContent(boxData[i].boxType), false, AddGroup, boxData[i]);
            }

        }


        private void SetSpriteThumbnail(){
            
            const int blankWidth = 48;
            const int columnHeight = 48;
            var sprites = SelectedAnim.GetSpriteList();
            biggerSpriteSize = new Vector2(blankWidth, blankWidth);//Must be deleted
            rowLayout.position = new Vector2(0, blankWidth);//Must be deleted

            if (columnRects.Length != sprites.Count) columnRects = new Rect[sprites.Count];
            
            for(var i = 0; i < columnRects.Length; i++){
                var sprite = sprites[i];
                var spriteTex = AssetPreview.GetAssetPreview(sprite);
                var column = columnRects[i];
                if(i > 0)
                    columnRects[i] = new Rect(columnRects[i-1].xMax + blankWidth, 0, 3, columnHeight);
                else
                    columnRects[0] = new Rect(columnLayout.xMax + blankWidth, 0, 3, columnHeight);

                EditorGUI.DrawRect(column, Color.black);

                var spriteCell = new Rect(column.xMin - blankWidth, column.yMin, blankWidth, blankWidth); // Setting the sprite texture rect
                var spriteWidth = sprite.rect.width < blankWidth ? sprite.rect.width : blankWidth;
                var spriteHeight = sprite.rect.height < blankWidth ? sprite.rect.height : blankWidth;
                float spriteXpos;
                float spriteYpos;
                
                if(spriteWidth != blankWidth){
                    spriteXpos = column.xMin - blankWidth * 0.5f - sprite.rect.width * 0.5f;
                }else{
                    spriteXpos = spriteCell.x;
                }
                
                if(spriteHeight != blankWidth){
                    spriteYpos = rowLayout.yMin - blankWidth * 0.5f - sprite.rect.height * 0.5f;
                }else{
                    spriteYpos = spriteCell.y;
                }

                var spriteTexRect = new Rect(spriteXpos, spriteYpos, spriteWidth, spriteHeight); //Texture rect
                if(spriteTex != null)
                    GUI.DrawTexture(spriteTexRect, spriteTex);

                if(spriteCell.Contains(Event.current.mousePosition) && LeftClicked){
                    ActiveFrameIndex = i;
                    PropertyFocus = PropertyFocusEnum.Sprite;
                }

                if (i != ActiveFrameIndex) continue; 
                
                var transparentRect = new Rect(column.xMin - blankWidth, 0, blankWidth, timelineRect.height); //Setting the transparent rect.
                EditorGUI.DrawRect(transparentRect, new Color(1, 1, 1, 0.1f)); 
                if(i > 0){
                    columnRects[i].height = timelineRect.height;
                    columnRects[i-1].height = timelineRect.height;
                }
                else
                    columnRects[i].height = timelineRect.height;


            }

            EditorGUI.DrawRect(rowLayout, Color.black);// Must be deleted

        }


        private void DrawGroup(IReadOnlyList<Group> groups){

            var width = columnLayout.xMin - rowLayout.xMin;
            const int height = 35;
            const int bottomLineHeight = 3;

            groupEditorData ??= new List<GroupEditorData>(); // null check
            var offColor = new Color(0.13f, 0.13f, 0.15f);

            
            for (var g = 0; g < groups.Count; g++) {
                var group = groups[g];
                var boxData = Preferences.GetBoxData(groups[g].BoxDataGuid);

                if (g == groupEditorData.Count) groupEditorData.Add(new GroupEditorData()); //Sync with group.
                

                var groupColor = ActiveGroupIndex == g ? boxData.color : boxData.color * Color.gray;

                var editorData = groupEditorData[g];
                
                var xPos = rowLayout.xMin;
                var yPos = g == 0 ? rowLayout.yMax : groupEditorData[g - 1].bottomLine.yMax;
                
                editorData.bodyRect = new Rect(xPos, yPos, width, height);
                editorData.settingsRect = new Rect(xPos, yPos, height, height);
                editorData.bottomLine = new Rect(xPos, yPos + height, columnRects[^1].xMax - xPos,
                    bottomLineHeight);
                var labelRect = new Rect(editorData.settingsRect.xMax + 5,
                    editorData.settingsRect.yMin + bottomLineHeight * 0.5f, width, 30);

                //Shortcut.
                var bodyRect = editorData.bodyRect;
                var settingsRect = editorData.settingsRect;
                var bottomLine = editorData.bottomLine;
 
                EditorGUI.DrawRect(bodyRect, offColor);
                EditorGUI.DrawRect(settingsRect, groupColor);
                
                EditorGUI.DrawRect(new Rect(settingsRect.xMax, settingsRect.y, 2, settingsRect.height), Color.black);


                var tempColor = GUI.color;
                GUI.color = ActiveGroupIndex == g
                    ? new Color(0.75f, 0.75f, 0.75f)
                    : new Color(0.39f, 0.43f, 0.47f); // Changing GUI color for the text color.
                EditorGUI.LabelField(labelRect, boxData.boxType);
                GUI.color = tempColor;


                if (group.isExpanded) {
                    while(editorData.layerRects.Count < groups[g].layers.Count){
                        editorData.layerRects.Add(new Rect());
                    }
                    while(editorData.layerRects.Count > groups[g].layers.Count){
                        editorData.layerRects.RemoveAt(editorData.layerRects.Count-1);
                    }
                    var layerYPos = bodyRect.yMax + (height * editorData.layerRects.Count);
                    var layerRect = new Rect(xPos, layerYPos, width, height);
                    for(var l = 0; l < editorData.layerRects.Count; l++){
                        layerYPos = bodyRect.yMax + (height * l);
                        layerRect.y = layerYPos;
                        editorData.layerRects[l] = layerRect;
                    } 
                    
                    if(editorData.layerRects.Count > 0){
                        bottomLine.y = editorData.layerRects[^1].yMax;
                        editorData.bottomLine = bottomLine;
                    }
                    DrawLayer(editorData);
                }
                EditorGUI.DrawRect(bottomLine, Color.black);


                while(editorData.layerRects.Count > groups[g].layers.Count){
                    editorData.layerRects.RemoveAt(editorData.layerRects.Count-1);
                }
            }
        }

        private void DrawLayer(GroupEditorData editorData){
            
            const float square = 25;
            var groupIndex = groupEditorData.IndexOf(editorData);
            for(var i = 0; i < editorData.layerRects.Count; i++){
                var layerRect = editorData.layerRects[i];
                var burgerMenu = new Rect(layerRect.xMin + 5, layerRect.yMin + 7, square, square);
                var color = new Color(0.08f, 0.08f, 0.10f);
                EditorGUI.DrawRect(layerRect, color);


                var labelRect = new Rect(burgerMenu.xMax + 20, layerRect.y + layerRect.height/2 -5, 300, 10);
                var tempColor = GUI.color;
                
                GUI.color = ActiveGroupIndex == groupIndex && ActiveLayerIndex == i ?
                new Color(0.75f, 0.75f, 0.75f) : 
                new Color(0.39f, 0.43f, 0.47f); // Changing GUI color for the text color.
                
                EditorGUI.LabelField(labelRect, "HitBox " + (i + 1));
                GUI.color = tempColor;
                
                CreateLayerButtons(burgerMenu);
            }
            
        }

        private void CreateLayerButtons(Rect burgerMenu){
            var layerCount = SelectedAnim.Groups[ActiveGroupIndex].layers.Count;
            for(var i = 0; i < layerCount; i++ ){
                
                if(PixelAnimatorUtility.Button(layerBurgerMenu, burgerMenu)){
                    ActiveLayerIndex = i;
                    layerPopup.ShowAsContext();
                } 
            }

        }

        private void CreateGroupButtons(){

            var evtCurrent = Event.current;
            var groups = SelectedAnim.Groups;
            if(isPlaying) return;
            DrawGroup(groups);
            for(var g = 0; g < groups.Count; g ++){
                var gRect = groupEditorData[g];
                var group = groups[g];
                
                if (LeftClicked) {

                    if(groupEditorData[g].bodyRect.Contains(evtCurrent.mousePosition) && g != ActiveGroupIndex){
                        ActiveGroupIndex = g;
                        while (ActiveLayerIndex > 0 &&
                               ActiveLayerIndex >= groups[ActiveGroupIndex].layers.Count) {
                            ActiveLayerIndex--;
                        }
                    } 
                    
                    for(var l = 0; l < groupEditorData[g].layerRects.Count; l++){
                        if(groupEditorData[g].layerRects[l].Contains(evtCurrent.mousePosition)){
                            if(g != ActiveGroupIndex){
                                ActiveGroupIndex = g;
                                ActiveLayerIndex = l;
                            }
                            else ActiveLayerIndex = l;
                        
                        }
                    }
                    
                }
                
                if(PixelAnimatorUtility.Button(settingsTex, gRect.settingsRect)){  // Opening settings popup
                    settingsPopup.ShowAsContext(); 
                    ActiveGroupIndex = g;
                }
                else if(gRect.bodyRect.Contains(evtCurrent.mousePosition) && RightClicked){  
                    settingsPopup.ShowAsContext();
                    ActiveGroupIndex = g;
                }
                var foldoutTex = group.isExpanded ? openTex : closeTex;
                if (PixelAnimatorUtility.Button(foldoutTex, gRect.FoldoutRect)) {
                    group.isExpanded = !group.isExpanded;
                }

                var isVisibleTex = group.isVisible ? visibleTex : unvisibleTex;
                if (PixelAnimatorUtility.Button(isVisibleTex, gRect.VisibleRect)) {
                    group.isVisible = !group.isVisible;
                }
                
                var isTriggerTex = group.colliderTypes == ColliderTypes.NoTrigger ? noTriggerTex : triggerTex;
                if(PixelAnimatorUtility.Button(isTriggerTex, gRect.ColliderTypeRect)){
                    group.colliderTypes = (int)group.colliderTypes == 1 ?(ColliderTypes) 2 :(ColliderTypes) 1;
                }


            }
        }

        private void SetGroupMenu(){
            settingsPopup = new GenericMenu{ allowDuplicateNames = true };
            settingsPopup.AddItem(new GUIContent("Settings/Delete"), false, DeleteGroup);
        }

        private void SetLayerMenu(){
            layerPopup = new GenericMenu{allowDuplicateNames = true};
            layerPopup.AddItem(new GUIContent("Add Layer Below"), false, AddBelowLayer);
            layerPopup.AddItem(new GUIContent("Delete Layer"), false, DeleteLayer);
        }
        

        private void SetFrameRect(){
            if (groupEditorData.Count != SelectedAnim.Groups.Count) return;
            var evtCurr = Event.current;
            var groups = SelectedAnim.Groups;
            const int size = 16;

            var columnWidth = columnRects[0].xMin - columnLayout.xMax; //Size of Fields of frames 
            var columnHeight =  groupEditorData[0].bodyRect.yMax - rowLayout.yMax;

            //maybe I'll use selectedFrameRect or use selected frame rect as a parameter
            for (var g = 0; g < groups.Count; g++) {

                var group = SelectedAnim.Groups[g];
                var editorData = groupEditorData[g];
                if(!group.isExpanded) continue;
                if (group.layers.Count != editorData.layerRects.Count) return;
                for (var l = 0; l < group.layers.Count; l++) {
                    var layer = group.layers[l];
                    var layerRect = editorData.layerRects[l];
                    var frames = layer.frames;

                    var yFramePos = layerRect.yMin + columnHeight/2 - size/2;
                    SetFrameVariable(frames);

                    for(var f = 0; f < frames.Count; f++) {
                        var frame = layer.frames[f];
                        var column = columnRects[f];
                        var cell = new Rect(column.xMin - columnWidth, layerRect.yMin, columnWidth, columnHeight);//frame cell

                        var xFramePos = cell.xMin + columnWidth/2 - size/2;
                        var frameRect = new Rect(xFramePos , yFramePos, size, size);

                        switch (frame.frameType) {
                            case FrameType.KeyFrame:
                                GUI.DrawTexture(frameRect, keyFrameTex);
                                break;
                            case FrameType.CopyFrame:
                                GUI.DrawTexture(frameRect, singleFrameTex);
                                break;
                            case FrameType.EmptyFrame:
                                GUI.DrawTexture(frameRect, emptyFrameTex);
                            break;
                        }

                        if (f != 0) {
                            var prevFrameRect = new Rect(columnRects[f - 1].xMin - columnWidth / 2 - size / 2, yFramePos, size, size);
                            SetFrameType(frameRect, layer.frames, f, prevFrameRect,evtCurr);
                        }else if(layer.frames.Count > 1){
                            if(frameRect.Contains(evtCurr.mousePosition) && LeftClicked){
                                frame.frameType = frame.frameType == FrameType.KeyFrame ? FrameType.EmptyFrame : FrameType.KeyFrame;
                                if(frame.frameType == FrameType.EmptyFrame  && layer.frames[f+1].frameType == FrameType.CopyFrame)
                                    layer.frames[f+1].frameType = FrameType.KeyFrame;
                            }
                        }
                        
                        if(cell.Contains(evtCurr.mousePosition) && LeftClicked ){
                            ActiveGroupIndex = g;
                            ActiveLayerIndex = l;
                            ActiveFrameIndex = f;
                            PropertyFocus = PropertyFocusEnum.HitBox;
                        }

                    }
                }
                
            }
        }

        private void SetFrameType(Rect frameRect, IReadOnlyList<Frame> frames, int frameIndex, Rect prevRect, Event evtCurr ){
            
            var frame = frames[frameIndex];
            var prevIndex = frameIndex-1;
            
            if (frame.frameType == FrameType.CopyFrame && frames[prevIndex].frameType != FrameType.EmptyFrame) {
                var linkedRect = new Rect(prevRect.xMax - 5, prevRect.yMin, frameRect.xMin - prevRect.xMax+10 , frameRect.height);
                GUI.DrawTexture(linkedRect, linkedFrameTex);
            }

            if (frameRect.Contains(evtCurr.mousePosition) && LeftClicked) {
                if(ActiveFrameIndex != frameIndex){
                    ActiveFrameIndex = frameIndex;
                    return;
                }
                
                switch (frame.frameType) {
                    case FrameType.KeyFrame :
                        if(frames[frameIndex-1].frameType != FrameType.EmptyFrame){
                            frames[frameIndex].frameType = FrameType.CopyFrame;
                            for(var i = frameIndex-1; i > 0; i--){
                                if(frames[i].frameType == FrameType.KeyFrame){
                                    break;
                                }
                                frames[i].frameType = FrameType.CopyFrame;
                            }
                        }else{
                            frame.frameType = FrameType.EmptyFrame;
                        }

                        break;
                    case FrameType.CopyFrame:
                        frame.frameType = FrameType.EmptyFrame;
                        
                        if(frameIndex != frames.Count-1){
                            
                            if(frames[frameIndex+1].frameType == FrameType.CopyFrame)
                                frames[frameIndex+1].frameType = FrameType.KeyFrame; //(If exist)The copy frame on the left is converted to the key frame
                            else{
                                for(var i = frameIndex-1; i>0; i--){
                                    if(frames[i].frameType != FrameType.KeyFrame){
                                        frames[i].frameType = FrameType.EmptyFrame;
                                    }else{
                                        break;
                                    }
                                }           
                            }
                        }
                        break;
                    case FrameType.EmptyFrame:
                        frame.frameType = FrameType.KeyFrame;
                        break;

                }
            
            
            }
    
        }

        private void SetFrameVariable(List<Frame> frames){
            var frame = frames[ActiveFrameIndex];

            for(var i = ActiveFrameIndex; i >= 0; i--){
                frames[i].hitBoxRect = frame.hitBoxRect;
                
                if(frames[i].frameType != FrameType.CopyFrame) break;
                
            }
            for(var i = ActiveFrameIndex+1; i < frames.Count; i++){
                if(frames[i].frameType != FrameType.CopyFrame) break;

                frames[i].hitBoxRect = frame.hitBoxRect;
            }
        }
        
        //Drawing the selected frame or sprite corner.
        private void DrawSelectedFrame(){
            int size;

            var topLeft = new Rect();
            var topRight = new Rect();
            var bottomLeft = new Rect();
            var bottomRight = new Rect();
            if (columnRects.Length != SelectedAnim.PixelSprites.Count) columnRects = new Rect[SelectedAnim.PixelSprites.Count];
            var column = columnRects[ActiveFrameIndex];
            

            switch (PropertyFocus) {
                case PropertyFocusEnum.HitBox:
                    if(SelectedAnim.Groups.Count <= 0) return;
                    if (!SelectedAnim.Groups[ActiveGroupIndex].isExpanded) return;
                    if (SelectedAnim.Groups[ActiveGroupIndex].layers.Count <= 0) break;
                    if(groupEditorData.Count-1 < ActiveGroupIndex) break;
                    if (groupEditorData[ActiveGroupIndex].layerRects.Count - 1 < ActiveLayerIndex) return;
                    var layerRect = groupEditorData[ActiveGroupIndex].layerRects[ActiveLayerIndex];
                    size = 12;
                    topLeft = new Rect(column.xMin - biggerSpriteSize.x, layerRect.yMin, size, size);
                    topRight = new Rect(column.xMin - size, layerRect.yMin, size, size);
                    bottomLeft = new Rect(column.xMin - biggerSpriteSize.x, layerRect.yMax - size, size,
                        size);
                    bottomRight = new Rect(column.xMin - size, layerRect.yMax - size, size, size);

                    break;
                case PropertyFocusEnum.Sprite:
                    size = 10;
                    var leftX = column.x - biggerSpriteSize.x;
                    var rightX = column.x - size;
                    topLeft = new Rect(leftX, column.yMin, size, size);
                    topRight = new Rect(rightX, column.yMin, size, size);
                    bottomLeft = new Rect(leftX, rowLayout.y - size, size, size);
                    bottomRight = new Rect(rightX, rowLayout.y - size, size, size);
                    break;
            }

            GUI.DrawTexture(topLeft, Resources.Load<Texture2D>("Sprites/Top Left"));
            GUI.DrawTexture(topRight, Resources.Load<Texture2D>("Sprites/Top Right"));
            GUI.DrawTexture(bottomLeft, Resources.Load<Texture2D>("Sprites/Bottom Left"));
            GUI.DrawTexture(bottomRight, Resources.Load<Texture2D>("Sprites/Bottom Right"));

        }

        

        private void SetFrameCopyPaste(){
            if (WindowFocus != WindowEnum.Timeline || PropertyFocus != PropertyFocusEnum.HitBox) return;
            var eventCurrent = Event.current;
            if (eventCurrent.type == EventType.ValidateCommand && eventCurrent.commandName == "Copy")
                eventCurrent.Use();
            
            switch (eventCurrent.type) {
                case EventType.ExecuteCommand when eventCurrent.commandName == "Copy":
                    // EditorGUIUtility.systemCopyBuffer =
                        // JsonUtility.ToJson(selectedAnimation.Groups[activeGroupIndex].layers[activeLayerIndex].frames[activeFrameIndex]);
                    break;
                case EventType.ValidateCommand when eventCurrent.commandName == "Paste":
                    eventCurrent.Use();
                    break;
            }


            if (eventCurrent.type == EventType.ExecuteCommand && eventCurrent.commandName == "Paste") {
                var copiedFrame = JsonUtility.FromJson<Frame>(EditorGUIUtility.systemCopyBuffer);
                
                var frameProp = TargetAnimation.FindProperty("layers").GetArrayElementAtIndex(ActiveGroupIndex)
                    .FindPropertyRelative("frames").GetArrayElementAtIndex(ActiveFrameIndex);
                
                var hitBoxRectProp = frameProp.FindPropertyRelative("hitBoxRect");
                // var colliderType = frameProp.FindPropertyRelative("colliderType");

                // colliderType.enumValueIndex = (int)copiedFrame.colliderType;
                hitBoxRectProp.rectValue = copiedFrame.hitBoxRect;
                TargetAnimation.ApplyModifiedProperties();

            }
        }

        #region Popup Functions
        private void AddGroup(object userData){
            
            TargetAnimation.Update();
            var data = (BoxData)userData;

            if (SelectedAnim.Groups.Any(x => x.BoxDataGuid == data.Guid)) {
                Debug.LogError("This boxData has already been added! Please add another boxData.");
                return;
            }

            SelectedAnim.AddGroup(data.Guid);
            SelectedAnim.Groups[^1].AddLayer(SelectedAnim.PixelSprites);
            CheckAndFixVariable();

        }

        private void TargetPreferences(){
            EditorGUIUtility.PingObject(Preferences);
            Selection.activeObject = Preferences;
        }

        private void UpdateAnimation(){
            // foreach(var group in selectedAnim.Groups) {
            //     var spriteCount = selectedAnim.PixelSprites.Count;
            //     for (var f = 0; f < spriteCount; f++) {
            //         
            //         foreach (var layer in group.layers) {
            //             var frame = layer.frames[f];
            //             
            //         }
            //     }
            //     
            //     
            //     
            //     foreach (var layer in group.layers) {
            //         for (var f = 0; f < layer.frames.Count; f++) {
            //             var frame = layer.frames[f];
            //             if (frame.frameType != FrameType.KeyFrame) {
            //                 frame.hitBoxData = new PropertyData();
            //                 continue;
            //             }
            //
            //             var hitBoxData = frame.HitBoxData;
            //             for (var i = 0; i < hitBoxData.genericData.Count; i++) {
            //                 var propertyValue = hitBoxData.genericData[i];
            //                 var myGuid = propertyValue.baseData.Guid;
            //                 var exist = preferences.HitBoxProperties.Any(x => x.Guid == myGuid);
            //
            //                 if (exist) continue;
            //                 hitBoxData.genericData.Remove(propertyValue);
            //                 i = -1;
            //             }
            //         }
            //     }
            // }

        }
        
        
        private void DeleteGroup(){
            TargetAnimation.Update();
            TargetAnimation.FindProperty("groups").DeleteArrayElementAtIndex(ActiveGroupIndex);
            TargetAnimation.ApplyModifiedProperties();
            CheckAndFixVariable();
        }

        private void DeleteLayer(){
            var propGroups = TargetAnimation.FindProperty("groups");
            var propLayers = propGroups.GetArrayElementAtIndex(ActiveGroupIndex).FindPropertyRelative("layers");
            propLayers.DeleteArrayElementAtIndex(ActiveLayerIndex);
            TargetAnimation.ApplyModifiedProperties();
            CheckAndFixVariable();
        }

        private void AddBelowLayer(){
            var groupsProp = TargetAnimation.FindProperty("groups");
            var layersProp = groupsProp.GetArrayElementAtIndex(ActiveGroupIndex).FindPropertyRelative("layers");
            PixelAnimationEditor.AddLayer(layersProp, TargetAnimation);
            CheckAndFixVariable();
        }
        #endregion
        
        
        #endregion

        #region Common
        // private Vector2 GetBiggerSpriteSize(){
        //     var greatestX = selectedAnim.GetSpriteList().Aggregate((current, next) => 
        //         current.rect.size.x > next.rect.size.x ? current : next).rect.size.x / 1.5f;
        //     
        //     var greatestY = selectedAnim.GetSpriteList().Aggregate((current, next) => 
        //         current.rect.size.y > next.rect.size.y ? current : next).rect.size.y / 1.5f;
        //
        //
        //     return new Vector2(greatestX, greatestY);
        // }

        private void Play(){
            timer += editorDeltaTime * SelectedAnim.fps;
            if(timer >= 1f){
                timer -= 1f;
                ActiveFrameIndex = (ActiveFrameIndex + 1) % SelectedAnim.GetSpriteList().Count;
            }
            Repaint();
        }

        private void SetEditorDeltaTime(){

            if(lastTimeSinceStartup == 0f){
                lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
            }
            
            editorDeltaTime = (float)(EditorApplication.timeSinceStartup - lastTimeSinceStartup);
            lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
            
            
        }

        private void SelectedObject(){
            foreach(var obj in Selection.objects) {
                if (obj is not PixelAnimation anim) continue;
                TargetAnimation = new SerializedObject(anim);

                if (SelectedAnim == anim) continue;
                var spriteList = anim.GetSpriteList();
                if(spriteList != null)
                TimelineWindow?.ReloadVariable(spriteList.Count);

                

                SelectedAnim = anim;
                lastTimeSinceStartup = 0;
                ActiveFrameIndex = 0;
                ActiveGroupIndex = 0; 
                ActiveLayerIndex = 0;
                timer = 0;
                groupEditorData = new List<GroupEditorData>();
                CheckAndFixVariable();
                if(spriteList?.Count == 0) return;
                spritePreview = AssetPreview.GetAssetPreview(spriteList?[ActiveFrameIndex]);
            }

        }

        private void CheckAndFixVariable(){
            if(SelectedAnim == null || SelectedAnim.Groups == null) return;
            if (SelectedAnim.Groups != null && ActiveGroupIndex >= SelectedAnim.Groups.Count || ActiveGroupIndex < 0) {
                ActiveGroupIndex = 0;    
            }
            
            if (ActiveFrameIndex >= SelectedAnim.GetSpriteList().Count || ActiveFrameIndex < 0) {
                ActiveFrameIndex = 0;
            }
            if(SelectedAnim.Groups.Count <= 0) return;
            if(ActiveLayerIndex >= SelectedAnim.Groups[ActiveGroupIndex].layers.Count){
                ActiveLayerIndex = 0;
            }
        }

            
        #endregion

        #region Keys
        private void SetPlayKeys(){
            var evtCurr = Event.current;

            if(evtCurr.isKey && evtCurr.type == EventType.KeyDown){
                var keyCode = evtCurr.keyCode;
                var spriteCount = SelectedAnim.GetSpriteList().Count;
                switch (keyCode) {
                    case KeyCode.Return:
                        isPlaying = !isPlaying;
                        evtCurr.Use();
                        break;
                    case KeyCode.RightArrow:
                        ActiveFrameIndex = (ActiveFrameIndex + 1) % spriteCount;
                        evtCurr.Use();
                        break;
                    case KeyCode.LeftArrow when ActiveFrameIndex != 0:
                        ActiveFrameIndex--;
                        evtCurr.Use();
                        break;
                    case KeyCode.LeftArrow when ActiveFrameIndex == 0:
                        ActiveFrameIndex = spriteCount - 1;
                        evtCurr.Use();
                        break;
                }
            }
        }

        private void SetGroupKeys(){
            if (SelectedAnim == null || SelectedAnim.Groups == null) return;
            if(SelectedAnim.Groups.Count == 0 ) return;
            var evtCurr = Event.current;
            var keyCode = evtCurr.keyCode;

            if(evtCurr.isKey && evtCurr.type == EventType.KeyDown){
                var groups = SelectedAnim.Groups;
                var group = groups[ActiveGroupIndex];
                
                switch (keyCode) {
                    case KeyCode.UpArrow when ActiveGroupIndex == 0 && ActiveLayerIndex == 0:
                        ActiveGroupIndex = groups.Count - 1;
                        ActiveLayerIndex = groups[^1].layers.Count-1;
                        evtCurr.Use();
                        PropertyFocus = PropertyFocusEnum.HitBox;
                        break;
                    case KeyCode.UpArrow:
                        if(ActiveLayerIndex == 0){
                            ActiveGroupIndex--;
                            ActiveLayerIndex = groups[ActiveGroupIndex].layers.Count-1;
                        }else{
                            ActiveLayerIndex--;
                        }
                        evtCurr.Use();
                        PropertyFocus = PropertyFocusEnum.HitBox;
                        break;
                    case KeyCode.DownArrow when groups.Count > 0:
                        if(ActiveLayerIndex == group.layers.Count-1){
                            ActiveGroupIndex = (ActiveGroupIndex+1) % groups.Count;
                            ActiveLayerIndex = 0;
                        }else{
                            ActiveLayerIndex++;
                        }

                        evtCurr.Use();
                        PropertyFocus = PropertyFocusEnum.HitBox;
                        break;
                }

            }
        }
        
        
        #endregion


        private void ZeroSpriteCount(){
            var eventCurrent = Event.current;
            var dropArea = new Rect(position.width / 3, (position.height - timelineRect.height) / 3,
                position.width / 3, position.height / 3);
            
            GUI.Window(
                7,
                dropArea,
                _ => {
                    EditorGUI.DrawRect(new Rect(Vector2.zero, dropArea.size), new Color(0.1f, 0.1f, 0.1f));
                    EditorGUILayout.LabelField("Please sprite drag in this rect");
                    
                }  
                ,
                GUIContent.none
            );
            
            switch (eventCurrent.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (dropArea.Contains(eventCurrent.mousePosition)) {
                    
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    
                        if (eventCurrent.type == EventType.DragPerform) {
                            DragAndDrop.AcceptDrag ();
                    
                            foreach (var draggedObject in DragAndDrop.objectReferences) {
                                if(draggedObject is Sprite sprite){
                                    SelectedAnim.AddPixelSprite(sprite);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void NoAnimation(){
            var infoArea = new Rect(position.width / 3, (position.height - timelineRect.height) / 3,
                position.width / 3, position.height / 3);
            
            GUI.Window(
                8,
                infoArea,
                _ => {
                    EditorGUI.DrawRect(new Rect(Vector2.zero, infoArea.size), new Color(0.1f, 0.1f, 0.1f));
                    EditorGUILayout.LabelField("Please select Pixel Animation or Create new Pixel Animation");
                    
                }  
                ,
                GUIContent.none
            );
        }

        public void SetActiveGroup(int index) {
            ActiveGroupIndex = index;
        }

        public void SetActiveLayer(int index) {
            ActiveLayerIndex = index;
        }

        public void SetActiveFrame(int index) {
            ActiveFrameIndex = index;
        }



        public void SetWindowFocus(Rect rect, WindowEnum windowFocus, bool include = true){
            if (windowFocus == WindowFocus) return;
            var focusedWindow = GetWindowForEnum(WindowFocus);
            if (focusedWindow == null || !focusedWindow.IsFocusChangeable) return; 

            var anyMouseOperations = LeftClicked || RightClicked || MiddleClicked;
            if (!anyMouseOperations) return;

            var isFocusable = include ? rect.Contains(Event.current.mousePosition) : !rect.Contains(Event.current.mousePosition);
            if (isFocusable) {
                Debug.Log("Focused the " + windowFocus.ToString());
                focusedWindow.IsFocusChangeable = false;
                WindowFocus = windowFocus;
            }



        }

        public CustomWindow GetWindowForEnum(WindowEnum window) {
            switch (window) {
                case WindowEnum.Property:
                    return null;
                    
                case WindowEnum.Timeline:
                    return TimelineWindow;

                case WindowEnum.Canvas:
                    return CanvasWindow;
                    
            }
            return null;
        }

        public bool IsMouseOnWindow(WindowEnum windowEnum) {
            var mousePos = Event.current.mousePosition;
            var window = GetWindowForEnum(windowEnum);
            var onWindow = window.WindowRect.Contains(mousePos);
            switch (windowEnum) {
                case WindowEnum.Property or WindowEnum.Timeline:
                    return onWindow;
                case WindowEnum.Canvas:
                    return onWindow || !(TimelineWindow.WindowRect.Contains(mousePos) || propertyWindowRect.Contains(mousePos));
            }

            return false;
        }



    }



}

    


