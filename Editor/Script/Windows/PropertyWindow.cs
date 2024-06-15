using System;
using System.Linq;
using binc.PixelAnimator.Common;
using binc.PixelAnimator.DataProvider;
using binc.PixelAnimator.Editor.Windows;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.Utility;
using UnityEditor;
using UnityEngine;

public class PropertyWindow : Window{

    private Vector2 propertyScrollPos = Vector2.one;
    private Rect propertyWindowRect = new(10, 6, 120, 20);
    private bool eventFoldout;

    // public PropertyWindow(WindowEnum windowFocusType) : 
    // base(windowFocusType){
    
    
    // }

    public override void SetWindow(Event eventCurrent){
        base.SetWindow(eventCurrent);

        DrawPropertyWindow();
    }

    public override void UIOperations(){
        
    }

    public override void FocusFunctions(){
    }






    private void DrawProperties(PropertyType propertyType, string header){
        var scrollScope = new EditorGUILayout.ScrollViewScope(propertyScrollPos);

        EditorGUI.LabelField(propertyWindowRect, header, EditorStyles.boldLabel); // Drawing header
        
        var rect = new Rect(7, 30, 300, 2f);
        var color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
        EditorGUI.DrawRect(rect, color); 

        GUILayout.Space(50);
        // propertyXScrollPos = xScroll.scrollPosition;

        using(scrollScope){
            propertyScrollPos = scrollScope.scrollPosition;
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

    private void DrawSpriteProp(){
        var targetAnimation = PixelAnimatorWindow.AnimatorWindow.TargetAnimation;
        targetAnimation.Update();
        var frameIndex = PixelAnimatorWindow.AnimatorWindow.ActiveFrameIndex;

        var propPixelSprite = targetAnimation.FindProperty("pixelSprites")
            .GetArrayElementAtIndex(frameIndex);

        var propSpriteData = propPixelSprite.FindPropertyRelative("spriteData");
        var propSpriteEventNames = propSpriteData.FindPropertyRelative("eventNames");
        var propSpriteDataValues = propSpriteData.FindPropertyRelative("genericData");
        var spriteDataValues = PixelAnimatorWindow.AnimatorWindow.SelectedAnimation.PixelSprites[frameIndex].SpriteData
            .genericData;

        foreach (var prop in PixelAnimatorWindow.AnimatorWindow.AnimationPreferences.SpriteProperties) {
            var single = spriteDataValues.FirstOrDefault(x => x.baseData.Guid == prop.Guid)
                .baseData;
            var selectedIndex = single == null
                ? -1
                : spriteDataValues.FindIndex(x => x.baseData == single);
            SetPropertyField(prop, propSpriteDataValues, single, selectedIndex);
        }

        DrawEventField(propSpriteEventNames);
        targetAnimation.ApplyModifiedProperties();
    }

    private void DrawHitBoxProp(){
        var groupIndex = PixelAnimatorWindow.AnimatorWindow.ActiveGroupIndex;
        var layerIndex = PixelAnimatorWindow.AnimatorWindow.ActiveLayerIndex;
        var frameIndex = PixelAnimatorWindow.AnimatorWindow.ActiveFrameIndex;
        
        var targetAnimation = PixelAnimatorWindow.AnimatorWindow.TargetAnimation;


        if (SelectedAnim.Groups[groupIndex].layers.Count <= 0) return;
        var group = targetAnimation.FindProperty("groups").GetArrayElementAtIndex(groupIndex);
        var layer = group.FindPropertyRelative("layers").GetArrayElementAtIndex(layerIndex);
        var frames = layer.FindPropertyRelative("frames");
        var frame = frames.GetArrayElementAtIndex(frameIndex);
        
        
        using (new GUILayout.HorizontalScope()) {
            EditorGUILayout.LabelField("Box", GUILayout.Width(70));
            var propHitBoxRect = frame.FindPropertyRelative("hitBoxRect");
            EditorGUILayout.PropertyField(propHitBoxRect, GUIContent.none, GUILayout.Width(140),
                GUILayout.MaxHeight(60));
        }
        
        targetAnimation.ApplyModifiedProperties();
        
        var dataProps = frame.FindPropertyRelative("hitBoxData");
        var eventNamesProp = dataProps.FindPropertyRelative("eventNames");
        var dataValuesProp = dataProps.FindPropertyRelative("genericData");

        var hitBoxDataValues = SelectedAnim.Groups[groupIndex].layers[layerIndex].frames[frameIndex].HitBoxData.genericData;
        foreach (var dataWareHouse in PixelAnimatorWindow.AnimatorWindow.AnimationPreferences.HitBoxProperties) {
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

        
        switch (PixelAnimatorWindow.AnimatorWindow.PropertyFocus) {
            case PropertyFocusEnum.HitBox:
                var selectedAnim = PixelAnimatorWindow.AnimatorWindow.SelectedAnimation;
                if (selectedAnim.Groups.Count == 0) {
                    // PixelAnimatorWindow.AnimatorWindow.SetPropertyFocus(PropertyFocusEnum.Sprite);
                    break;
                }
                // if(ActiveGroupIndex >= SelectedAnim.Groups.Count) CheckAndFixVariable();
                var groupIndex = PixelAnimatorWindow.AnimatorWindow.ActiveGroupIndex;
                var layerIndex = PixelAnimatorWindow.AnimatorWindow.ActiveLayerIndex;
                var frameIndex = PixelAnimatorWindow.AnimatorWindow.ActiveFrameIndex;
                if (selectedAnim.Groups[groupIndex].layers[layerIndex].frames[frameIndex]
                        .frameType != FrameType.KeyFrame) break;
                GUI.Window(4, windowRect,
                    _ => { DrawProperties(PropertyType.HitBox, "HitBox Properties"); }, GUIContent.none);
                break;
            case PropertyFocusEnum.Sprite:
                GUI.Window(4, windowRect,
                    _ => { DrawProperties(PropertyType.Sprite, "Sprite Properties"); }, GUIContent.none);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (propertyWindowRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown) {
            PixelAnimatorWindow.AnimatorWindow.WindowFocus = WindowEnum.Property;
        }

        GUI.color = tempColor;

    }
    
    private void DrawEventField(SerializedProperty eventNames){
        GUILayout.Space(20);
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




}

