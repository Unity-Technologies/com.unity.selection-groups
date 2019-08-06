using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Utj.Film
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class SelectionGroupRenderer : MonoBehaviour
    {
        public Color backgroundColor = Color.black;
        public Color defaultColor = Color.white;
        public Camera mainCamera;
        public Shader objectIdShader;
        new Camera camera;

        void Reset()
        {
            mainCamera = Camera.main;
            objectIdShader = Shader.Find("Utj/ObjectIdRenderer");
        }

        void OnValidate()
        {
            camera = GetComponent<Camera>();
            if (mainCamera != null)
            {
                camera.CopyFrom(mainCamera);
                camera.depth -= 1;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = backgroundColor;
            }
        }

        void Start()
        {
            camera.SetReplacementShader(objectIdShader, null);
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

        void OnPreRender()
        {
            if (camera != null && mainCamera != null)
            {
                camera.transform.position = mainCamera.transform.position;
                camera.transform.rotation = mainCamera.transform.rotation;
                camera.rect = mainCamera.rect;
                camera.fieldOfView = mainCamera.fieldOfView;
                camera.nearClipPlane = mainCamera.nearClipPlane;
                camera.farClipPlane = mainCamera.farClipPlane;
            }
        }

    }
}