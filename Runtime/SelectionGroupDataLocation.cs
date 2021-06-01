namespace Unity.SelectionGroups
{
    /// <summary>
    /// Indicates where the SelectionGroup data is stored
    /// </summary>
    public enum SelectionGroupDataLocation
    {
        /// <summary>
        /// Stored in an asset file outside scenes
        /// </summary>
        Editor,
        
        /// <summary>
        /// Stored in a scene (default)
        /// </summary>
        Scene
    }
}