using System;
using UnityEditor;

namespace Unity.SelectionGroups
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SelectionGroupToolAttribute : Attribute
    {
        public string icon;
        public string text;

        public SelectionGroupToolAttribute(string icon, string text)
        {
            this.icon = icon;
            this.text = text;
        }
    }
}