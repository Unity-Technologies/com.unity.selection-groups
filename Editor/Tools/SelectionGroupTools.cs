﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.SelectionGroups.Runtime;

namespace Unity.SelectionGroups
{
    /// <summary>
    /// This class containes methods which implement the default tools available for selection groups.
    /// </summary>
    public static class SelectionGroupTools
    {
        [SelectionGroupTool("d_VisibilityOn", "Toggle Visibility", "Show and hide objects in the scene.",0)]
        static void ToggleVisibility(ISelectionGroup group)
        {
            foreach (var g in group.Members)
            {
                var go = g as GameObject;
                if (go == null) continue;
                SceneVisibilityManager.instance.ToggleVisibility(go, false);
            }
        }

        [SelectionGroupTool("LockIcon-On", "Toggle Lock", "Enable and disable editing of objects.",1)]
        static void DisableEditing(ISelectionGroup group)
        {
            var isLocked = false;
            foreach (var g in group.Members)
            {
                if (g.hideFlags.HasFlag(HideFlags.NotEditable))
                    isLocked = true;
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
