﻿using System.Collections;
using System.Collections.Generic;
using Unity.SelectionGroups;
using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEngine;

namespace Unity.SelectionGroupsEditor
{
    //[TODO-sin:2021-12-20] Remove in version 0.7.0 
    // internal partial class EditorSelectionGroup
    // {
    //     /// <summary>
    //     /// Number of objects in this group that are available to be referenced. (Ie. they exist in a loaded scene)
    //     /// </summary>
    //     public int Count => PersistentReferenceCollection.LoadedObjectCount;
    //
    //     public bool ShowMembers { get; set; }
    //
    //     public string Name
    //     {
    //         get => name;
    //         set => name = value;
    //     }
    //
    //     public string Query
    //     {
    //         get => query;
    //         set => query = value;
    //     }
    //
    //     public Color Color
    //     {
    //         get => color;
    //         set => color = value;
    //     }
    //
    //     public HashSet<string> EnabledTools
    //     {
    //         get => enabledTools;
    //         set => enabledTools = value;
    //     }
    //
    //     public SelectionGroupDataLocation Scope
    //     {
    //         get => SelectionGroupDataLocation.Editor;
    //     }
    //
    //     public void OnDelete(ISelectionGroup group)
    //     {
    //         
    //     }
    //
    //     public void Add(IList<Object> objectReferences)
    //     {
    //         foreach(var i in objectReferences) {
    //             var go = i as GameObject;
    //             if(go != null && string.IsNullOrEmpty(go.scene.path)) {
    //                 //GameObject is not saved into a scene, therefore it cannot be stored in a editor selection group.
    //                 //TODO: work out better way to notify user of this exception.
    //                 // throw new SelectionGroupException("Cannot add an Object from an unsaved scene.");
    //             } 
    //             PersistentReferenceCollection.Add(i);    
    //         }
    //     }
    //     
    //     public void SetMembers(IList<Object> objects)
    //     {
    //         PersistentReferenceCollection.Clear();
    //         Add(objects);
    //     }
    //
    //     public void Remove(IList<Object> objectReferences)
    //     {
    //         PersistentReferenceCollection.Remove(objectReferences);
    //     }
    //
    //     public void Clear()
    //     {
    //         PersistentReferenceCollection.Clear();
    //     }
    //
    // }
}