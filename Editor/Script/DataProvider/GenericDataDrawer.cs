using binc.PixelAnimator.DataProvider;
using binc.PixelAnimator.Utility;
using UnityEditor;
using UnityEngine;

namespace binc.PixelAnimator.Editor.DataProvider{

    
    [CustomPropertyDrawer(typeof(GenericData))]
    public class GenericDataDrawer : PropertyDrawer{
        private SerializedProperty mType;
        private SerializedProperty mData;
        public static float extraMultiply = 1; 
        

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
            mType = property.FindPropertyRelative("dataType");
            mData = property.FindPropertyRelative("baseData");

            property.serializedObject.Update();

            var widthSize = position.width / 3 * extraMultiply - 10;
            
            var typePos = new Rect(position.x, position.y, widthSize, EditorGUIUtility.singleLineHeight);
            var dataPos = new Rect(typePos.xMax + 10, position.y, 80, 20);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(typePos, mType, GUIContent.none);
            if (EditorGUI.EndChangeCheck()) {
                mData.managedReferenceValue = PixelAnimatorUtility.CreateBlankBaseData((DataType)mType.intValue);
            }

            if (mData.managedReferenceValue != null) {
                EditorGUI.PropertyField(dataPos, mData, GUIContent.none);
            }
            else {
                mData.managedReferenceValue = PixelAnimatorUtility.CreateBlankBaseData(DataType.IntData);
            }
            property.serializedObject.ApplyModifiedProperties();
            
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label){
            return EditorGUIUtility.singleLineHeight;
        }
    }



}


