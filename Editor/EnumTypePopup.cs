#nullable enable
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SeweralIdeas.Config.Editor
{
    /// <summary>A popup window to find an enum by typing a part of its name, and pick it from the list of those that match.</summary>
    internal class EnumTypePopup : PopupWindowContent
    {
        private const int   MaxShown   = 300; // more than can be looked through anyway; the search narrows it down
        private const float RowHeight  = 20;
        private const string SearchControl = "EnumTypeSearch";

        private readonly Action<Type?> m_onPicked;
        private readonly Type?         m_current;

        private string             m_search = string.Empty;
        private string?            m_shownSearch;
        private List<Type>         m_matches = new();
        private Vector2            m_scroll;
        private bool               m_focused;

        public EnumTypePopup(Type? current, Action<Type?> onPicked)
        {
            m_current  = current;
            m_onPicked = onPicked;
        }

        public override Vector2 GetWindowSize() => new(380, 340);

        public override void OnGUI(Rect rect)
        {
            Event e = Event.current;
            bool pickFirst = e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter);
            if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                editorWindow.Close();
                return;
            }

            GUI.SetNextControlName(SearchControl);
            m_search = EditorGUILayout.TextField(m_search, EditorStyles.toolbarSearchField);
            if(!m_focused)
            {
                EditorGUI.FocusTextInControl(SearchControl);
                m_focused = true;
            }

            if(m_shownSearch != m_search)
            {
                m_matches     = string.IsNullOrWhiteSpace(m_search) ? new List<Type>(EnumTypes.All) : EnumTypes.Find(m_search.Trim());
                m_shownSearch = m_search;
            }

            if(pickFirst && m_matches.Count > 0)
            {
                Pick(m_matches[0]);
                e.Use();
                return;
            }

            using(var scroll = new EditorGUILayout.ScrollViewScope(m_scroll))
            {
                m_scroll = scroll.scrollPosition;

                if(GUILayout.Button(m_current == null ? "(none) *" : "(none)", EditorStyles.miniButton, GUILayout.Height(RowHeight)))
                {
                    Pick(null);
                    return;
                }

                int shown = Mathf.Min(m_matches.Count, MaxShown);
                for(int i = 0; i < shown; i++)
                {
                    Type type = m_matches[i];
                    string label = type == m_current ? type.FullName + " *" : type.FullName!;
                    if(GUILayout.Button(new GUIContent(label, type.Assembly.GetName().Name), EditorStyles.miniButton, GUILayout.Height(RowHeight)))
                    {
                        Pick(type);
                        return;
                    }
                }
            }

            string summary = m_matches.Count == 0 ? "No enum has that name."
                           : m_matches.Count > MaxShown ? $"{m_matches.Count} enums, showing {MaxShown}: type to narrow it down."
                           : $"{m_matches.Count} enums. Enter picks the first.";
            EditorGUILayout.LabelField(summary, EditorStyles.miniLabel);
        }

        private void Pick(Type? type)
        {
            m_onPicked(type);
            editorWindow.Close();
        }
    }
}
