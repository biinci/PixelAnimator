using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System.Linq.Expressions;
using binc.PixelAnimator;
using UnityEditor;

public class MethodUtility
{
    public static Action GetFunction(MethodData data)
    {
        
        var parameters = data.parameters.Select(p => p.InheritData).ToArray();

        var lambdaParam = Expression.Parameter(typeof(object[]), "parameters");
        data.method.OnAfterDeserialize();
        var info = data.method.methodInfo;

        var methodParams = info.GetParameters();
        var convertedParams = new Expression[methodParams.Length];
        for (var i = 0; i < methodParams.Length; i++)
        {
            var paramAccess = Expression.ArrayIndex(lambdaParam, Expression.Constant(i));
            convertedParams[i] = Expression.Convert(paramAccess, methodParams[i].ParameterType);
        }

        var parsable= GlobalObjectId.TryParse(data.globalId, out var id);
        object value = parsable ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id):null;
        if (value == null)
        {
            return () => Debug.LogError("Object not found");
        }
        var methodCall = Expression.Call(
            Expression.Constant(value),  
            info,                    
            convertedParams                
        );

        var lambda = Expression.Lambda<Action<object[]>>(methodCall, lambdaParam);
        var compiledDelegate = lambda.Compile();
        return ()=>compiledDelegate(parameters);
    }

}


public class Test : MonoBehaviour
{
    public PixelAnimator animator;
    public PixelAnimation run;
    private void Start()
    {
        // animator.Play(run);
        
    }
    public void Log(string msg)
    {
        // Debug.Log(msg);
    }

}
