using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace binc.PixelAnimator.DataManipulations
{
    [Serializable]
    public abstract class BaseMethodData : ISerializationCallbackReceiver
    {
        public string GlobalId => globalId;
        [SerializeField, ReadOnly] private string globalId;
        public SerializableMethodInfo method = new(null);
        [SerializeReference] public List<BaseData> parameters = new();

        public virtual void SelectMethod(MethodInfo methodInfo)
        { 
            method.methodInfo = methodInfo;
            method.OnBeforeSerialize();
            CreateParametersForMethod(methodInfo);
        }

        protected virtual void CreateParametersForMethod(MethodInfo methodInfo)
        {
            parameters = new List<BaseData>();
            if (methodInfo == null) return;

            foreach (var param in methodInfo.GetParameters())
            {
                var parameterType = param.ParameterType;
                var genericType = typeof(SerializableData<>).MakeGenericType(parameterType);
                parameters.Add((BaseData)Activator.CreateInstance(genericType));
            }
        }

        public virtual void OnBeforeSerialize()
        {
            if (method?.methodInfo == null)
            {
                parameters = new List<BaseData>();
            }
        }

        public void OnAfterDeserialize()
        {
        }

        protected Object GetTargetObject()
        {
            return GlobalObjectId.TryParse(GlobalId, out var id)
                ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id)
                : null;
        }
        
    }

    [Serializable]
    public class MethodData : BaseMethodData
    {
        public Action CompileFunction()
        {
            var methodInfo = method.methodInfo;
            var reference = GetTargetObject();
            if(!reference) return () => { Debug.LogError("Reference not found"); };
            
            var editorParameters = parameters.Select(p => p.InheritData).ToArray();
            var lambdaParam = Expression.Parameter(typeof(object[]), "parameters");

            var methodParams = methodInfo.GetParameters();
            var convertedParams = new Expression[methodParams.Length];
            for (var i = 0; i < methodParams.Length; i++)
            {
                var paramAccess = Expression.ArrayIndex(lambdaParam, Expression.Constant(i));
                convertedParams[i] = Expression.Convert(paramAccess, methodParams[i].ParameterType);
            }

            var methodCall = Expression.Call(
                Expression.Constant(reference), 
                methodInfo,                   
                convertedParams               
            );

            var lambda = Expression.Lambda<Action<object[]>>(methodCall, lambdaParam);

            var compiledDelegate = lambda.Compile();
            return () => compiledDelegate(editorParameters);
        }
    }

    [Serializable]
    public class MethodData<T> : BaseMethodData
    {
        public Action<T> CompileFunction()
        {
            var methodInfo = method.methodInfo;
            var reference = GetTargetObject();
            if(!reference) return _ => { Debug.LogError("Reference not found"); };
            
            var editorParameters = parameters.Skip(1).Select(p => p.InheritData).ToArray();
            var lambdaParam = Expression.Parameter(typeof(object[]), "parameters");

            var methodParams = methodInfo.GetParameters();
            var convertedParams = new Expression[methodParams.Length];
            for (var i = 0; i < methodParams.Length; i++)
            {
                var paramAccess = Expression.ArrayIndex(lambdaParam, Expression.Constant(i));
                convertedParams[i] = Expression.Convert(paramAccess, methodParams[i].ParameterType);
            }

            var methodCall = Expression.Call(
                Expression.Constant(reference), 
                methodInfo,                   
                convertedParams               
            );

            var lambda = Expression.Lambda<Action<object[]>>(methodCall, lambdaParam);

            var compiledDelegate = lambda.Compile();
            return obj => compiledDelegate(CombineParameters(obj, editorParameters));
        }
        
        private object[] CombineParameters(T firstParam, object[] additional)
        {
            var combined = new object[additional.Length + 1];
            combined[0] = firstParam;
            Array.Copy(additional, 0, combined, 1, additional.Length);
            return combined;
        }
    }
}


