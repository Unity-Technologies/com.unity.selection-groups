using System.Collections;
using System.Collections.Generic;
using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEngine;

namespace Unity.SelectionGroups
{
    public partial class SelectionGroup : ISelectionGroup
    {
        /// <summary>
        /// Number of objects in this group that are available to be referenced. (Ie. they exist in a loaded scene)
        /// </summary>
        public int Count => PersistentReferenceCollection.LoadedObjectCount;

        public bool ShowMembers { get; set; }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public string Query
        {
            get => query;
            set => query = value;
        }

        public Color Color
        {
            get => color;
            set => color = value;
        }

        public HashSet<string> EnabledTools
        {
            get => enabledTools;
            set => enabledTools = value;
        }

        public SelectionGroupScope Scope
        {
            get => scope;
            set => scope = value;
        }

        public int GroupId
        {
            get => groupId;
            set => groupId = value;
        }

        public void OnCreate(SelectionGroupScope sender, int groupId, string name, string query, Color color, IList<Object> members)
        {
            if (groupId != this.groupId) return;
        }

        public void OnUpdate(SelectionGroupScope sender, int groupId, string name, string query, Color color, IList<Object> members)
        {
            if (groupId != this.groupId) return;
            this.name = name;
            this.query = query;
            this.color = color;
            Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Update");
            PersistentReferenceCollection.Clear();
            OnAdd(sender, groupId, members);
        }

        public void OnDelete(SelectionGroupScope sender, int groupId)
        {
            if (groupId != this.groupId) return;
        }

        public void OnAdd(SelectionGroupScope sender, int groupId, IList<Object> members)
        {
            if (groupId != this.groupId) return;
            foreach(var i in members) {
                var go = i as GameObject;
                if(go != null && string.IsNullOrEmpty(go.scene.path)) {
                    //GameObject is not saved into a scene, therefore it cannot be stored in a selection group.
                    // throw new SelectionGroupException("Cannot add a gameobject from an unsaved scene.");
                    
                    return;
                }
            }
            Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Add");                        
            PersistentReferenceCollection.Add(members);
        }

        public void OnRemove(SelectionGroupScope sender, int groupId, IList<Object> members)
        {
            if (groupId != this.groupId) return;
            Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Remove");
            PersistentReferenceCollection.Remove(members);
        }

        /// <summary>
        /// Enumerator for all members of this group.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Object> GetEnumerator()
        {
            PersistentReferenceCollection.LoadObjects();
            return PersistentReferenceCollection.GetEnumerator();
        }
        /// <summary>
        /// Enumerable for all members of this group.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            PersistentReferenceCollection.LoadObjects();
            return PersistentReferenceCollection.GetEnumerator();
        }
    }
}