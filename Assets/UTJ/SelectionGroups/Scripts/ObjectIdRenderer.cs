using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Utj.Film
{
    public class ObjectIdRenderer : ReplacementShaderRenderer
    {
        public Color backgroundColor = Color.black;
        public Color defaultColor = Color.white;

        protected override void Start()
        {
            base.Start();
            camera.backgroundColor = backgroundColor;
            ConfigureRendererComponents();
        }

        void ConfigureRendererComponents()
        {
            var unselectedRenderers = new HashSet<Renderer>(FindObjectsOfType<Renderer>());
            var selectedRenderers = new HashSet<Renderer>();
            foreach (var selectionGroup in SelectionGroups.Instance.groups)
            {
                foreach (var i in selectionGroup.objects)
                {
                    if (i is Renderer)
                    {
                        selectedRenderers.Add((Renderer)i);
                        AddPropertyBlock((Renderer)i, selectionGroup.color);
                    }
                    else if (i is GameObject)
                    {
                        foreach (var r in ((GameObject)i).GetComponents<Renderer>())
                        {
                            selectedRenderers.Add(r);
                            AddPropertyBlock(r, selectionGroup.color);
                        }
                    }
                }
            }
            unselectedRenderers.ExceptWith(selectedRenderers);
            foreach (var r in unselectedRenderers)
            {
                AddPropertyBlock(r, defaultColor);
            }
        }

        void AddPropertyBlock(Renderer renderer, Color color)
        {
            var mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);
            mpb.SetColor("_IdColor", color);
            renderer.SetPropertyBlock(mpb);
        }

    }
}