using System.Collections.Generic;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEngine;

namespace Unity.SelectionGroups.Editor
{
    /// <summary>
    /// This class containes methods which implement the default tools available for selection groups.
    /// </summary>
    public static class SelectionGroupTools
    {
        [SelectionGroupTool("d_VisibilityOn", "Show or hide objects in the scene.",(int) SelectionGroupToolType.Visibility)]
        static void ToggleVisibility(SelectionGroup group) 
        {
            List<GameObject> goMembers = group.FindGameObjectMembers();
            if (goMembers.Count <= 0)
                return;

            SceneVisibilityManager sceneVisibilityManager = SceneVisibilityManager.instance;
            bool show = (sceneVisibilityManager.IsHidden(goMembers[0]));
            if (show) {
                sceneVisibilityManager.Show(goMembers.ToArray(), false);
            } else {
                sceneVisibilityManager.Hide(goMembers.ToArray(), false);
            }
        }

        [SelectionGroupTool("LockIcon-On", "Enable or disable editing of objects.",(int) SelectionGroupToolType.Lock)]
        static void ToggleLock(SelectionGroup group) 
        {
            IList<Object> members  = group.Members;
            if (null == members || members.Count <= 0)
                return;
            
            bool isLocked = members[0].hideFlags.HasFlag(HideFlags.NotEditable);
            if (isLocked)
            {
                foreach (Object obj in group.Members)
                    obj.hideFlags &= ~HideFlags.NotEditable;
            }
            else
            {
                foreach (Object obj in group.Members)
                    obj.hideFlags |= HideFlags.NotEditable;
            }
        }

    }
}
