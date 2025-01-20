using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using binc.PixelAnimator;
using binc.PixelAnimator.DataManipulations;
using Object = UnityEngine.Object;

[Serializable]
public class MethodData : ISerializationCallbackReceiver
{
    public Object instance;
    public SerializableMethodInfo method;
    [SerializeReference] public List<BaseData> parameters;
    [ReadOnly] public string globalId;
    public void SelectMethod(MethodInfo methodInfo)
    {
        method.methodInfo = methodInfo;
        method.OnBeforeSerialize();
        parameters = new List<BaseData>();
        if(methodInfo == null) return;

        var paramCount = methodInfo.GetParameters().Length;
        for (var i = 0; i < paramCount; i++)
        {
            var parameterType = methodInfo.GetParameters()[i].ParameterType;
            var genericDataType = typeof(SerializableData<>).MakeGenericType(parameterType);
            var baseData = (BaseData)Activator.CreateInstance(genericDataType);
            parameters.Add(baseData);
        }
    }

    public void OnBeforeSerialize()
    {
        if (method?.methodInfo == null)
        {
            parameters = new List<BaseData>();
        }
    }

    public void OnAfterDeserialize()
    {
    }
}

