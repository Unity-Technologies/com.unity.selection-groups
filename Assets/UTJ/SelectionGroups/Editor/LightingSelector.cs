using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Utj.Film
{
    public class LightingSelector : MacroEditor
    {
        HashSet<string> layerNames = new HashSet<string>();


        public override void OnSelectionChange()
        {
            layerNames.Clear();
            foreach (var g in Selection.gameObjects)
            {
                layerNames.Add(LayerMask.LayerToName(g.layer));
            }
        }

        public override void OnGUI()
        {
            if (layerNames.Count > 1)
            {
                EditorGUILayout.HelpBox("The selection has multiple layers.", MessageType.Info);
                foreach (var i in layerNames)
                {
                    if (GUILayout.Button($"Set Group to Layer: {i}"))
                    {
                        foreach (var g in Selection.gameObjects)
                        {
                            g.layer = LayerMask.NameToLayer(i);
                        }
                        EditorApplication.delayCall += OnSelectionChange;
                    }
                }
            }
            foreach (var i in SelectionGroups.Instance.groups)
            {
                if (i.isLightGroup)
                {
                    if (GUILayout.Button(i.groupName))
                    {
                        foreach (var g in Selection.gameObjects)
                        {
                            var sgm = g.GetComponent<SelectionGroupMember>();
                            if (sgm == null) sgm = g.AddComponent<SelectionGroupMember>();
                            sgm.lights = i.GetComponents<Light>().ToArray();

                        }
                    }
                }
            }
        }
    }
}