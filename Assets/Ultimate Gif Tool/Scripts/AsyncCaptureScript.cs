using UnityEngine;
using System.Collections.Generic;
#if UNITY_2018_2_OR_NEWER
using UnityEngine.Rendering;
//using UnityEngine.Experimental.Rendering;
#endif

namespace TRS.CaptureTool
{
    public class AsyncCaptureScript : MonoBehaviour
    {
#if UNITY_2018_2_OR_NEWER
        public System.Action<Color32[]> AsyncCaptureListeners;

        const int MAX_REQUEST_COUNT = 8;

        int[] cameraLayersUsed;
        Queue<AsyncGPUReadbackRequest> _requests = new Queue<AsyncGPUReadbackRequest>();

        void Update()
        {
            while (_requests.Count > 0)
            {
                var request = _requests.Peek();
                if (request.hasError)
                {
                    Debug.Log("GPU readback error detected.");
                    _requests.Dequeue();
                }
                else if (request.done)
                {
                    if (AsyncCaptureListeners != null)
                    {
                        /*
                        int totalSize = Camera.main.pixelWidth * Camera.main.pixelHeight;
                        Debug.Log("Total pixels on camera: " + totalSize.Length);
                        Debug.Log("Total pixels in GPU capture: " + request.GetData<Color32>().ToArray().Length);
    */
                        AsyncCaptureListeners(request.GetData<Color32>().ToArray());
                    }

                    _requests.Dequeue();
                }
                else
                    break;
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (AsyncCaptureListeners != null)
            {
                if (_requests.Count < MAX_REQUEST_COUNT)
                    _requests.Enqueue(AsyncGPUReadback.Request(source));
                else
                    Debug.Log("Too many requests.");
            }

            Graphics.Blit(source, destination);
        }
#endif
    }
}