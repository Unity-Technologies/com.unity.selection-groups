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
        public bool isLightGroup;
        public string lightGroupName;
        public DynamicSelectionQuery selectionQuery;

        public IEnumerable<T> GetComponents<T>() where T : Component
        {
            foreach (var i in objects)
            {
                if (i is T) yield return (T)i;
                if (i is GameObject)
                {
                    foreach (var j in ((GameObject)i).GetComponents<T>())
                        yield return j;
                }
            }
        }
    }

}