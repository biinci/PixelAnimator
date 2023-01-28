using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using binc.PixelAnimator.DataProvider;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

#endif


namespace binc.PixelAnimator.Utility{

    
    public static class PixelAnimatorUtility{
        
#if UNITY_EDITOR

        public static bool Button(Texture2D image, Rect rect){
            var e = Event.current;
            GUI.DrawTexture(rect, image);
            if (!rect.Contains(e.mousePosition)) return false;
            EditorGUI.DrawRect(rect, new Color(255, 255, 255, 0.2f)); 
            return e.button == 0 && e.type == EventType.MouseDown;
        }

        public static bool Button(Rect rect, Color color){
            var e = Event.current;
            EditorGUI.DrawRect(rect, color);
            if(rect.Contains(e.mousePosition)){
                EditorGUI.DrawRect(rect, new Color(255, 255, 255, 0.2f)); 
                if(e.button == 0 && e.type == EventType.MouseDown){
                    return true;
                }
            }
            return false;
        }

        public static bool Button(Texture2D defaultImg, Texture2D onMouse, Rect rect){
            var e = Event.current;
            if(rect.Contains(e.mousePosition)){
                GUI.DrawTexture(rect, onMouse);
                EditorGUI.DrawRect(rect, new Color(255, 255, 255, 0.2f));

                if(e.button == 0 && e.type == EventType.MouseDown){
                    return true;
                }
            }else{
                GUI.DrawTexture(rect, defaultImg);

            }
            return false;
        }


        public static void CreateTooltip(Rect rect, string tooltip, Vector2 containsPosition){
            if (rect.Contains(containsPosition)) {
                EditorGUI.LabelField(rect,
                    new GUIContent("", tooltip));
            }
        }
        

        public static void SystemObjectPreviewField(object obj, params GUILayoutOption[] guiLayoutOptions){
            using (new EditorGUI.DisabledGroupScope(true)) {
                switch (obj) {
                    case int data:
                        EditorGUILayout.IntField(data, guiLayoutOptions);
                        break;
                    case string data:
                        EditorGUILayout.TextField(data, guiLayoutOptions);
                        break;
                    case bool data:
                        EditorGUILayout.Toggle(data, guiLayoutOptions);
                        break;
                    case float data:
                        EditorGUILayout.FloatField(data, guiLayoutOptions);
                        break;
                    case long data:
                        EditorGUILayout.LongField(data, guiLayoutOptions);
                        break;
                    case double data:
                        EditorGUILayout.DoubleField(data, guiLayoutOptions);
                        break;
                    case Rect data:
                        EditorGUILayout.RectField(data, guiLayoutOptions);
                        break;
                    case RectInt data:
                        EditorGUILayout.RectIntField(data, guiLayoutOptions);
                        break;
                    case Color data:
                        EditorGUILayout.ColorField(data, guiLayoutOptions);
                        break;
                    case AnimationCurve data:
                        EditorGUILayout.CurveField(data, guiLayoutOptions);
                        break;
                    case Bounds data:
                        EditorGUILayout.BoundsField(data, guiLayoutOptions);
                        break;
                    case BoundsInt data:
                        EditorGUILayout.BoundsIntField(data, guiLayoutOptions);
                        break;
                    case Vector2 data:
                        EditorGUILayout.Vector2Field(GUIContent.none, data, guiLayoutOptions);
                        break;
                    case Vector3 data:
                        EditorGUILayout.Vector3Field(GUIContent.none, data, guiLayoutOptions);
                        break;
                    case Vector4 data:
                        EditorGUILayout.Vector4Field(GUIContent.none, data, guiLayoutOptions);
                        break;
                    case Vector2Int data:
                        EditorGUILayout.Vector2IntField(GUIContent.none, data, guiLayoutOptions);
                        break;
                    case Vector3Int data:
                        EditorGUILayout.Vector3IntField(GUIContent.none, data, guiLayoutOptions);
                        break;
                    case UnityEngine.Object data:
                        EditorGUILayout.ObjectField(data, obj.GetType(), false, guiLayoutOptions);
                        break;
                    case Gradient data:
                        EditorGUILayout.GradientField(data, guiLayoutOptions);
                        break;
                    default:
                        Debug.LogWarning("The entered object cannot be converted to any class. " + $"{((obj))}");
                        break;
                }
                
            }
        }
        
        public static void AddPropertyValue(SerializedProperty propertyValues,
            BasicPropertyData propData){
            propertyValues.InsertArrayElementAtIndex(propertyValues.arraySize);
            propertyValues.serializedObject.ApplyModifiedProperties();
            propertyValues.serializedObject.Update();
            var propPropertyValue = propertyValues.GetArrayElementAtIndex(propertyValues.arraySize - 1);
            var propBaseData = propPropertyValue.FindPropertyRelative("baseData");
            var propDataType = propPropertyValue.FindPropertyRelative("dataType");
            propDataType.intValue = (int)propData.dataType;
            propBaseData.managedReferenceValue = CreateBlankBaseData(propData.dataType);

            propBaseData.FindPropertyRelative("guid").stringValue = propData.Guid;
        }
        
        
        

#endif
        private static object CreateObject(Type type){ //DELETE
            object obj;
            if (type.IsValueType) {
                obj = Activator.CreateInstance(type);
                return obj;
            }
            obj = FormatterServices.GetUninitializedObject(type);
            obj = Convert.ChangeType(obj, type);
            return obj;

        }

