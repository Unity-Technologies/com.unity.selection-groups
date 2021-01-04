using System;
using UnityEditor;

namespace Unity.SelectionGroups
{
    /// <summary>
    /// Marks a method to be included as a SelectionGroup tool, which is enabled in the configuration dialog for a selection group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SelectionGroupToolAttribute : Attribute
    {
        /// <summary>
        /// The icon used to represent the tool in the editor window.
        /// </summary>
        public string icon;
        /// <summary>
        /// The short name for the tool.
        /// </summary>
        public string text;
        /// <summary>
        /// The longer description of the tool.
        /// </summary>
        public string description;
        /// <summary>
        /// An automatically generated unique ID for the tool
        /// </summary>
        public readonly string toolId;

        /// <summary>
        /// Create the attribute with icon and text and descrption.
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        /// <param name="description"></param>
        public SelectionGroupToolAttribute(string icon, string text, string description)
        {
            this.icon = icon;
            this.text = text;
            this.description = description;
            this.toolId = $"{icon}.{text}.{description}";
        }
    }
}