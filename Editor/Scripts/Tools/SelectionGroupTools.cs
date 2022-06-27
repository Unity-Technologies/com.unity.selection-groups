﻿using System.Collections.Generic;
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
            IList<GameObject> goMembers  = group.Members;
            int               numMembers = goMembers.Count;
            if (numMembers <= 0)
                return;

            GameObject[] members = new GameObject[numMembers];
            group.CopyTo(members,0);

            SceneVisibilityManager sceneVisibilityManager = SceneVisibilityManager.instance;
            bool show = (sceneVisibilityManager.IsHidden(members[0]));
            if (show) {
                sceneVisibilityManager.Show(members, false);
            } else {
                sceneVisibilityManager.Hide(members, false);
            }
        }

        [SelectionGroupTool("LockIcon-On", "Enable or disable editing of objects.",(int) SelectionGroupToolType.Lock)]
        static void ToggleLock(SelectionGroup group) 
        {
            IList<GameObject> members  = group.Members;
            if (null == members || members.Count <= 0)
                return;
            
            bool isLocked = members[0].hideFlags.HasFlag(HideFlags.NotEditable);
            if (isLocked)
            {
                foreach (GameObject obj in group.Members)
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
