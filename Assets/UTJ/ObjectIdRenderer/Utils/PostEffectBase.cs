// (C) UTJ
using UnityEngine;
using System;
using System.Reflection;
using Compositor = Utj.Film.Compositor;
using Compositor;
using Compositor.Util;

namespace Compositor
{

    [RequireComponent(typeof(Camera))]
    public abstract class PostEffectBase : MonoBehaviour
    {
        Camera masterCamera_ = null;
        protected virtual Camera MasterCamera
        {
            get { return masterCamera_; }
            set { masterCamera_ = value; }
        }

        Camera thisCamera_ = null;
        protected Camera ThisCamera
        {
            get { return thisCamera_ = thisCamera_ ?? GetComponent<Camera>(); }
        }

        Material effectMaterial_ = null;
        protected Material EffectMaterial
        {
            get { return effectMaterial_; }
            set { DestroyMaterial(effectMaterial_); effectMaterial_ = value; }
        }

        protected bool IsGBufferAvailable
        {
            get { return ThisCamera.actualRenderingPath == RenderingPath.DeferredShading; }
        }

        protected static void DestroyMaterial(Material material)
        {
            if (material != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(material); // Unity Editor
#else
			Destroy(material);
#endif
            }
        }

        void Start()
        {
            onPreStart();
            if (MasterCamera != null)
            {
                var targetDisplay = ThisCamera.targetDisplay;
                ThisCamera.CopyFrom(MasterCamera);
                ThisCamera.targetDisplay = targetDisplay;
            }
            onStart();
            if (EffectMaterial != null)
            {
                EffectMaterial.hideFlags = HideFlags.DontSave;
            }
        }

        void OnDestroy()
        {
            onDestroy();
            EffectMaterial = null;
        }

        void OnPreCull()
        {
            onPreCull();
        }

        void OnPreRender()
        {
            if (ThisCamera != null && MasterCamera != null)
            {
                ThisCamera.transform.position = MasterCamera.transform.position;
                ThisCamera.transform.rotation = MasterCamera.transform.rotation;
                ThisCamera.rect = MasterCamera.rect;
                ThisCamera.fieldOfView = MasterCamera.fieldOfView;
                ThisCamera.nearClipPlane = MasterCamera.nearClipPlane;
                ThisCamera.farClipPlane = MasterCamera.farClipPlane;
            }
            onPreRender();
        }

        void OnPostRender()
        {
            onPostRender();
        }

        [ImageEffectOpaque]
        void OnRenderImage(RenderTexture srcRt, RenderTexture dstRt)
        {
            if (!onRenderImage(srcRt, dstRt))
            {
                Graphics.Blit(srcRt, dstRt);
            }
        }

        protected virtual void onPreStart() { }
        protected virtual void onStart() { }
        protected virtual void onDestroy() { }
        protected virtual void onPreCull() { }
        protected virtual void onPreRender() { }
        protected virtual void onPostRender() { }
        protected virtual bool onRenderImage(RenderTexture srcRt, RenderTexture dstRt) { return false; }

        //	public static bool SetupRenderTextureAndMovieRecorder(Camera camera, UTJ.FrameCapturer.MovieRecorder movieRecorder) {
        public static bool SetupRenderTextureAndMovieRecorder(Camera camera, Behaviour movieRecorder)
        {
            if (movieRecorder == null)
            {
                return false;
            }
            RenderTexture renderTexture = null;
            {
                var rtd = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight);
                rtd.width = camera.pixelWidth;
                rtd.height = camera.pixelHeight;

                rtd.autoGenerateMips = false;
                rtd.bindMS = false;
                rtd.colorFormat = RenderTextureFormat.Default;
                rtd.depthBufferBits = 24;
                rtd.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
                rtd.enableRandomWrite = false;
                //		rtd.flags				= RenderTextureCreationFlags.CreatedFromScript;
                rtd.memoryless = RenderTextureMemoryless.None;
                rtd.msaaSamples = 1;
                rtd.shadowSamplingMode = UnityEngine.Rendering.ShadowSamplingMode.CompareDepths;
                rtd.sRGB = false;
                rtd.useMipMap = false;
                rtd.volumeDepth = 1;
                rtd.vrUsage = VRTextureUsage.None;

                renderTexture = new RenderTexture(rtd);
                renderTexture.filterMode = FilterMode.Point;
                renderTexture.autoGenerateMips = false;
            }
            if (renderTexture == null)
            {
                return false;
            }
            camera.targetTexture = renderTexture;

#if false
		movieRecorder.targetRT = renderTexture;
		movieRecorder.captureTarget = UTJ.FrameCapturer.MovieRecorder.CaptureTarget.RenderTexture;
#else
            {
                Type t = movieRecorder.GetType();

                // movieRecorder.targetRT = renderTexture
                {
                    PropertyInfo pi = t.GetProperty("targetRT");
                    pi.SetValue(movieRecorder, Convert.ChangeType(renderTexture, pi.PropertyType), null);
                }

                // movieRecorder.captureTarget = UTJ.FrameCapturer.MovieRecorder.CaptureTarget.RenderTexture;
                {
                    var tEnumCaptureTarget = t.GetNestedType("CaptureTarget", BindingFlags.Public | BindingFlags.NonPublic);
                    var eRenderTexture = Enum.Parse(tEnumCaptureTarget, "RenderTexture");
                    PropertyInfo pi = t.GetProperty("captureTarget");
                    pi.SetValue(movieRecorder, Convert.ChangeType(eRenderTexture, pi.PropertyType), null);
                }
            }
#endif
            return true;
        }
    }

} // namespace
