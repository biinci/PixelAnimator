using UnityEditor;
using UnityEngine;
using binc.PixelAnimator.DataProvider;

namespace  binc.PixelAnimator.Editor.DataProvider{
    [CustomPropertyDrawer(typeof(BaseData))]
    public class BaseDataPropertyDrawer : PropertyDrawer{
        public static float extraMultiply = 1f;
            
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Keyboard), GUIContent.none);
            var mData = property.FindPropertyRelative("data");
            var mGuid = property.FindPropertyRelative("guid");
                
            var widthSize = position.width / 3 * extraMultiply;


            var dataLabel = new Rect(position.x, position.y, 30, position.height);
            var dataRect = new Rect(position.x + dataLabel.width, position.y, widthSize * 2, position.height );

            var guidLabel = new Rect(dataRect.x + dataRect.width + 10, position.y, 30, position.height);
            var guidRect = new Rect(guidLabel.x + guidLabel.width, position.y, widthSize * 2, position.height);
                
            EditorGUI.LabelField(dataLabel, "Data :");
            EditorGUI.PropertyField(dataRect, mData, GUIContent.none);
            EditorGUI.LabelField(guidLabel, "Guid :");
            EditorGUI.PropertyField(guidRect, mGuid, GUIContent.none);
            property.serializedObject.ApplyModifiedProperties();
        }

    }
}
