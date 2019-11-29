using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
    public class SelectionGroupManager : ScriptableObject
    {
        private static SelectionGroupManager s_Instance;

        public static SelectionGroupManager instance
        {
            get
            {
                if (s_Instance == null) CreateAndLoad();
                return s_Instance;
            }
        }

        void OnEnable()
        {
        }

        void OnDisable()
        {
            Save();
        }

        static void CreateAndLoad()
        {
            System.Diagnostics.Debug.Assert(s_Instance == null);

            // Load
            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                // If a file exists the
                InternalEditorUtility.LoadSerializedFileAndForget(filePath);
            }

            if (s_Instance == null)
            {
                // Create
                var t = CreateInstance<SelectionGroupManager>();
                t.hideFlags = HideFlags.HideAndDontSave;
                s_Instance = t;
            }

            System.Diagnostics.Debug.Assert(s_Instance != null);
        }

        public void Save()
        {
            InternalEditorUtility.SaveToSerializedFileAndForget(new[] { s_Instance }, GetFilePath(), true);
        }

        static string GetFilePath()
        {
            string filePath = "Library" + "/" + "SelectionGroups.asset";
            return filePath;
        }
    }
}
