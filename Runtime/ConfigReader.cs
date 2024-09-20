using System;
using UnityEngine;
using UnityEngine.Events;

namespace SeweralIdeas.Config
{
    public class ConfigReader<T, TVal> : ConfigListener<T,TVal> 
        where T : IEquatable<T> 
        where TVal: ConfigField<T>
    {
        [SerializeField] private UnityEvent<T> m_onChanged = new UnityEvent<T>();

        public UnityEvent<T> OnChanged => m_onChanged;
        
        protected override void ValueChanged(T value) => OnChanged.Invoke(PostprocessValue(value));

        protected virtual T PostprocessValue(T value) => value;

    }
}