using UnityEngine;

namespace Utj.Film
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class ReplacementShaderRenderer : MonoBehaviour
    {
        public Camera mainCamera;
        public Shader shader;
        protected new Camera camera;

        protected virtual void Reset()
        {
            mainCamera = Camera.main;
            shader = Shader.Find($"Utj/{GetType().Name}");
        }

        protected virtual void Start()
        {
            camera = GetComponent<Camera>();
            if (mainCamera != null)
            {
                camera.CopyFrom(mainCamera);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.white * 0;
            }
            camera.SetReplacementShader(shader, null);
        }

        void OnPreRender()
        {
            if (mainCamera != null)
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