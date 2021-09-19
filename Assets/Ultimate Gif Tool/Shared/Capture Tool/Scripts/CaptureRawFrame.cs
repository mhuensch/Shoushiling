using UnityEngine;

namespace TRS.CaptureTool
{
    /** Note these methods require the rect to be set with a width and height. */
    public static class CaptureRawFrame
    {
        public static RawFrameData ScreenCapture(Rect rect, Resolution resizeResolution, bool solidify)
        {
            TextureFormat textureFormat = solidify ? TextureFormat.RGB24 : TextureFormat.ARGB32;
            Texture2D screenCapture = new Texture2D((int)rect.width, (int)rect.height, textureFormat, false);
            screenCapture.ReadPixels(rect, 0, 0, false);

            Rect fullScreenCaptureRect = new Rect(0, 0, screenCapture.width, screenCapture.height);
            return new RawFrameData(screenCapture, fullScreenCaptureRect, resizeResolution, solidify);
        }

#if UNITY_2017_3_OR_NEWER
        public static RawFrameData AltScreenCapture(Rect rect, Resolution resizeResolution, bool solidify, int scale = 1)
        {
            return new RawFrameData(UnityEngine.ScreenCapture.CaptureScreenshotAsTexture(scale), rect, resizeResolution, solidify);
        }
#endif

        public static RawFrameData CameraRenderTexture(Camera camera, Rect rect, Resolution resizeResolution, bool solidify)
        {
            RenderTexture renderTexture = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight, 24);
            Texture2D resultTexture = new Texture2D((int)rect.width, (int)rect.height, solidify ? TextureFormat.RGB24 : TextureFormat.ARGB32, false);

            RenderTexture originalCameraTargetTexture = camera.targetTexture;
            RenderTexture originalRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            camera.targetTexture = renderTexture;
            camera.Render();

            resultTexture.ReadPixels(rect, 0, 0, false);

            camera.targetTexture = originalCameraTargetTexture;
            RenderTexture.active = originalRenderTexture;
            RenderTexture.ReleaseTemporary(renderTexture);

            Rect fullScreenCaptureRect = new Rect(0, 0, resultTexture.width, resultTexture.height);
            return new RawFrameData(resultTexture, fullScreenCaptureRect, resizeResolution, solidify);
        }

        public static RawFrameData CamerasRenderTexture(Camera[] cameras, Rect rect, Resolution resizeResolution, bool solidify)
        {
            RenderTexture renderTexture = RenderTexture.GetTemporary(Camera.main.pixelWidth, Camera.main.pixelHeight, 24);
            Texture2D resultTexture = new Texture2D((int)rect.width, (int)rect.height, solidify ? TextureFormat.RGB24 : TextureFormat.ARGB32, false);

            foreach (Camera camera in cameras)
            {
                if (!camera.enabled || !camera.gameObject.activeInHierarchy || camera.cameraType == CameraType.SceneView)
                    continue;

                RenderTexture originalCameraTargetTexture = camera.targetTexture;
                camera.targetTexture = renderTexture;
                camera.Render();
                camera.targetTexture = originalCameraTargetTexture;
            }

            RenderTexture originalRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            resultTexture.ReadPixels(rect, 0, 0, false);
            RenderTexture.active = originalRenderTexture;
            RenderTexture.ReleaseTemporary(renderTexture);

            Rect fullScreenCaptureRect = new Rect(0, 0, resultTexture.width, resultTexture.height);
            return new RawFrameData(resultTexture, fullScreenCaptureRect, resizeResolution, solidify);
        }

        public static RawFrameData AllCamerasRenderTexture(Rect rect, Resolution resizeResolution, bool solidify)
        {
            return CamerasRenderTexture(Camera.allCameras, rect, resizeResolution, solidify);
        }
    }
}
