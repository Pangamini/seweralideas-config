#nullable enable
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace SeweralIdeas.Config
{
    /// <summary>
    /// Passes the value of an <see cref="EnumConfigField"/> on, as the enum itself, whatever enum it is: a <c>UnityEvent</c> can't carry
    /// one, so it writes it to members (<see cref="Binding"/>), and reports whether each option is on (<see cref="Option"/>).
    /// </summary>
    /// <remarks>
    /// The members are reached by reflection. Where the code is stripped (IL2CPP), a property that nothing else uses may be removed:
    /// keep it with <c>[Preserve]</c>. A serialized field is always kept.
    /// </remarks>
    public class EnumConfigReader : ConfigReader<string, EnumConfigField>
    {
        /// <summary>A field or property of the type of the enum, to which the value is written.</summary>
        [Serializable]
        public struct Binding
        {
            public UnityEngine.Object? Target;
            public string?             Member;
        }

        /// <summary>Called with true when the option is on, and with false when it is not any more. Called once at the start, with what it is then.</summary>
        [Serializable]
        public class Option
        {
            public string          Name = string.Empty;
            public UnityEvent<bool> OnActive = new();
        }

        [SerializeField] private Binding[] m_bindings = Array.Empty<Binding>();
        [SerializeField] private Option[]  m_options  = Array.Empty<Option>();

        private MemberInfo?[]? m_members;
        private bool?[]?       m_optionStates;

        protected override void Start()
        {
            ResolveMembers();
            m_optionStates = new bool?[m_options.Length];
            base.Start();
        }

        private void ResolveMembers()
        {
            m_members = new MemberInfo?[m_bindings.Length];
            Type? enumType = Field != null ? Field.EnumType : null;

            for(int i = 0; i < m_bindings.Length; i++)
            {
                Binding binding = m_bindings[i];
                if(binding.Target == null || string.IsNullOrEmpty(binding.Member))
                    continue;

                MemberInfo? member = FindMember(binding.Target.GetType(), binding.Member!);
                Type? memberType = member switch
                {
                    FieldInfo field       => field.FieldType,
                    PropertyInfo property => property.PropertyType,
                    _                     => null,
                };

                if(member == null)
                    Debug.LogWarning($"{binding.Target.GetType().Name} has no field or property {binding.Member} to write to", this);
                else if(memberType != enumType)
                    Debug.LogWarning($"{binding.Target.GetType().Name}.{binding.Member} is {memberType?.Name}, not the {enumType?.Name ?? "enum"} of the config field", this);
                else
                    m_members[i] = member;
            }
        }

        // Fields (of any visibility, as the serialized ones often are private) and writable properties, also of the base classes
        private static MemberInfo? FindMember(Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for(Type? current = type; current != null; current = current.BaseType)
            {
                FieldInfo? field = current.GetField(name, flags);
                if(field != null)
                    return field;

                PropertyInfo? property = current.GetProperty(name, flags);
                if(property is { CanWrite: true })
                    return property;
            }
            return null;
        }

        protected override void FieldChanged(string value)
        {
            base.FieldChanged(value);

            Type? enumType = Field != null ? Field.EnumType : null;
            if(enumType == null || !EnumConfigField.TryParseValue(enumType, value, out object parsed))
                return;

            if(m_members != null)
            {
                for(int i = 0; i < m_bindings.Length; i++)
                {
                    MemberInfo? member = m_members[i];
                    object? target = m_bindings[i].Target;
                    if(member == null || target == null)
                        continue;

                    if(member is FieldInfo field)
                        field.SetValue(target, parsed);
                    else if(member is PropertyInfo property)
                        property.SetValue(target, parsed);
                }
            }

            if(m_optionStates == null || m_optionStates.Length != m_options.Length)
                m_optionStates = new bool?[m_options.Length];

            for(int i = 0; i < m_options.Length; i++)
            {
                Option option = m_options[i];
                if(!EnumConfigField.TryParseValue(enumType, option.Name, out object optionValue))
                    continue;

                bool active = EnumConfigField.IsActive(enumType, parsed, optionValue);
                if(m_optionStates[i] == active)
                    continue;

                m_optionStates[i] = active;
                option.OnActive.Invoke(active);
            }
        }
    }
}
