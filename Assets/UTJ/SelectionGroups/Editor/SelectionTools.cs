using UnityEditor;
using UnityEngine;

namespace Utj.Film
{
    public class SelectionTools : AutoMacroEditor
    {
        public override bool IsValidForSelection => Selection.transforms.Length > 0;

        public void MacroReparent()
        {
            var g = new GameObject("GameObject");
            Undo.RegisterCreatedObjectUndo(g, "Reparent");
            var position = Vector3.zero;
            foreach (var i in Selection.transforms)
                position += i.position;
            position /= Selection.transforms.Length;
            g.transform.position = position;
            foreach (var i in Selection.transforms)
                Undo.SetTransformParent(i, g.transform, "Reparent");
            Selection.objects = new[] { g };
        }
    }
}