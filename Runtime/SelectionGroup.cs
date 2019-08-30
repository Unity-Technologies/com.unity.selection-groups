using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Unity.SelectionGroups
{
    [System.Serializable]
    public struct SelectionGroup : ISerializationCallbackReceiver
    {
        public string groupName;
        public Color color;
        public HashSet<GameObject> objects;
        public bool showMembers;
        public DynamicSelectionQuery selectionQuery;
        public HashSet<GameObject> queryResults;
        public List<Object> attachments;
        public Rect rect;

        [SerializeField] GameObject[] _objects;

        public void OnAfterDeserialize()
        {
            objects = new HashSet<GameObject>(_objects ?? new GameObject[0]);
        }

        public void OnBeforeSerialize()
        {
            if (objects != null)
                _objects = objects.ToArray();
        }

        public void ClearQueryResults()
        {
            if (queryResults != null) queryResults.Clear();
        }
    }

}