using UnityEngine;
namespace Utj.Film
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class NormalsRenderer : MonoBehaviour
    {
        public Camera mainCamera;
        public Shader normalsdShader;
        new Camera camera;

        void Reset()
        {
            mainCamera = Camera.main;
            normalsdShader = Shader.Find("Utj/NormalsRenderer");
        }

        void OnValidate()
        {
            camera = GetComponent<Camera>();
            if (mainCamera != null)
            {
                camera.CopyFrom(mainCamera);
                camera.depth -= 3;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
            }
        }

        void Start()
        {
            camera.SetReplacementShader(normalsdShader, null);
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