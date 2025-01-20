#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace binc.PixelAnimator{
    public class ReadOnlyAttribute : PropertyAttribute { }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
            
        }
    }
#endif
}
