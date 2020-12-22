using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.SelectionGroups.Runtime;
using UnityEngine.SceneManagement;

namespace Unity.SelectionGroups
{
    /// <summary>
    /// This class is the Editor-only container for selection group information and members.
    /// </summary>
    [System.Serializable]
    public partial class SelectionGroup : ISelectionGroup
    {
        /// <summary>
        /// The name of the group.
        /// </summary>
        public string name;
        /// <summary>
        /// A color assigned to the group.
        /// </summary>
        public Color color;
        /// <summary>
        /// Should this group expand the list of members in the UI.
        /// </summary>
        public bool showMembers;

        /// <summary>
        /// A GoQL query string which will populate the group with matching members.
        /// </summary>
        public string query = string.Empty;

        /// <summary>
        /// Unique ID of the group.
        /// </summary>
        public int groupId;

        public SelectionGroupScope scope = SelectionGroupScope.Editor;
        public Scene scene;

        /// <summary>
        /// Number of objects in this group that are available to be referenced. (Ie. they exist in a loaded scene)
        /// </summary>
        public int Count => PersistentReferenceCollection.LoadedObjectCount;

        public bool ShowMembers { get; set; }

        /// <summary>
        /// Number of objects that exist in this group, including objects that cannot be loaded. (Ie. The containing scene has not been loaded)
        /// </summary>
        internal int TotalCount => PersistentReferenceCollection.TotalObjectCount;

        /// <summary>
        /// Access by index the loaded objects in this group.
        /// </summary>
        /// <value></value>
        internal Object this[int index] { get => PersistentReferenceCollection[index]; set => PersistentReferenceCollection[index] = value; }

        internal HashSet<string> enabledTools = new HashSet<string>();

        [SerializeField] PersistentReferenceCollection _persistentReferenceCollection;
        PersistentReferenceCollection PersistentReferenceCollection
        {
            get
            {
                if (_persistentReferenceCollection == null)
                {
                    _persistentReferenceCollection = new PersistentReferenceCollection();
                    _persistentReferenceCollection.LoadObjects();
                }
                return _persistentReferenceCollection;
            }
        }

        GoQL.GoQLExecutor executor = new GoQL.GoQLExecutor();


        public string Name
        {
            get => name;
            set => name = value;
        }

        string ISelectionGroup.Query
        {
            get => query;
            set => query = value;
        }

        Color ISelectionGroup.Color
        {
            get => color;
            set => color = value;
        }
        public HashSet<string> EnabledTools { 
            get => enabledTools; 
            set =>enabledTools = value; 
        }

        public SelectionGroupScope Scope
        {
            get => scope; 
            set => scope = value;
        }
        public int GroupId { 
            get => groupId; 
            set => groupId = value; 
        }

        public void RefreshQueryResults()
        {
            if (string.IsNullOrEmpty(query)) return;
            executor.Code = query;
            var objects = executor.Execute();
            PersistentReferenceCollection.Update(objects);
        }

        /// <summary>
        /// Creates all references in this group that exist in a loaded scene.
        /// </summary>
        public void Reload() {
            PersistentReferenceCollection.LoadObjects(forceReload:true);
        }

        public void Clear()
        {
            PersistentReferenceCollection.Clear();
        }

        public void Remove(IList<Object> objects)
        {
            PersistentReferenceCollection.Remove(objects);
        }

        public void Add(IList<Object> objects)
        {
            foreach(var i in objects) {
                var go = i as GameObject;
                if(go != null && string.IsNullOrEmpty(go.scene.path)) {
                    //GameObject is not saved into a scene, therefore it cannot be stored in a selection group.
                    throw new SelectionGroupException("Cannot add a gameobject from an unsaved scene.");
                }
            }
            Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Add");                        
            PersistentReferenceCollection.Add(objects);
            SelectionGroupEvents.Update(SelectionGroupScope.Editor, groupId, name, query, color, this.ToArray());
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

        public void CopyToRuntimeGroup(Runtime.SelectionGroup sg)
        {
            sg.name = name;
            sg.members.Clear();
            sg.members.AddRange(this);
            sg.color = color;
            sg.query = query;
            sg.groupId = groupId;
        }
    }
}
