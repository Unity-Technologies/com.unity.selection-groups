
namespace Unity.SelectionGroups.Tests 
{
internal static class SelectionGroupTestsUtility {
    
    internal static SelectionGroupManager GetAndInitGroupManager() {
        SelectionGroupManager groupManager = SelectionGroupManager.GetOrCreateInstance();
        groupManager.ClearGroups();
        return groupManager;
    }
}

} //end namespace