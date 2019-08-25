using System.Collections.Generic;
using UnityEngine;


namespace Utj.Film
{
    [System.Serializable]
    public struct SelectionGroup
    {
        public string groupName;
        public Color color;
        public List<Object> objects;
        public List<Object> queryResults;
        public bool edit;
        public bool isLightGroup;
        public string lightGroupName;
        public bool showMembers;
        public DynamicSelectionQuery selectionQuery;
        public Object[] attachments;
    }

}