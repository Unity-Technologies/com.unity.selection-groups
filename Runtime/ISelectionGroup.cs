using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups
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
        string Query { get; }

        /// <summary>
        /// Gets whether the group is automatically filled
        /// </summary>
        bool IsAutoFilled();
        
        /// <summary>
        /// Sets/gets the color of the SelectionGroup 
        /// </summary>
        Color Color { get; set; }
        
        /// <summary>
        /// Gets the number of members in this SelectionGroup
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Get the members of the SelectionGroup
        /// </summary>
        IList<Object> Members { get; }
        
        /// <summary>
        /// Adds a list of objects to the SelectionGroup 
        /// </summary>
        /// <param name="objects">A list of objects to be added</param>
        void Add(IEnumerable<Object> objects);

        /// <summary>
        /// Removes a list of objects from the SelectionGroup 
        /// </summary>
        /// <param name="objects">A list of objects to be removed</param>
        void Remove(IEnumerable<Object> objects);
        
        /// <summary>
        /// Clears the members of the SelectionGroup
        /// </summary>
        void Clear();

        /// <summary>
        /// Clears and set the members of the SelectionGroup 
        /// </summary>
        /// <param name="objects">A enumerable collection of objects to be added</param>
        void SetMembers(IEnumerable<Object> objects);
    }
}