using UnityEditor;
using UnityEngine;


namespace Unity.SelectionGroups
{
    public static class SelectionGroupEditorUtility
    {
        public static void ShowGroup(string groupName)
        {
            SceneVisibilityManager.instance.Show(SelectionGroupUtility.GetMembers(groupName).ToArray(), false);
        }

        public static void HideGroup(string groupName)
        {
            SceneVisibilityManager.instance.Hide(SelectionGroupUtility.GetMembers(groupName).ToArray(), false);
        }

        public static void LockGroup(string groupName)
        {
            foreach (var go in SelectionGroupUtility.GetMembers(groupName))
                go.hideFlags |= HideFlags.NotEditable;
        }

        public static void UnlockGroup(string groupName)
        {
            foreach (var go in SelectionGroupUtility.GetMembers(groupName))
                go.hideFlags &= ~HideFlags.NotEditable;
        }

        public static bool AreAnyMembersLocked(string groupName)
        {
            foreach (var go in SelectionGroupUtility.GetMembers(groupName))
            {
                if (go.hideFlags.HasFlag(HideFlags.NotEditable))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool AreAnyMembersHidden(string groupName)
        {
            foreach (var go in SelectionGroupUtility.GetMembers(groupName))
            {
                if(SceneVisibilityManager.instance.IsHidden(go)) {
                    return true;
                }
            }
            return false;
        }

    }
}
