#nullable enable
using System;
using System.Globalization;
using System.Text;
using SeweralIdeas.Config;
using UnityEngine;
using UnityEngine.Events;

namespace SeweralIdeas.ConfigGui
{
    /// <summary>A GUI element of a number field that also shows the value as text: to an event, and to a <c>TMP_Text</c>.</summary>
    public abstract class ConfigGuiTextElement<T, TField> : ConfigGuiElement<T, TField> where TField : ConfigField<T>
    {
        [SerializeField] private string             m_numberFormat = "{0:F2}";
        [SerializeField] private UnityEvent<string> m_textOutput = new();
#if SEWERALIDEAS_CONFIG_TMP
        [Tooltip("Shows the value, without allocating a string for it.")]
        [SerializeField] private TMPro.TMP_Text?    m_tmpOutput;
#endif

        private ValueFormat? m_format;
        private int          m_codeListeners;

        /// <summary>
        /// The text of the value, when it changes: the same event as the one in the inspector, for listeners added in code.
        /// The string is only built while something listens. It is not raised for a subscriber that comes after the text was
        /// last set.
        /// </summary>
        public event Action<string> TextChanged
        {
            add
            {
                m_textOutput.AddListener(new UnityAction<string>(value));
                m_codeListeners++;
            }
            remove
            {
                m_textOutput.RemoveListener(new UnityAction<string>(value));
                m_codeListeners = Mathf.Max(0, m_codeListeners - 1);
            }
        }

        /// <summary>The text of the value in the format. The builder is the format's own: use it before the next call.</summary>
        private protected abstract StringBuilder BuildText(ValueFormat format, T value);

        protected void TryUpdateText()
        {
            if(Field == null)
                return;

            m_format ??= new ValueFormat(m_numberFormat, CultureInfo.InvariantCulture);
            StringBuilder? text = null;

#if SEWERALIDEAS_CONFIG_TMP
            if(m_tmpOutput != null)
            {
                text = BuildText(m_format, Field.Value);
                m_tmpOutput.SetText(text);
            }
#endif
            // A UnityEvent only counts the listeners set in the inspector; the ones added in code are counted by TextChanged.
            if(m_textOutput.GetPersistentEventCount() <= 0 && m_codeListeners <= 0)
                return;
            
            m_textOutput.Invoke((text ?? BuildText(m_format, Field.Value)).ToString());
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            m_format = null;
            if(RegisteredField != null)
                TryUpdateText();
        }
    }
}
