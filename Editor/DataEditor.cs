using binc.PixelAnimator.DataManipulations;
using UnityEditor;
using UnityEngine;

namespace binc.PixelAnimator.Editor
{
    [CustomPropertyDrawer(typeof(BaseData))]
    public class BaseDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
        
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Keyboard), GUIContent.none);
            var mData = property.FindPropertyRelative("data");
            var referenceValue = property.managedReferenceValue;
            if (referenceValue == null) return;
        
        
            var dataRect = new Rect(position.x, position.y, position.width, position.height );
            if (mData != null)
            {
                EditorGUI.PropertyField(dataRect, mData, label, true);
                property.serializedObject.ApplyModifiedProperties();

            }
            else
            {
                var linkStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleCenter,
                    richText = true,
                };
                EditorGUI.LabelField(dataRect, new GUIContent("<color=#ff5c57><u>No data found</u></color>", "Probably you tried to use System.Object."), linkStyle);
            
            }

        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 0f;
            height += property.isExpanded ? EditorGUIUtility.singleLineHeight : 0f;
            var dataProperty = property.FindPropertyRelative("data");
            if (dataProperty != null)
            {
                height += EditorGUI.GetPropertyHeight(dataProperty, GUIContent.none);
            }
            else
            {
                height = EditorGUIUtility.singleLineHeight;
            }

            return height;
        }
    }
}
