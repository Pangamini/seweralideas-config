#nullable enable
using UnityEngine;
using UnityEngine.Serialization;

namespace SeweralIdeas.Config
{
    public class BoolConfigReader : ConfigReader<bool, BoolConfigField>
    {
        [SerializeField]
        bool m_invert;
        
        [FormerlySerializedAs("onTrue")]
        [SerializeField]
        private UnityEngine.Events.UnityEvent m_onTrue = new();
        
        [FormerlySerializedAs("onFalse")]
        [SerializeField] 
        private UnityEngine.Events.UnityEvent m_onFalse = new();

        public UnityEngine.Events.UnityEvent OnTrue => m_onTrue;
        public UnityEngine.Events.UnityEvent OnFalse => m_onFalse;
        
        protected override bool PostprocessValue(bool value) => value ^ m_invert;
        
        protected override void FieldChanged(bool value)
        {
            value = PostprocessValue(value);
            OnChanged.Invoke(value);

            if (value)
                m_onTrue.Invoke();
            else
                m_onFalse.Invoke();
        }
    }
}