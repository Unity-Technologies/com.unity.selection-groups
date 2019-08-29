using System.Collections.Generic;
using UnityEngine;


namespace Unity.SelectionGroups
{
    [System.Serializable]
    public struct SelectionGroup
    {
        public string groupName;
        public Color color;
        public List<GameObject> objects;
        public List<GameObject> queryResults;
        public bool edit;
        public bool isLightGroup;
        public string lightGroupName;
        public bool showMembers;
        public DynamicSelectionQuery selectionQuery;
        public Object[] attachments;
        public Rect rect;
    }

}