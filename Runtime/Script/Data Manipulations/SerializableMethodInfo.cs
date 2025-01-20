using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class SerializableMethodInfo : ISerializationCallbackReceiver
{
    public SerializableMethodInfo(MethodInfo aMethodInfo)
    {
        methodInfo = aMethodInfo;
    }
    public MethodInfo methodInfo;
    public SerializableType serializableType;
    public string methodName;
    public List<SerializableType> parameters;
    public int flags;

    public void OnBeforeSerialize()
    {
        if (methodInfo == null)
        {
            parameters = new List<SerializableType>();
            serializableType = new SerializableType(null);
            methodName = "";
            flags = 0;
            return;  
        }
        
        serializableType = new SerializableType(methodInfo.DeclaringType);
        methodName = methodInfo.Name;
        if (methodInfo.IsPrivate)
            flags |= (int)BindingFlags.NonPublic;
        else
            flags |= (int)BindingFlags.Public;
        if (methodInfo.IsStatic)
            flags |= (int)BindingFlags.Static;
        else
            flags |= (int)BindingFlags.Instance;
        var p = methodInfo.GetParameters();
        if (p is { Length: > 0 })
        {
            parameters = new List<SerializableType>(p.Length);
            foreach (var t in p)
            {
                parameters.Add(new SerializableType(t.ParameterType));
            }
        }
        else
            parameters = null;
    }

    public void OnAfterDeserialize()
    {
        LoadMethodInfo();
    }

    public void LoadMethodInfo()
    {
        if (serializableType == null || string.IsNullOrEmpty(methodName))
            return;
        var type = serializableType.SystemType;
        System.Type[] param = null;
        if (parameters is { Count: > 0 })
        {
            param = new System.Type[parameters.Count];
            for (var i = 0; i < parameters.Count; i++)
            {
                param[i] = parameters[i].SystemType;
            }
        }

        if (param == null)
            methodInfo = type.GetMethod(methodName, (BindingFlags)flags);
        else
            methodInfo = type.GetMethod(methodName, (BindingFlags)flags, null, param, null);
    }
}
