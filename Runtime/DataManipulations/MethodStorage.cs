using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace binc.PixelAnimator.DataManipulations
{
    public abstract class BaseMethodStorage
    {

    }

    [Serializable]
    public class MethodStorage : BaseMethodStorage
    {
        [SerializeField] public UnityEvent methods;
        [SerializeField] public List<MethodData> methodData;

        public void CompileAllFunctions(GameObject gameObject)
        {
            methods = new UnityEvent();
            foreach (var data in methodData)
            {

                methods.AddListener(data.CompileFunction(gameObject).Invoke );
            }
        }
    }

    [Serializable]
    public class MethodStorage<T> : BaseMethodStorage
    {
        [NonSerialized] public UnityEvent<T> methods;
        [SerializeField] public List<MethodData<T>> methodData;
        
        public void CompileAllFunctions(GameObject gameObject)
        {
            methods = new UnityEvent<T>();
            foreach (var data in methodData)
            {
                methods.AddListener(data.CompileFunction(gameObject).Invoke);
            }
        }
        
    }
}
