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
        public bool edit;
    }
}