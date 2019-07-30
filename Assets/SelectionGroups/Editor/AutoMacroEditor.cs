using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class AutoMacroEditor : MacroEditor
{
    List<MethodInfo> exposedActions = new List<MethodInfo>();

    public override bool IsValidForSelection => false;

    public override void OnEnable()
    {
        foreach (var i in GetType().GetMethods())
        {
            if (i.Name.StartsWith("Macro") && i.GetParameters().Length == 0)
            {
                exposedActions.Add(i);
            }
        }
    }

    public override void OnGUI()
    {
        foreach (var i in exposedActions)
        {
            if (GUILayout.Button(i.Name.Substring(5))) i.Invoke(this, System.Type.EmptyTypes);
        }
    }
}
