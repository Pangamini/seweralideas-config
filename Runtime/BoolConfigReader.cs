using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeweralIdeas.Config
{
    public class BoolConfigReader : ConfigReader<bool, BoolConfigValue>
    {
        [SerializeField] bool m_invert;
        [SerializeField] public UnityEngine.Events.UnityEvent onTrue;
        [SerializeField] public UnityEngine.Events.UnityEvent onFalse;

        protected override bool PostprocessValue(bool value)
        {
            return value ^ m_invert;
        }

        protected override void OnValueChanged(bool value)
        {
            value = PostprocessValue(value);
            onChanged.Invoke(value);

            if (value)
                onTrue.Invoke();
            else
                onFalse.Invoke();
        }
    }
}