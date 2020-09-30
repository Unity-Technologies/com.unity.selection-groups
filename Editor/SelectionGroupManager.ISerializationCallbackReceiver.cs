using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.SelectionGroups
{

    public partial class SelectionGroupManager : ISerializationCallbackReceiver
    {
        [SerializeField] int[] _keys;
        [SerializeField] SelectionGroup[] _values;

        string[] _names;

        static SelectionGroupManager s_Instance;

        /// <summary>
        /// The single instance of the SelectionGroupManager.
        /// </summary>
        /// <value></value>
        public static SelectionGroupManager instance
        {
            get
            {
                return s_Instance;
            }
        }

        internal static void CreateAndLoad()
        {

            System.Diagnostics.Debug.Assert(s_Instance == null);
            //Find existing
            var managers = Resources.FindObjectsOfTypeAll(typeof(SelectionGroupManager));
            if (managers.Length > 0)
            {
                s_Instance = managers[0] as SelectionGroupManager;
                if (managers.Length != 1)
                {
                    Debug.LogError($"Multiple SelectionGroupManager instances detected! {managers.Length}");
                }
            }

            if (s_Instance == null)
            {
                // Load
                string filePath = GetFilePath();
                if (!string.IsNullOrEmpty(filePath))
                {
                    // If a file exists the
                    var objects = InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                    if (objects.Length > 0 && objects[0] is SelectionGroupManager)
                    {
                        s_Instance = (SelectionGroupManager)objects[0];
                    }
                }
            }

            if (s_Instance == null)
            {
                // Create
                var t = CreateInstance<SelectionGroupManager>();
                // t.hideFlags = HideFlags.HideAndDontSave;
                s_Instance = t;
            }
            System.Diagnostics.Debug.Assert(s_Instance != null);
        }

        void Save()
        {
            if (s_Instance != null)
            {
                var path = GetFilePath();
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                InternalEditorUtility.SaveToSerializedFileAndForget(new[] { s_Instance }, path, true);
            }
        }

        static string GetFilePath()
        {
            string filePath = "SelectionGroups" + "/" + "SelectionGroups.asset";
            return filePath;
        }

        /// <summary>
        /// The serialization method for this class.
        /// </summary>
        public void OnBeforeSerialize()
        {
            if (groups != null)
            {
                _values = groups.Values.ToArray();
                _keys = (from i in _values select i.groupId).ToArray();
                _names = (from i in _values select i.name).ToArray();
            }
        }

        /// <summary>
        /// The deserialization method for this class.
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (groups == null)
                groups = new Dictionary<int, SelectionGroup>();
            else
                groups.Clear();
            if (_keys != null && _values != null)
            {
                for (var i = 0; i < _keys.Length; i++)
                {
                    groups.Add(_keys[i], _values[i]);
                }
            }
        }

        internal static SelectionGroup Popup(Rect rect, SelectionGroup group)
        {
            var groupId = group.groupId;
            var index = System.Array.IndexOf(instance._keys, groupId);
            if (index < 0) index = 0;
            index = EditorGUI.Popup(rect, index, instance._names);
            if (index > 0 && index < instance._keys.Length)
                return instance._values[index];
            return null;
        }
    }
}