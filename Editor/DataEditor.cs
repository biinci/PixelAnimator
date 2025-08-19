using binc.PixelAnimator.DataManipulations;
using UnityEditor;
using UnityEngine;

namespace binc.PixelAnimator.Editor
{
    [CustomPropertyDrawer(typeof(BaseData))]
    public class BaseDataDrawer : PropertyDrawer
    {
        private const string NoDataText = "<color=#FFC107><u>Data not shown</u></color>";
        private const string NoDataTip = "This data probably is not serializable.";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            
            var mData = property.FindPropertyRelative("data");
            var referenceValue = property.managedReferenceValue;
            if (referenceValue == null) return;

            // EditorGUI.DrawRect(prefixRect, Color.red);
            // var dataRect = prefixRect;
            if (mData != null)
            {
                EditorGUI.PropertyField(position, mData, label, true);
                property.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                var tempRichText = GUI.skin.label.richText;
                GUI.skin.label.richText = true;
                label.text = "<color=#FFC107><u>" + label.text + "</u></color>";
                // Debug.Log(label.tooltip);
                // Debug.Log(property.tooltip);
                label.tooltip = NoDataTip + "\n" + label.tooltip;
                var prefixRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Keyboard), label, GUI.skin.label);
                GUI.skin.label.richText = tempRichText;
                
                // var content = new GUIContent(NoDataText, NoDataTip);
                // label.tooltip = NoDataTip;
                // EditorGUI.LabelField(dataRect, label);
                EditorGUI.LabelField(prefixRect,"", GUI.skin.textField);
                // var tempAlignment = GUI.skin.box.alignment;
                // var tempRichText = GUI.skin.box.richText;
                // GUI.skin.box.alignment = TextAnchor.MiddleCenter;
                // EditorGUI.LabelField(dataRect, content, GUI.skin.box);
                // GUI.skin.box.alignment = tempAlignment;
                // GUI.skin.box.richText = tempRichText;
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
