using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using binc.PixelAnimator;


[Serializable]
public class MethodStorage
{

    [SerializeField] private UnityEvent methods;
    
    #if UNITY_EDITOR
    
    [SerializeField] public List<MethodData> methodData;
    
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

