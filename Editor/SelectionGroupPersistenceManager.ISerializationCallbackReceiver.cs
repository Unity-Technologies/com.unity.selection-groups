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

    //[TODO-sin:2021-12-20] Remove in version 0.7.0 
    // [InitializeOnLoad]
    // internal partial class SelectionGroupPersistenceManager: ISerializationCallbackReceiver
    // {
    //     //[TODO-sin:2021-12-20] Remove in version 0.7.0
    //     //[SerializeField] EditorSelectionGroup[] _values;
    //
    //     string[] _names;
    //
    //     static SelectionGroupPersistenceManager _instance;
    //
    //     internal static SelectionGroupPersistenceManager Instance
    //     {
    //         get
    //         {
    //             if (_instance == null) CreateAndLoad();
    //             return _instance;
    //         }
    //         set { _instance = value; }
    //     }
    //
    //     static SelectionGroupPersistenceManager()
    //     {
    //         EditorApplication.delayCall += CreateAndLoad;
    //     }
    //
    //     private static void CreateAndLoad()
    //     {
    //
    //         System.Diagnostics.Debug.Assert(_instance == null);
    //         //Find existing
    //         var managers = Resources.FindObjectsOfTypeAll(typeof(SelectionGroupPersistenceManager));
    //         if (managers.Length > 0)
    //         {
    //             _instance = managers[0] as SelectionGroupPersistenceManager;
    //             if (managers.Length != 1)
    //             {
    //                 Debug.LogError($"Multiple SelectionGroupManager instances detected! {managers.Length}");
    //             }
    //         }
    //
    //         if (_instance == null)
    //         {
    //             // Load
    //             string filePath = GetFilePath();
    //             if (!string.IsNullOrEmpty(filePath))
    //             {
    //                 // If a file exists the
    //                 var objects = InternalEditorUtility.LoadSerializedFileAndForget(filePath);
    //                 if (objects.Length > 0 && objects[0] is SelectionGroupPersistenceManager)
    //                 {
    //                     _instance = (SelectionGroupPersistenceManager)objects[0];
    //                 }
    //             }
    //         }
    //
    //         if (_instance == null)
    //         {
    //             // Create
    //             var t = CreateInstance<SelectionGroupPersistenceManager>();
    //             // t.hideFlags = HideFlags.HideAndDontSave;
    //             _instance = t;
    //         }
    //         
    //         System.Diagnostics.Debug.Assert(_instance != null);
    //         _instance.hideFlags = HideFlags.HideAndDontSave;
    //         Instance = _instance;
    //     }
    //
    //     private static void Save()
    //     {
    //         if (Instance != null)
    //         {
    //             var path = GetFilePath();
    //             System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
    //             InternalEditorUtility.SaveToSerializedFileAndForget(new[] { Instance }, path, true);
    //         }
    //     }
    //
    //     static string GetFilePath()
    //     {
    //         string filePath = "SelectionGroups" + "/" + "SelectionGroups.asset";
    //         return filePath;
    //     }
    //
    //     public void OnBeforeSerialize()
    //     {
    //         //[TODO-sin:2021-12-20] Remove in version 0.7.0
    //         //_values = editorGroups.ToArray();
    //     }
    //
    //     public void OnAfterDeserialize()
    //     {
    //         //[TODO-sin:2021-12-20] Remove in version 0.7.0
    //         //editorGroups.Clear();
    //         //editorGroups.UnionWith(_values);
    //         
    //         //[TODO-sin:2021-12-20] Remove in version 0.7.0
    //         // SelectionGroupManager.ClearEditorGroups();
    //         // foreach (var i in _values)
    //         //     SelectionGroupManager.Register(i);
    //     }
    // }
}