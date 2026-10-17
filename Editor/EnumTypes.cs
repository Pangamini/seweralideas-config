#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SeweralIdeas.Config.Editor
{
    /// <summary>Every enum of the loaded assemblies, for picking the one of an <see cref="EnumConfigField"/>.</summary>
    internal static class EnumTypes
    {
        private static Type[]? s_all;

        public static IReadOnlyList<Type> All => s_all ??= Collect();

        private static Type[] Collect()
        {
            var result = new List<Type>();
            foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type?[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch(ReflectionTypeLoadException exception)
                {
                    types = exception.Types; // the ones that did load
                }

                foreach(Type? type in types)
                {
                    if(type is { IsEnum: true, IsGenericType: false } && type.FullName != null)
                        result.Add(type);
                }
            }

            result.Sort((lhs, rhs) => string.CompareOrdinal(lhs.FullName, rhs.FullName));
            return result.ToArray();
        }

        /// <summary>The enums whose full or short name contains the text, ignoring the case.</summary>
        public static List<Type> Find(string text)
        {
            var found = new List<Type>();
            if(string.IsNullOrWhiteSpace(text))
                return found;

            foreach(Type type in All)
            {
                if(type.FullName!.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    found.Add(type);
            }
            return found;
        }

    }
}
