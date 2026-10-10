#nullable enable
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SeweralIdeas.Config.Editor
{
    /// <summary>A dropdown of the concrete types that are, or derive from, a base type; picking one calls back with it.</summary>
    internal static class TypeMenu
    {
        public static void Show(Rect buttonRect, Type baseType, Action<Type> onTypeSelected)
        {
            var types = new List<Type>();
            if(!baseType.IsAbstract)
                types.Add(baseType);
            foreach(Type type in TypeCache.GetTypesDerivedFrom(baseType))
            {
                if(!type.IsAbstract && !type.IsGenericTypeDefinition)
                    types.Add(type);
            }
            types.Sort((lhs, rhs) => string.CompareOrdinal(lhs.Name, rhs.Name));

            var menu = new GenericMenu();
            foreach(Type type in types)
            {
                Type selected = type;
                menu.AddItem(new GUIContent(type.Name), false, () => onTypeSelected(selected));
            }
            menu.DropDown(buttonRect);
        }
    }
}
