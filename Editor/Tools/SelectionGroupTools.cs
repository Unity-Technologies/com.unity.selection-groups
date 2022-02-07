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
        [SelectionGroupTool("d_VisibilityOn", "Show and hide objects in the scene.",(int) SelectionGroupToolType.VISIBILITY)]
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

        [SelectionGroupTool("LockIcon-On", "Enable and disable editing of objects.",(int) SelectionGroupToolType.LOCK)]
        static void DisableEditing(SelectionGroup group)
        {
            var isLocked = false;
            foreach (var g in group.Members) {
                if (!g.hideFlags.HasFlag(HideFlags.NotEditable)) 
                    continue;
                isLocked = true;
                break;
            }
            if (isLocked)
            {
                foreach (var g in group.Members)
                    g.hideFlags &= ~HideFlags.NotEditable;
            }
            else
            {
                foreach (var g in group.Members)
                    g.hideFlags |= HideFlags.NotEditable;
            }
        }

    }
}
