using UnityEngine;
using System;

namespace  binc.PixelAnimator.DataProvider{

    [Serializable]
    public class BaseData{
        
        [ReadOnly, SerializeField] private string guid;
        public object InheritData{ get; protected set; }
        public string Guid => guid;
        public BaseData(string guid){
            this.guid = guid;
        }

        public void SetGuid(string guid){
            this.guid = guid;
        }
        

    }
    
    [Serializable]
    public struct GenericData{
        [SerializeReference] public BaseData baseData;
        public DataType dataType;

        public object GetInheritData(){
            return baseData.InheritData;
        }
        
        public T GetInheritData<T>(){
            return (T)baseData.InheritData;
        }
        
    }
    
    
    [Serializable]
    public class Data<T> : BaseData, ISerializationCallbackReceiver{
        
        [SerializeField] private T data;
        
        public Data(string guid) : base(guid){
        }

        public void OnBeforeSerialize(){
            InheritData = data;
        }

        public void OnAfterDeserialize(){
            // data = (T)InheritData;
        }
    }
    

    #region DataClasses 
        
        [Serializable]
        public class IntData : Data<int>{
            public IntData(string guid) : base(guid){
            }

        }
        
        [Serializable]
        public class StringData : Data<string>{
            public StringData(string guid) : base(guid){
            }
        }

        [Serializable]
        public class BoolData : Data<bool>{
            public BoolData(string guid) : base(guid){
            }
        }
        
        [Serializable]
        public class FloatData : Data<float> {
            public FloatData(string guid) : base(guid){
            }
            
        }
        
        
        [Serializable]
        public class DoubleData : Data<double>{
            public DoubleData(string guid) : base(guid){
            }
        }

        
        [Serializable]
        public class LongData : Data<long> {
            public LongData(string guid) : base(guid){
            }

        }
        
        public class RectData : Data<Rect>{

            public RectData(string guid ) : base(guid){
            }

        }
        
        public class RectIntData : Data<RectInt>{
            public RectIntData(string guid ) : base(guid){
            }

        }
        
        [Serializable]
        public class ColorData : Data<Color> {
            public ColorData(string guid) : base(guid){
            }

        }
        
        [Serializable]
        public class AnimationCurveData : Data<AnimationCurve>{
            public AnimationCurveData(string guid) : base(guid){
            }

        }
        
        [Serializable]
        public class BoundsData : Data<Bounds>{
            public BoundsData(string guid) : base(guid){
            }
            
        }
        [Serializable]
        public class BoundsIntData : Data<BoundsInt>{
            public BoundsIntData(string guid) : base(guid){
            }

        }
        
        
        [Serializable]
        public class Vector2Data : Data<Vector2> {
            public Vector2Data(string guid) : base(guid){
            }

        }
        
        [Serializable]
        public class Vector3Data : Data<Vector3>{
            public Vector3Data(string guid) : base(guid){
            }
        }
    
        [Serializable]
        public class Vector4Data : Data<Vector4>{
            public Vector4Data(string guid) : base(guid){
            }
        }
        
        [Serializable]
        public class Vector2IntData : Data<Vector2Int>{
            public Vector2IntData(string guid) : base(guid){
            }
        }
        
        [Serializable]
        public class Vector3IntData : Data<Vector3Int>{
            public Vector3IntData(string guid) : base(guid){
            }
        }
        
        [Serializable]
        public class UnityObjectData : Data<UnityEngine.Object>{
            public UnityObjectData(string guid) : base(guid){
            }
            
        }
        
        [Serializable]
        public class GradientData : Data<Gradient>{
            public GradientData(string guid) : base(guid){
            }
        }

    #endregion
    
    
}