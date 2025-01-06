using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using binc.PixelAnimator;
using binc.PixelAnimator.Common;
using UnityEngine.Events;
using System.Reflection;
using System.Linq.Expressions;
public class Program
{
    public static Action GetFunction()
    {
        Program program = new Program();

        var parameters = new object[] {"alo" };

        MethodInfo methodInfo = typeof(Program).GetMethod("FunctionCreatedByUser");

        var lambdaParam = Expression.Parameter(typeof(object[]), "parameters");

        var methodParams = methodInfo.GetParameters();
        var convertedParams = new Expression[methodParams.Length];
        for (int i = 0; i < methodParams.Length; i++)
        {
            var paramAccess = Expression.ArrayIndex(lambdaParam, Expression.Constant(i));
            convertedParams[i] = Expression.Convert(paramAccess, methodParams[i].ParameterType);
        }

        var methodCall = Expression.Call(
            Expression.Constant(program),  
            methodInfo,                    
            convertedParams                
        );

        var lambda = Expression.Lambda<Action<object[]>>(methodCall, lambdaParam);

        Action<object[]> compiledDelegate = lambda.Compile();
        return () => { compiledDelegate(parameters);};
    }

    public void FunctionCreatedByUser(string data)
    {
        Debug.Log(data);
    }
}
public class Test : MonoBehaviour
{

    // public DynamicEvent dynamicEvent;
    public UnityEvent e;
    public MethodStorage storage;
    public MethodData data;

    private void Start()
    {
        // dynamicEvent.Init();
        var a = Program.GetFunction();
        UnityAction unityAction = new UnityAction(a);
        e.AddListener(unityAction);
        e.RemoveListener(unityAction);
        e.Invoke();
        e.AddListener(unityAction);
        e.Invoke();
        
        
        
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
