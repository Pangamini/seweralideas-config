#nullable enable
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SeweralIdeas.Config.Editor
{
    /// <summary>
    /// The descriptions of a plan in the order they are tried, which can be changed by dragging. A description is created
    /// as a nested asset of the plan, or is any description asset put in the list; removing one from the list also
    /// destroys it if it is nested in this plan.
    /// </summary>
    [CustomEditor(typeof(ConfigStoragePlan))]
    public class ConfigStoragePlanEditor : UnityEditor.Editor
    {
        private const string DescriptionsProperty = "m_descriptions";

        private ReorderableList m_list = null!;
        private UnityEditor.Editor m_descriptionEditor = null!; // of the selected description, kept between repaints

        protected void OnEnable()
        {
            SerializedProperty descriptions = serializedObject.FindProperty(DescriptionsProperty);
            m_list = new ReorderableList(serializedObject, descriptions, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Storage descriptions - the first one available is used"),
                drawElementCallback = DrawElement,
                onAddDropdownCallback = OnAddDropdown,
                onRemoveCallback = OnRemove,
                elementHeight = EditorGUIUtility.singleLineHeight + 4
            };
        }

        protected void OnDisable()
        {
            if(m_descriptionEditor != null)
                DestroyImmediate(m_descriptionEditor);
            m_descriptionEditor = null!;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            m_list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            SelectedDescriptionGUI();
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = m_list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;

            const float numberWidth = 20;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, numberWidth, rect.height), (index + 1).ToString());
            EditorGUI.PropertyField(new Rect(rect.x + numberWidth, rect.y, rect.width - numberWidth, rect.height), element, GUIContent.none);
        }

        private void SelectedDescriptionGUI()
        {
            SerializedProperty descriptions = m_list.serializedProperty;
            int index = m_list.index;
            if(index < 0 || index >= descriptions.arraySize)
                return;

            Object? description = descriptions.GetArrayElementAtIndex(index).objectReferenceValue;
            if(description == null)
                return;

            using (new GUILayout.VerticalScope(GUIContent.none, "box"))
            {
                GUILayout.Label(description.name, EditorStyles.boldLabel);

                // Kept, not created on every repaint: an Editor is an object, and a leaked one stays until the domain reloads.
                CreateCachedEditor(description, null, ref m_descriptionEditor);
                m_descriptionEditor.OnInspectorGUI();
            }
        }

        private void OnAddDropdown(Rect buttonRect, ReorderableList list)
        {
            var plan = (ConfigStoragePlan)target;
            TypeMenu.Show(buttonRect, typeof(StorageDescription), type => AddDescription(plan, type));
        }

        // Nested in the plan, and at the end of the list, which is the least preferred place.
        private void AddDescription(ConfigStoragePlan plan, Type type)
        {
            if(!EditorUtility.IsPersistent(plan))
            {
                Debug.LogError("Descriptions are nested assets of the plan, which has to be an asset first.", plan);
                return;
            }

            // One undo step for the whole thing: the object, and its place in the list
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Create StorageDescription");
            int undoGroup = Undo.GetCurrentGroup();

            var description = (StorageDescription)CreateInstance(type);
            description.name = type.Name;
            AssetDatabase.AddObjectToAsset(description, plan);
            Undo.RegisterCreatedObjectUndo(description, "Create StorageDescription");

            using SerializedObject planObject = new(plan);
            SerializedProperty descriptions = planObject.FindProperty(DescriptionsProperty);
            int index = descriptions.arraySize;
            descriptions.InsertArrayElementAtIndex(index);
            descriptions.GetArrayElementAtIndex(index).objectReferenceValue = description;
            planObject.ApplyModifiedProperties();

            Undo.CollapseUndoOperations(undoGroup);
            EditorUtility.SetDirty(plan);
            m_list.index = index;
            SaveAndRefresh();
        }

        private void OnRemove(ReorderableList list)
        {
            SerializedProperty descriptions = list.serializedProperty;
            int index = list.index;
            if(index < 0 || index >= descriptions.arraySize)
                return;

            var plan = (ConfigStoragePlan)target;
            SerializedProperty element = descriptions.GetArrayElementAtIndex(index);
            Object? description = element.objectReferenceValue;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Remove StorageDescription");
            int undoGroup = Undo.GetCurrentGroup();

            // Deleting an element that holds an object only clears it; the second call removes the element.
            if(description != null)
                element.objectReferenceValue = null;
            descriptions.DeleteArrayElementAtIndex(index);
            descriptions.serializedObject.ApplyModifiedProperties();

            // Only what is nested in this plan is the plan's to destroy; an asset that was merely listed stays.
            if(description != null && AssetDatabase.IsSubAsset(description) && AssetDatabase.GetAssetPath(description) == AssetDatabase.GetAssetPath(plan))
                Undo.DestroyObjectImmediate(description);

            Undo.CollapseUndoOperations(undoGroup);
            list.index = Mathf.Clamp(index, 0, descriptions.arraySize - 1);
            SaveAndRefresh();
        }

        private static void SaveAndRefresh()
        {
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            };
        }
    }
}
