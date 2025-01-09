using System;    
using UnityEngine;

[Serializable]
public class BaseData
{
    [SerializeField] public string name;
    public object InheritData{ get; protected set; }
}

[Serializable]
public class Data<T> : BaseData, ISerializationCallbackReceiver{
    
    [SerializeField] private T data;
    public void OnBeforeSerialize() => InheritData ??= data;
    public void OnAfterDeserialize() => data ??= (T)InheritData;
}
    