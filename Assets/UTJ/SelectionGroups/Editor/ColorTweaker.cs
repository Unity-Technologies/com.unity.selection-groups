using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Utj.Film
{
    public class ColorTweaker : MacroEditor
    {

        List<Material> materials = new List<Material>();

        public override void OnSelectionChange()
        {
            materials.Clear();
            foreach (var go in Selection.gameObjects)
            {
                var renderer = go.GetComponent<Renderer>();
                if (renderer != null)
                {
                    materials.AddRange(renderer.sharedMaterials);
                }
            }
        }

        public override bool IsValidForSelection => materials.Count > 0;

        public override void OnGUI()
        {
            if (materials.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Saturation:");
                if (GUILayout.Button("+"))
                {
                    IncreaseSaturation();
                }
                if (GUILayout.Button("-"))
                {
                    DecreaseSaturation();
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Brightness:");
                if (GUILayout.Button("+"))
                {
                    IncreaseBrightness();
                }
                if (GUILayout.Button("-"))
                {
                    DecreaseBrightness();
                }
                GUILayout.EndHorizontal();
            }
        }

        void DecreaseSaturation()
        {
            foreach (var m in materials)
            {
                Undo.RecordObject(m, "Decrease Saturation");
                Color.RGBToHSV(m.color, out float h, out float s, out float v);
                s -= 0.05f;
                m.color = Color.HSVToRGB(h, s, v);
            }
        }

        void IncreaseSaturation()
        {
            foreach (var m in materials)
            {
                Undo.RecordObject(m, "Increase Saturation");
                Color.RGBToHSV(m.color, out float h, out float s, out float v);
                s += 0.05f;
                m.color = Color.HSVToRGB(h, s, v);
            }
        }

        void DecreaseBrightness()
        {
            foreach (var m in materials)
            {
                Undo.RecordObject(m, "Decrease Brightness");
                Color.RGBToHSV(m.color, out float h, out float s, out float v);
                v -= 0.05f;
                m.color = Color.HSVToRGB(h, s, v);
            }
        }

        void IncreaseBrightness()
        {
            foreach (var m in materials)
            {
                Undo.RecordObject(m, "Increase Brightness");
                Color.RGBToHSV(m.color, out float h, out float s, out float v);
                v += 0.05f;
                m.color = Color.HSVToRGB(h, s, v);
            }
        }
    }
}