        public static DataType ToDataType(Type type){ //Delete
            var obj = CreateObject(type);
            return obj switch{
                int => DataType.IntData,
                string => DataType.StringData,
                bool => DataType.BoolData,
                float => DataType.FloatData,
                double => DataType.DoubleData,
                long => DataType.LongData,
                Rect => DataType.RectData,
                RectInt => DataType.RectIntData,
                Color => DataType.ColorData,
                AnimationCurve => DataType.AnimationCurveData,
                Bounds => DataType.BoundsData,
                BoundsInt => DataType.BoundsIntData,
                Vector2 => DataType.Vector2Data,
                Vector3 => DataType.Vector3Data,
                Vector4 => DataType.Vector4Data,
                Vector2Int => DataType.Vector2INTData,
                Vector3Int => DataType.Vector3INTData,
                UnityEngine.Object => DataType.UnityObjectData,
                Gradient => DataType.GradientData,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public static object DataTypeToSystemObject(DataType type){
            return type switch{
                DataType.IntData => default(int),
                DataType.StringData => "",
                DataType.BoolData => default(bool),
                DataType.FloatData => default(float),
                DataType.DoubleData => default(double),
                DataType.LongData => default(long),
                DataType.RectData => default(Rect),
                DataType.RectIntData => default(RectInt),
                DataType.ColorData => default(Color),
                DataType.AnimationCurveData => new AnimationCurve(),
                DataType.BoundsData => default(Bounds),
                DataType.BoundsIntData => default(BoundsInt),
                DataType.Vector2Data => default(Vector2),
                DataType.Vector3Data => default(Vector3),
                DataType.Vector4Data => default(Vector4),
                DataType.Vector2INTData => default(Vector2Int),
                DataType.Vector3INTData => default(Vector3Int),
                DataType.UnityObjectData => new UnityEngine.Object(),
                DataType.GradientData => new Gradient(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        
        public static BaseData CreateBlankBaseData(DataType type){
            return type switch{
                DataType.IntData => new IntData(""),
                DataType.StringData => new StringData(""),
                DataType.BoolData => new BoolData(""),
                DataType.FloatData => new FloatData(""),
                DataType.DoubleData => new DoubleData(""),
                DataType.LongData => new LongData(""),
                DataType.RectData => new RectData(""),
                DataType.RectIntData => new RectIntData(""),
                DataType.ColorData => new ColorData(""),
                DataType.AnimationCurveData => new AnimationCurveData(""),
                DataType.BoundsData => new BoundsData(""),
                DataType.BoundsIntData => new BoundsIntData(""),
                DataType.Vector2Data => new Vector2Data(""),
                DataType.Vector3Data => new Vector3Data(""),
                DataType.Vector4Data => new Vector4Data(""),
                DataType.Vector2INTData => new Vector2IntData(""),
                DataType.Vector3INTData => new Vector3Data(""),
                DataType.UnityObjectData => new UnityObjectData(""),
                DataType.GradientData => new GradientData(""),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static void DrawGrid(Rect rect, Texture2D spritePreview, int spriteScale){

            var whiteColor = new Color(0.75f, 0.75f, 0.75f);
            var blackColor = new Color(0.75f, 0.75f, 0.75f);

            // Set grid texture.
            var gridWhiteTex = new Texture2D(1, 1); //Set grid, black and white.
            gridWhiteTex.SetPixel(0, 0, whiteColor);
            gridWhiteTex.Apply();

            var gridBlackTex = new Texture2D(1, 1);
            gridBlackTex.SetPixel(0, 0, blackColor);
            gridBlackTex.Apply();


            var grid = new Rect(rect.x, rect.y, 16 * spriteScale, 16 * spriteScale); //define a single 16x16 tile

            for (var i = 0; i < spritePreview.width / 16; i++) {


                for (var j = 0; j < spritePreview.height / 16; j += 2) {

                    var tex = i % 2 == 0 ? gridWhiteTex : gridBlackTex;
                    GUI.DrawTexture(grid, tex);
                    grid.y += grid.height; 
                    var texTwo = tex == gridWhiteTex ? gridBlackTex : gridWhiteTex;
                    GUI.DrawTexture(grid, texTwo);
                    grid.y += grid.height;
                }

                grid.y = rect.y;
                grid.x += grid.width;

            }

            if (!(rect.x + rect.width - grid.x > 0)) return;

            grid.width = rect.x + rect.width - grid.x; 

            for (var j = 0; j < spritePreview.height / 16; j += 2) {
                //iterate over Y
                GUI.DrawTexture(grid, gridBlackTex); 
                grid.y += grid.height;
                GUI.DrawTexture(grid, gridWhiteTex);
                grid.y += grid.height;
            }

            grid.height = rect.y + rect.height - grid.y;
            if (rect.y + rect.height - grid.y > 0) GUI.DrawTexture(grid, gridBlackTex); 
        }


        

    }

 
}


