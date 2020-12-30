using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.SelectionGroups;
using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.SelectionGroupsEditor
{

    [InitializeOnLoad]
    public partial class SelectionGroupPersistenceManager: ISerializationCallbackReceiver
    {
        [SerializeField] int[] _keys;
        [SerializeField] SelectionGroup[] _values;

        string[] _names;
        static SelectionGroupPersistenceManager Instance;

        static SelectionGroupPersistenceManager()
        {
            EditorApplication.delayCall += CreateAndLoad;
        }
        internal static void CreateAndLoad()
        {

            System.Diagnostics.Debug.Assert(Instance == null);
            //Find existing
            var managers = Resources.FindObjectsOfTypeAll(typeof(SelectionGroupPersistenceManager));
            if (managers.Length > 0)
            {
                Instance = managers[0] as SelectionGroupPersistenceManager;
                if (managers.Length != 1)
                {
                    Debug.LogError($"Multiple SelectionGroupManager instances detected! {managers.Length}");
                }
            }

            if (Instance == null)
            {
                // Load
                string filePath = GetFilePath();
                if (!string.IsNullOrEmpty(filePath))
                {
                    // If a file exists the
                    var objects = InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                    if (objects.Length > 0 && objects[0] is SelectionGroupPersistenceManager)
                    {
                        Instance = (SelectionGroupPersistenceManager)objects[0];
                    }
                }
            }

            if (Instance == null)
            {
                // Create
                var t = CreateInstance<SelectionGroupPersistenceManager>();
                // t.hideFlags = HideFlags.HideAndDontSave;
                Instance = t;
            }
            System.Diagnostics.Debug.Assert(Instance != null);
        }

        internal static void Save()
        {
            if (Instance != null)
            {
                var path = GetFilePath();
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                InternalEditorUtility.SaveToSerializedFileAndForget(new[] { Instance }, path, true);
            }
        }

        static string GetFilePath()
        {
            string filePath = "SelectionGroups" + "/" + "SelectionGroups.asset";
            return filePath;
        }

        public void OnBeforeSerialize()
        {
            _values = editorGroups.ToArray();
            foreach (var i in _values)
                SelectionGroupManager.Unregister(i);
        }

        public void OnAfterDeserialize()
        {
            editorGroups.Clear();
            editorGroups.UnionWith(_values);
            foreach (var i in _values)
                SelectionGroupManager.Register(i);
        }
    }
}