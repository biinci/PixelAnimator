using System;
using UnityEngine;

namespace binc.PixelAnimator.DataManipulations
{
    [Serializable]
    public class BaseData
    {
        [SerializeField] public string name;
        public object InheritData{ get; protected set; }
    }

    [Serializable]
    public class SerializableData<T> : BaseData, ISerializationCallbackReceiver{
    
        [SerializeField] private T data;
        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize()
        {
            InheritData = data;
        }
    }
}