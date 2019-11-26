using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Unity.SelectionGroups
{
    public enum VisibilityMode
    {
        Disabled,
        Enabled
    }

    public enum MutabilityMode
    {
        Disabled,
        Enabled
    }

    [System.Serializable]
    public class SelectionGroup : MonoBehaviour, ISerializationCallbackReceiver
    {
        public Color color;
        public HashSet<GameObject> objects;
        public bool showMembers;
        public DynamicSelectionQuery selectionQuery;
        public HashSet<GameObject> queryResults;
        public List<Object> attachments;
        public VisibilityMode visibility;
        public MutabilityMode mutability;

        public GameObject[] sz_objects;

        public void OnAfterDeserialize()
        {
            objects = new HashSet<GameObject>(sz_objects ?? new GameObject[0]);
        }

        public void OnBeforeSerialize()
        {
            if (objects != null)
                sz_objects = objects.ToArray();
        }

        public void ClearQueryResults()
        {
            if (queryResults != null) queryResults.Clear();
        }
    }
}