using UnityEngine;

namespace Utj.Film
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class DepthRenderer : MonoBehaviour
    {
        public Camera mainCamera;
        public Shader depthShader;
        new Camera camera;

        void Reset()
        {
            mainCamera = Camera.main;
            depthShader = Shader.Find("Utj/DepthRenderer");
        }

        void OnValidate()
        {
            camera = GetComponent<Camera>();
            if (mainCamera != null)
            {
                camera.CopyFrom(mainCamera);
                camera.depth -= 2;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
            }
        }

        void Start()
        {
            camera.SetReplacementShader(depthShader, null);
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