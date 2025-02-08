using System;
using System.Collections.Generic;
using System.Linq;
using binc.PixelAnimator.DataManipulations;
using UnityEngine;
using UnityEngine.Events;

public class BaseMethodStorage
{
    
}

[Serializable]
public class MethodStorage : BaseMethodStorage
{
    [SerializeField] public UnityEvent methods;
    [SerializeField] public List<MethodData> methodData;
    public void OnEnable()
    { 
        methods = new UnityEvent();
        foreach (var method in methodData.Select(m => m.CompileFunction()))
        {
            methods.AddListener(method.Invoke);
        }
    }
}

[Serializable]
public class MethodStorage<T> : BaseMethodStorage
{
    [SerializeField] public UnityEvent<T> methods;
    [SerializeField] public List<MethodData<T>> methodData;
    public void OnEnable()
    { 
        methods = new UnityEvent<T>();
        foreach (var method in methodData.Select(m => m.CompileFunction()))
        {
            methods.AddListener(method.Invoke);
        }
    }
}
