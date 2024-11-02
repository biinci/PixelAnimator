using System.Collections;
using System.Collections.Generic;
using binc.PixelAnimator.DataProvider;
using binc.PixelAnimator.Utility;
using UnityEditor;
using UnityEngine;

namespace binc.PixelAnimator.Editor.Windows
{
    public partial class PropertyWindow
    {
        
        private void SetPropertyField(BasicPropertyData propData, SerializedProperty propertyValues,
            BaseData baseData, int baseDataIndex)
        {
            propertyValues.serializedObject.Update();
            var alreadyExist = baseData != null;


            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(propData.Name, GUILayout.MaxWidth(70));


                if (alreadyExist)
                {
                    var propertyData = propertyValues.GetArrayElementAtIndex(baseDataIndex)
                        .FindPropertyRelative("baseData")
                        .FindPropertyRelative("data");
                    EditorGUILayout.PropertyField(propertyData, GUIContent.none, GUILayout.Width(90));
                    propertyValues.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    PixelAnimatorUtility.SystemObjectPreviewField(
                        PixelAnimatorUtility.DataTypeToSystemObject(propData.dataType), GUILayout.Width(90)
                        );
                }


                GUILayout.Space(10);
                if (GUILayout.Button("X", GUILayout.MaxWidth(15), GUILayout.MaxHeight(15)))
                {
                    // Drawing added or remove button.
                    if (alreadyExist)
                    {
                        propertyValues.DeleteArrayElementAtIndex(baseDataIndex);
                    }
                    else
                    {
                        PixelAnimatorUtility.AddPropertyValue(propertyValues, propData);
                    }

                    propertyValues.serializedObject.ApplyModifiedProperties();


                }
            }

        }
        
        private void AddEvent(SerializedProperty eventNames)
        {
            eventNames.arraySize++;
            eventNames.serializedObject.ApplyModifiedProperties();
            
        }
        

        private void RemoveEvent((SerializedProperty, int) tuple)
        {
            tuple.Item1.DeleteArrayElementAtIndex(tuple.Item2);
            tuple.Item1.serializedObject.ApplyModifiedProperties();
        }

        public override void FocusFunctions()
        {
            DrawFocusOutline();
        }
    }
}