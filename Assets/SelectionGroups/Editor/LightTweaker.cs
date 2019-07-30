using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class LightTweaker : AutoMacroEditor
{

    List<Light> lights = new List<Light>();

    public override void OnSelectionChange()
    {
        lights.Clear();
        foreach (var go in Selection.gameObjects)
        {
            var light = go.GetComponent<Light>();
            if (light != null)
            {
                lights.Add(light);
            }
        }
    }

    public override bool IsValidForSelection => lights.Count > 0;

    public void MacroDecreaseIntensity()
    {
        foreach (var i in lights)
        {
            Undo.RecordObject(i, "Decrease Intensity");
            i.intensity -= 0.05f;
        }
    }

    public void MacroIncreaseIntensity()
    {
        foreach (var i in lights)
        {
            Undo.RecordObject(i, "Increase Intensity");
            i.intensity += 0.05f;
        }
    }

    public void MacroDecreaseSaturation()
    {
        foreach (var i in lights)
        {
            Undo.RecordObject(i, "Decrease Saturation");
            Color.RGBToHSV(i.color, out float h, out float s, out float v);
            s -= 0.05f;
            i.color = Color.HSVToRGB(h, s, v);
        }
    }

    public void MacroIncreaseSaturation()
    {
        foreach (var i in lights)
        {
            Undo.RecordObject(i, "Increase Saturation");
            Color.RGBToHSV(i.color, out float h, out float s, out float v);
            s += 0.05f;
            i.color = Color.HSVToRGB(h, s, v);
        }
    }

    public void MacroDecreaseBrightness()
    {
        foreach (var i in lights)
        {
            Undo.RecordObject(i, "Decrease Brightness");
            Color.RGBToHSV(i.color, out float h, out float s, out float v);
            v -= 0.05f;
            i.color = Color.HSVToRGB(h, s, v);
        }
    }

    public void MacroIncreaseBrightness()
    {
        foreach (var i in lights)
        {
            Undo.RecordObject(i, "Increase Brightness");
            Color.RGBToHSV(i.color, out float h, out float s, out float v);
            v += 0.05f;
            i.color = Color.HSVToRGB(h, s, v);
        }
    }

    public void MacroDecreaseShadowStrength()
    {
        foreach (var i in lights)
        {
            Undo.RecordObject(i, "Decrease ShadowStrength");
            i.shadowStrength -= 0.05f;
        }
    }

    public void MacroIncreaseShadowStrength()
    {
        foreach (var i in lights)
        {
            Undo.RecordObject(i, "Increase ShadowStrength");
            i.shadowStrength += 0.05f;
        }
    }
}
