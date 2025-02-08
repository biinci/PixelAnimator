using binc.PixelAnimator.DataManipulations;
using UnityEditor;
using UnityEngine;

namespace binc.PixelAnimator.Editor
{
    [CustomPropertyDrawer(typeof(BaseData))]
    public class BaseDataDrawer : PropertyDrawer
    {
        private const string NoDataText = "<color=#FFC107><u>Data not shown</u></color>";
        private const string NoDataTip = "This data probably is not serializable";
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
                var tempAlignment = GUI.skin.box.alignment;
                var tempRichText = GUI.skin.box.richText;
                GUI.skin.box.alignment = TextAnchor.MiddleCenter;
                GUI.skin.box.richText = true;
                var content = new GUIContent(NoDataText, NoDataTip);
                EditorGUI.LabelField(dataRect, content, GUI.skin.box);
                GUI.skin.box.alignment = tempAlignment;
                GUI.skin.box.richText = tempRichText;
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
