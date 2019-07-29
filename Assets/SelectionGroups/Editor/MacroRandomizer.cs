using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MacroRandomizer : MacroEditor
{

    List<Material> materials = new List<Material>();
    List<Transform> transforms = new List<Transform>();

    public override void OnSelectionChange()
    {
        materials.Clear();
        transforms.Clear();
        foreach (var go in Selection.gameObjects)
        {
            transforms.Add(go.transform);
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                materials.AddRange(renderer.sharedMaterials);
            }
        }
    }

    public override bool IsValidForSelection => materials.Count > 0 || transforms.Count > 0;

    public override void OnGUI()
    {
        if (materials.Count > 0)
        {
            if (GUILayout.Button("Randomize Colors"))
            {
                RandomizeColors();
            }
            if (GUILayout.Button("Mutate Colors"))
            {
                MutateColors();
            }
        }
        if (transforms.Count > 0)
        {
            if (GUILayout.Button("Randomize Rotations"))
            {
                RandomizeRotations();
            }
            if (GUILayout.Button("Mutate Rotations"))
            {
                MutateRotations();
            }
        }
    }

    void RandomizeColors()
    {
        foreach (var m in materials)
        {
            Undo.RecordObject(m, "Randomize Colors");
            Color.RGBToHSV(m.color, out float h, out float s, out float v);
            h = Random.value;
            m.color = Color.HSVToRGB(h, s, v);
        }
    }

    void MutateColors()
    {
        foreach (var m in materials)
        {
            Undo.RecordObject(m, "Mutate Colors");
            Color.RGBToHSV(m.color, out float h, out float s, out float v);
            h += Random.Range(-0.05f, +0.05f);
            m.color = Color.HSVToRGB(h, s, v);
        }
    }

    void RandomizeRotations()
    {
        foreach (var t in transforms)
        {
            Undo.RecordObject(t, "Randomize Rotation");
            t.rotation = Random.rotation;
        }
    }

    void MutateRotations()
    {
        foreach (var t in transforms)
        {
            Undo.RecordObject(t, "Mutate Rotation");
            t.rotation = Quaternion.Slerp(t.rotation, Random.rotation, 0.05f);
        }
    }


}