using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    
    internal interface ISelectionGroupContainer : IEnumerable<ISelectionGroup>
    {
        
    }

    internal interface ISelectionGroup
    {
        string Name { get; set;  }
        /// <summary>
        /// A query which will automatically include GameObjects from the hierarchy that match the query into the group.
        /// </summary>
        string Query { get; set;  }

        /// <summary>
        /// The color of the SelectionGroup 
        /// </summary>
        Color Color { get; set; }
        
        /// <summary>
        /// Tools enabled in the SelectionGroups window
        /// </summary>
        HashSet<string> EnabledTools { get; set; }
        
        /// <summary>
        /// The data location of the SelectionGroup
        /// </summary>
        SelectionGroupDataLocation Scope { get; set; }

        /// <summary>
        /// Gets the number of members in this SelectionGroup
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Show/Hide the members of the group in the SelectionGroups window
        /// </summary>
        bool ShowMembers { get; set; }
        
        /// <summary>
        /// Get the members of the SelectionGroup
        /// </summary>
        IList<Object> Members { get; }
        
        /// <summary>
        /// Adds a list of objects to the SelectionGroup 
        /// </summary>
        /// <param name="objectReferences">A list of objects to be added</param>
        void Add(IList<Object> objectReferences);

        /// <summary>
        /// Removes a list of objects from the SelectionGroup 
        /// </summary>
        /// <param name="objectReferences">A list of objects to be removed</param>
        void Remove(IList<Object> objectReferences);
        
        /// <summary>
        /// Clears the members of the SelectionGroup
        /// </summary>
        void Clear();
        void SetMembers(IList<Object> objects);
    }
}