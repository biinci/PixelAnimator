using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using binc.PixelAnimator;
using binc.PixelAnimator.Common;
using UnityEngine.Events;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine.Serialization;

public class Program
{
    public static Action GetFunction(MethodData data)
    {
        var instance = data.instance;
        var info = data.method.methodInfo;

        var parameters = data.parameters.Select(p => p.InheritData).ToArray();

        var lambdaParam = Expression.Parameter(typeof(object[]), "parameters");

        var methodParams = info.GetParameters();
        var convertedParams = new Expression[methodParams.Length];
        for (var i = 0; i < methodParams.Length; i++)
        {
            var paramAccess = Expression.ArrayIndex(lambdaParam, Expression.Constant(i));
            convertedParams[i] = Expression.Convert(paramAccess, methodParams[i].ParameterType);
        }

        var methodCall = Expression.Call(
            Expression.Constant(instance),  
            info,                    
            convertedParams                
        );

        var lambda = Expression.Lambda<Action<object[]>>(methodCall, lambdaParam);

        var compiledDelegate = lambda.Compile();
        return () => { compiledDelegate(parameters);};
    }

    public void FunctionCreatedByUser(string data)
    {
        Debug.Log(data);
    }
}
//kullanici yazdigi kodlarla instance'ni belirtebilir. boylelikle herhangi bir karisilikta yasanmaz
public class StringClass
{
    public string name;
}

public class Test : MonoBehaviour
{
    public float asdadasdas;
    public BoxData boxData;
    private PixelAnimator animator;
    // public DynamicEvent dynamicEvent;
    private Rigidbody2D body;
    private StringClass stringClass;
    public MethodStorage storage;
    private void Start()
    {
        
        foreach (var unityAction in storage.methodData.Select(t => new UnityAction(Program.GetFunction(t))))
        {
            storage.methods.AddListener(unityAction);
        }
        stringClass = new StringClass
        {
            name = gameObject.transform.position.x.ToString(CultureInfo.InvariantCulture)
        };
        storage.methods.Invoke();

        // // dynamicEvent.Init();
        // var a = Program.GetFunction();
        // UnityAction unityAction = new UnityAction(a);
        // e.AddListener(unityAction);
        // e.RemoveListener(unityAction);
        // e.Invoke();
        // e.AddListener(unityAction);
        // e.Invoke();
        //
    }
    
    public void Log(float msg2, string asda, BoxData data)
    {
        
        Debug.Log(stringClass.name + "   " + asda);
    }


    private void Update()
    {
        // dynamicEvent.unityEvent.Invoke();
    }


}
