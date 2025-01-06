using binc.PixelAnimator.Preferences;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using binc.PixelAnimator.Utility;


namespace binc.PixelAnimator.Editor.Preferences{

    [CustomEditor(typeof(PixelAnimationPreferences))]
    
    public class PixelAnimationPreferencesEditor : UnityEditor.Editor{

        #region Variables

        private SerializedObject editorSerializedObject;
        private SerializedProperty serializedBoxData;
        
        private ReorderableList boxDataList;

        #endregion

        private void OnEnable() {     
        
            serializedBoxData = serializedObject.FindProperty("boxData");

            InitGroupList();
        }
        

        public override void OnInspectorGUI(){
            serializedObject.Update();
            boxDataList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        #region BoxData

        private void InitGroupList(){
            boxDataList = new ReorderableList(serializedObject, serializedBoxData,true, true, true, true){
                drawElementCallback = DrawGroups,
                elementHeight = EditorGUIUtility.singleLineHeight,
                drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Box Data");},
          
                onAddCallback = _ => AddGroupElement() 
            };
            

        }

        private void AddGroupElement(){
            var index = boxDataList.serializedProperty.arraySize;
            boxDataList.serializedProperty.arraySize ++;
            boxDataList.index = index;
            var element = boxDataList.serializedProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("guid").stringValue = GUID.Generate().ToString();
            element.serializedObject.ApplyModifiedProperties();
        }

        
        private void DrawGroups(Rect rect, int index, bool isActive, bool isFocused){
            var element = boxDataList.serializedProperty.GetArrayElementAtIndex(index);
            var eventCurrent = Event.current;
            serializedObject.Update();

            rect.y += 2;


            var color = element.FindPropertyRelative("color");
            var boxType = element.FindPropertyRelative("boxType");
            var activeLayer = element.FindPropertyRelative("activeLayer");
            var physicMaterial2D = element.FindPropertyRelative("physicMaterial");
            var rounded = element.FindPropertyRelative("rounded");
            // var detection = element.FindPropertyRelative("colliderDetection");
            // var collisionLayer = element.FindPropertyRelative("collisionLayer");

            var colorRect = new Rect(rect.x, rect.y, 140, EditorGUIUtility.singleLineHeight);
            var boxTypeRect = new Rect(colorRect.xMax + 10, rect.y, 100, EditorGUIUtility.singleLineHeight);
            var activeLayerRect = new Rect(boxTypeRect.xMax + 10, rect.y, 100, EditorGUIUtility.singleLineHeight);
            var physicMaterial2DRect = new Rect(activeLayerRect.xMax + 10, rect.y, 100, EditorGUIUtility.singleLineHeight);
            var roundedRect = new Rect(physicMaterial2DRect.xMax + 10, rect.y, 10, EditorGUIUtility.singleLineHeight);
            // var detectionRect = new Rect(roundedRect.xMax + 10, rect.y, 100, EditorGUIUtility.singleLineHeight);
            // var collisionLayerRect = new Rect(detectionRect.xMax + 10, rect.y, 100, EditorGUIUtility.singleLineHeight);
            
            //Color
            EditorGUI.PropertyField(
                colorRect,
                color,
                GUIContent.none
                );

            //Box Type (Name)
            EditorGUI.PropertyField(
                    boxTypeRect,
                    boxType,
                    GUIContent.none
                );

            //ActiveLayer
            EditorGUI.PropertyField(
                activeLayerRect,
                activeLayer,
                GUIContent.none
                );

            //PhysicsMat2D
            EditorGUI.PropertyField(
                physicMaterial2DRect,
                physicMaterial2D,
                GUIContent.none
                );

            //Rounded
            EditorGUI.PropertyField(
                roundedRect,
                rounded,
                GUIContent.none
                );

            // EditorGUI.PropertyField(
            //     detectionRect,
            //     detection,
            //     GUIContent.none
            //     );
            //
            // EditorGUI.PropertyField(
            //     collisionLayerRect,
            //     collisionLayer,
            //     GUIContent.none
            // );

            

            //Setting Tool tips
            
            PixelAnimatorUtility.CreateTooltip(activeLayerRect, LayerMask.LayerToName(activeLayer.intValue),
                eventCurrent.mousePosition);
            
            PixelAnimatorUtility.CreateTooltip(roundedRect, "Rounded", eventCurrent.mousePosition);
            
            PixelAnimatorUtility.CreateTooltip(boxTypeRect, "Name", eventCurrent.mousePosition);
            
            element.serializedObject.ApplyModifiedProperties();


        }
        
        #endregion


    }
    
}



