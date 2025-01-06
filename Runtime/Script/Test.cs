using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using binc.PixelAnimator;
using binc.PixelAnimator.Common;
using UnityEngine.Events;

public class Test : MonoBehaviour
{

    // public DynamicEvent dynamicEvent;
    public UnityEvent e;
    public MethodStorage storage;
    public MethodData data;

    private void Start()
    {
        // dynamicEvent.Init();
    }




    private void Update()
    {
        // dynamicEvent.unityEvent.Invoke();
    }

    public void Log(float msg2, string asda, BoxData data)
    {
        Debug.Log("   aynen oyle  " + msg2);
    }
}
