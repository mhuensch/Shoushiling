using UnityEngine;

namespace TRS.CaptureTool
{
    [RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    public class OnRenderImageScript : MonoBehaviour
    {
        public System.Action<RenderTexture> OnRenderImageListeners;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);
            if (OnRenderImageListeners != null)
                OnRenderImageListeners(source);
        }
        /*
        RenderTexture _rt;
        Camera _camera;

        void Awake()
        {
            _camera = GetComponent<Camera>();
            _rt = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight);
        }

        void OnDestroy()
        {
            RenderTexture.ReleaseTemporary(_rt);
        }

        void OnPreRender()
        {
            if (_rt == null || _rt.width != _camera.pixelWidth || _rt.height != _camera.pixelHeight)
            {
                RenderTexture.ReleaseTemporary(_rt);
                _rt = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight);
            }

            _camera.targetTexture = _rt;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (OnRenderImageListeners != null)
                OnRenderImageListeners(source);
            _camera.targetTexture = null;
            Graphics.Blit(source, null as RenderTexture);
        }
        */
    }
}