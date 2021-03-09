﻿using Unity.SelectionGroups.Runtime;
using UnityEditor;

namespace Unity.SelectionGroupsEditor
{
    class SelectionGroupDebugInformation
    {

        public string text;

        internal SelectionGroupDebugInformation(ISelectionGroup group)
        {
            text = EditorJsonUtility.ToJson(group, prettyPrint: true);
        }
    }
}