using UnityEngine;

namespace Unity.SelectionGroups.Editor
{
    internal class GroupMember
    {
        public readonly SelectionGroup group;
        public readonly GameObject gameObject;

        public GroupMember(SelectionGroup selectionGroup, GameObject groupGameObject)
        {
            this.group = selectionGroup;
            this.gameObject = groupGameObject;
        }
    }
}