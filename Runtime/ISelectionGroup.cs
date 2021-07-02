using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    
    internal interface ISelectionGroupContainer : IEnumerable<ISelectionGroup>
    {
        
    }

    internal interface ISelectionGroup
    {
        /// <summary>
        /// Sets/gets the name of the SelectionGroup
        /// </summary>
        string Name { get; set;  }
        /// <summary>
        /// Sets/gets the query which will automatically include GameObjects from the hierarchy that match the query into the group.
        /// </summary>
        string Query { get; set;  }

        /// <summary>
        /// Sets/gets the color of the SelectionGroup 
        /// </summary>
        Color Color { get; set; }
        
        /// <summary>
        /// Sets/gets the tools enabled in the SelectionGroups window
        /// </summary>
        HashSet<string> EnabledTools { get; set; }
        
        /// <summary>
        /// Sets/gets the data location of the SelectionGroup
        /// </summary>
        SelectionGroupDataLocation Scope { get; set; }

        /// <summary>
        /// Gets the number of members in this SelectionGroup
        /// </summary>
        int Count { get; }

        
        /// <summary>
        /// Sets/gets the members visibility in the SelectionGroups window
        /// </summary>
        bool ShowMembers { get; set; }
        
        /// <summary>
        /// Get the members of the SelectionGroup
        /// </summary>
        IList<Object> Members { get; }
        
        /// <summary>
        /// Adds a list of objects to the SelectionGroup 
        /// </summary>
        /// <param name="objects">A list of objects to be added</param>
        void Add(IList<Object> objects);

        /// <summary>
        /// Removes a list of objects from the SelectionGroup 
        /// </summary>
        /// <param name="objects">A list of objects to be removed</param>
        void Remove(IList<Object> objects);
        
        /// <summary>
        /// Clears the members of the SelectionGroup
        /// </summary>
        void Clear();

        /// <summary>
        /// Clears and set the members of the SelectionGroup 
        /// </summary>
        /// <param name="objects">A list of objects to be added</param>
        void SetMembers(IList<Object> objects);
    }
}