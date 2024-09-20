using UnityEngine;

namespace SeweralIdeas.Config
{
    public class BoolConfigReader : ConfigReader<bool, BoolConfigField>
    {
        [SerializeField] bool m_invert;
        [SerializeField] public UnityEngine.Events.UnityEvent onTrue;
        [SerializeField] public UnityEngine.Events.UnityEvent onFalse;

        protected override bool PostprocessValue(bool value) => value ^ m_invert;
        
        protected override void ValueChanged(bool value)
        {
            value = PostprocessValue(value);
            OnChanged.Invoke(value);

            if (value)
                onTrue.Invoke();
            else
                onFalse.Invoke();
        }
    }
}