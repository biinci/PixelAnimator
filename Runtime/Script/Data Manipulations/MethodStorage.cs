using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class MethodStorage 
{

    [SerializeField] public UnityEvent methods;
    [SerializeField] public List<MethodData> methodData;
    public void OnEnable()
    { 
        methods = new UnityEvent();
        foreach (var method in methodData.Select(MethodUtility.GetFunction))
        {
            methods.AddListener(method.Invoke);
        }
    }
    
    #if UNITY_EDITOR
    
    
    public void AddMethod(MethodData method)
    {
        methodData.Add(method);
    }
    
    public void RemoveMethod(MethodData method)
    {
        var index = methodData.IndexOf(method);
        if (index == -1) return;
        methodData.RemoveAt(index);
    }

    #endif
    
    public void Call()
    {
        methods.Invoke();
    }

}

