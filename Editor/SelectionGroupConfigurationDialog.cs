using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.SelectionGroups
{
    public class SelectionGroupConfigurationDialog : EditorWindow
    {
        string groupName;
        SelectionGroup group;
        ReorderableList materialList;
        ReorderableList typeList;
        ReorderableList shaderList;
        ReorderableList attachmentList;

        public static void Open(string groupName)
        {
            var dialog = EditorWindow.GetWindow<SelectionGroupConfigurationDialog>();
            dialog.ShowPopup();
            dialog.Configure(groupName);
        }

        void Configure(string groupName)
        {
            this.groupName = groupName;
            group = SelectionGroupUtility.GetFirstGroup(groupName);
            titleContent.text = "Selection Group Settings";
        }

        void OnGUI()
        {
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                GUILayout.BeginVertical("box", GUILayout.Width(position.width));
                group.groupName = EditorGUILayout.DelayedTextField("Group Name", group.groupName);
                group.color = EditorGUILayout.ColorField("Group Color", group.color);
                GUILayout.Space(EditorGUIUtility.singleLineHeight);
                
                group.mutability = (MutabilityMode) EditorGUILayout.EnumPopup("Lock/Unlock Tool", group.mutability);
                group.visibility = (VisibilityMode) EditorGUILayout.EnumPopup("Show/Hide Tool", group.visibility);
                GUILayout.Space(EditorGUIUtility.singleLineHeight);
                GUILayout.EndVertical();
                GUILayout.BeginVertical("box", GUILayout.Width(position.width));
                group.selectionQuery.enabled = GUILayout.Toggle(group.selectionQuery.enabled, "Enable Selection Query");
                EditorGUILayout.Space();
                if (group.selectionQuery.enabled)
                {
                    group.selectionQuery.nameQuery = EditorGUILayout.TextField("Name Contains Query", group.selectionQuery.nameQuery);
                    EditorGUILayout.Space();

                    if (typeList == null)
                        typeList = BuildTypeList();
                    typeList.DoLayoutList();
                    EditorGUILayout.Space();

                    if (materialList == null)
                        materialList = BuildMaterialList();
                    materialList.DoLayoutList();
                    EditorGUILayout.Space();

                    if (shaderList == null)
                        shaderList = BuildShaderList();
                    shaderList.DoLayoutList();
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical("box", GUILayout.Width(position.width));
                if (attachmentList == null)
                    attachmentList = BuildAttachmentList();
                attachmentList.DoLayoutList();
                GUILayout.EndVertical();
                if (cc.changed)
                {
                    SelectionGroupEditorWindow.UndoRecordObject("Configure group settings");
                    SelectionGroupUtility.UpdateGroup(groupName, group);
                    SelectionGroupEditorWindow.MarkAllContainersDirty();
                }
            }
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
            {
                SelectionGroupEditorWindow.UndoRecordObject("Configure group settings");
                SelectionGroupUtility.UpdateGroup(groupName, group);
                SelectionGroupEditorWindow.MarkAllContainersDirty();
                Close();
            }
        }

        ReorderableList BuildMaterialList()
        {
            if(group.selectionQuery.requiredMaterials == null) group.selectionQuery.requiredMaterials = new List<Material>();
            var materialList = new ReorderableList(group.selectionQuery.requiredMaterials, typeof(Material));
            materialList.onAddCallback = (list) => list.list.Add(null);
            materialList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.x += 8;
                rect.width -= 16;
                rect.y += (rect.height - EditorGUIUtility.singleLineHeight) * 0.25f;
                rect.height = EditorGUIUtility.singleLineHeight;
                var material = group.selectionQuery.requiredMaterials[index];
                material = (Material)EditorGUI.ObjectField(rect, material, typeof(Material), false);
                group.selectionQuery.requiredMaterials[index] = material;
            };
            materialList.draggable = false;
            materialList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Required Materials");
            return materialList;
        }

        ReorderableList BuildShaderList()
        {
            if(group.selectionQuery.requiredShaders == null) group.selectionQuery.requiredShaders = new List<Shader>();
            var shaderList = new ReorderableList(group.selectionQuery.requiredShaders, typeof(Shader));
            shaderList.onAddCallback = (list) => list.list.Add(null);
            shaderList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.x += 8;
                rect.width -= 16;
                rect.y += (rect.height - EditorGUIUtility.singleLineHeight) * 0.25f;
                rect.height = EditorGUIUtility.singleLineHeight;
                var shader = group.selectionQuery.requiredShaders[index];
                shader = (Shader)EditorGUI.ObjectField(rect, shader, typeof(Shader), false);
                group.selectionQuery.requiredShaders[index] = shader;
            };
            shaderList.draggable = false;
            shaderList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Required Shaders");
            return shaderList;
        }

        ReorderableList BuildTypeList()
        {
            if(group.selectionQuery.requiredTypes == null) group.selectionQuery.requiredTypes = new List<string>();
            var typeList = new ReorderableList(group.selectionQuery.requiredTypes, typeof(string));
            typeList.onAddCallback = (list) => list.list.Add(string.Empty);
            typeList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.x += 8;
                rect.width -= 16;
                rect.y += (rect.height - EditorGUIUtility.singleLineHeight) * 0.25f;
                rect.height = EditorGUIUtility.singleLineHeight;
                var typeName = group.selectionQuery.requiredTypes[index];
                typeName = EditorGUI.TextField(rect, typeName);
                group.selectionQuery.requiredTypes[index] = typeName;
            };
            typeList.draggable = false;
            typeList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Required Types");
            return typeList;
        }

        ReorderableList BuildAttachmentList()
        {
            if(group.attachments == null) group.attachments = new List<Object>();
            var attachmentList = new ReorderableList(group.attachments, typeof(Object));
            attachmentList.onAddCallback = (list) => list.list.Add(null);
            attachmentList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.x += 8;
                rect.width -= 16;
                rect.y += (rect.height - EditorGUIUtility.singleLineHeight) * 0.25f;
                rect.height = EditorGUIUtility.singleLineHeight;
                var obj = group.attachments[index];
                obj = EditorGUI.ObjectField(rect, obj, typeof(UnityEngine.Object), false);
                group.attachments[index] = obj;
            };
            attachmentList.draggable = false;
            attachmentList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Attachments");
            return attachmentList;
        }

    }
}
