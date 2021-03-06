using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    [CustomEditor(typeof(LocalizedTableCollection), true)]
    class LocalizedTableCollectionEditor : UnityEditor.Editor
    {
        class Styles
        {
            public static readonly GUIContent addTable = new GUIContent("Add", "Add the table to the collection.");
            public static readonly GUIContent createTable = new GUIContent("Create", "Create a table for the Locale.");
            public static readonly GUIContent editCollection = new GUIContent("Open in Table Editor", "Open the collection for editing in the tables window.");
            public static readonly GUIContent extensions = new GUIContent("Extensions");
            public static readonly GUIContent looseTables = new GUIContent("Loose Tables");
            public static readonly GUIContent looseTablesInfo = new GUIContent("The following tables do not belong to any collection and share the same Shared Table Data as this collection. They can be added to this collection.");
            public static readonly string missingSharedTableData = "This collection is missing its Shared Table Data.";
            public static readonly GUIContent missingTables = new GUIContent("Missing Tables");
            public static readonly GUIContent missingTablesInfo = new GUIContent("These are tables that are missing for the Locales in the project.");
            public static readonly GUIContent noExtensions = new GUIContent("No Available Extensions");
            public static readonly GUIContent removeTable = new GUIContent("Remove", "Remove the table from the collection");
        }

        LocalizedTableCollection m_Collection;
        SerializedProperty m_Tables;
        SerializedProperty m_SharedTableData;
        SerializedProperty m_Extensions;
        List<LocalizedTable> m_LooseTables = new List<LocalizedTable>();
        List<Locale> m_MissingTables = new List<Locale>();
        ReorderableListExtended m_ExtensionsList;
        bool m_ShowLooseTables = true;
        bool m_ShowMissingTables = true;

        void OnEnable()
        {
            m_Collection = target as LocalizedTableCollection;
            m_Tables = serializedObject.FindProperty("m_Tables");
            m_SharedTableData = serializedObject.FindProperty("m_SharedTableData");
            m_Extensions = serializedObject.FindProperty("m_Extensions");

            m_ExtensionsList = new ReorderableListExtended(serializedObject, m_Extensions);
            m_ExtensionsList.AddMenuType = typeof(CollectionExtension);
            m_ExtensionsList.RequiredAttribute = target is StringTableCollection ? typeof(StringTableCollectionExtensionAttribute) : typeof(AssetTableCollectionExtensionAttribute);
            m_ExtensionsList.Header = Styles.extensions;
            m_ExtensionsList.NoItemMenuItem = Styles.noExtensions;
            m_ExtensionsList.CreateNewInstance = (type) =>
            {
                var instance = Activator.CreateInstance(type) as CollectionExtension;
                instance.Init(target as LocalizedTableCollection);
                return instance;
            };

            LocalizationEditorSettings.EditorEvents.TableAddedToCollection += OnTableModified;
            LocalizationEditorSettings.EditorEvents.TableRemovedFromCollection += OnTableModified;
            Undo.undoRedoPerformed += RefreshTables;
            RefreshTables();
        }

        void OnDisable()
        {
            LocalizationEditorSettings.EditorEvents.TableAddedToCollection -= OnTableModified;
            LocalizationEditorSettings.EditorEvents.TableRemovedFromCollection -= OnTableModified;
            Undo.undoRedoPerformed -= RefreshTables;
        }

        void OnTableModified(LocalizedTableCollection col, LocalizedTable tbl)
        {
            if (col == m_Collection)
                RefreshTables();
        }

        void RefreshTables()
        {
            // Find loose tables
            m_LooseTables.Clear();

            if (m_Collection.SharedData == null)
                return;

            LocalizationEditorSettings.FindLooseStringTablesUsingSharedTableData(m_Collection.SharedData, m_LooseTables);

            // Find missing tables by project locales
            var projectLocales = LocalizationEditorSettings.GetLocales();
            m_MissingTables.Clear();
            foreach (var locale in projectLocales)
            {
                if (!m_Collection.ContainsTable(locale.Identifier))
                    m_MissingTables.Add(locale);
            }

            Repaint();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (m_Collection.SharedData == null)
            {
                EditorGUILayout.HelpBox(Styles.missingSharedTableData, MessageType.Error);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_SharedTableData);
                if (EditorGUI.EndChangeCheck())
                {
                    RefreshTables();
                }
                return;
            }

            if (EditorGUILayout.PropertyField(m_Tables, false))
            {
                EditorGUI.indentLevel++;
                var tables = m_Collection.Tables;
                for (int i = 0; i < tables.Count; ++i)
                {
                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button(tables[i].asset?.name, EditorStyles.label))
                    {
                        EditorGUIUtility.PingObject(tables[i].asset);
                    }

                    if (GUILayout.Button(Styles.removeTable, GUILayout.Width(60)))
                    {
                        m_Collection.RemoveTable(tables[i].asset);
                        GUIUtility.ExitGUI();
                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            // Loose tables
            if (m_LooseTables.Count > 0)
            {
                m_ShowLooseTables = EditorGUILayout.Foldout(m_ShowLooseTables, Styles.looseTables);
                if (m_ShowLooseTables)
                {
                    EditorGUILayout.HelpBox(Styles.looseTablesInfo);
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < m_LooseTables.Count; ++i)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(m_LooseTables[i].name, EditorStyles.label))
                        {
                            EditorGUIUtility.PingObject(m_LooseTables[i]);
                        }

                        if (GUILayout.Button(Styles.addTable, GUILayout.Width(50)))
                        {
                            m_Collection.AddTable(m_LooseTables[i]);
                            GUIUtility.ExitGUI();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
            }

            // Missing tables
            if (m_MissingTables.Count > 0)
            {
                m_ShowMissingTables = EditorGUILayout.Foldout(m_ShowMissingTables, Styles.missingTables);
                if (m_ShowMissingTables)
                {
                    EditorGUILayout.HelpBox(Styles.missingTablesInfo);
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < m_MissingTables.Count; ++i)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(m_MissingTables[i].name, EditorStyles.label))
                        {
                            EditorGUIUtility.PingObject(m_MissingTables[i]);
                        }

                        if (GUILayout.Button(Styles.createTable, GUILayout.Width(60)))
                        {
                            m_Collection.AddNewTable(m_MissingTables[i].Identifier);
                            GUIUtility.ExitGUI();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
            }

            if (GUILayout.Button(Styles.editCollection))
            {
                LocalizationTablesWindow.ShowWindow(target as LocalizedTableCollection);
            }

            m_ExtensionsList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
