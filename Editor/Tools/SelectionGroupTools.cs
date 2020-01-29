using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.SelectionGroups
{
    public static class SelectionGroupTools
    {
        [SelectionGroupTool("d_VisibilityOn", "Toggle Visibility")]
        static void ToggleVisibility(SelectionGroup group)
        {
            foreach (var g in group)
            {
                var go = g as GameObject;
                if (go == null) continue;
                SceneVisibilityManager.instance.ToggleVisibility(go, false);
            }
        }

        [SelectionGroupTool("LockIcon-On", "Lock")]
        static void DisableEditing(SelectionGroup group)
        {
            foreach (var g in group)
            {
                g.hideFlags |= HideFlags.NotEditable;
            }
        }

        [SelectionGroupTool("LockIcon", "Unlock")]
        static void EnableEditing(SelectionGroup group)
        {
            foreach (var g in group)
            {
                g.hideFlags &= ~HideFlags.NotEditable;
            }
        }

    }
}
