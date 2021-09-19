using UnityEngine;
using UnityEngine.UI;

namespace TRS.CaptureTool
{
    public class ToggleCaptureModeButtonScript : MonoBehaviour
    {
        public GifScript gifScript;

        public Button previewButton;
        public Text previewText;

        Text text;

        void Awake()
        {
            text = GetComponentInChildren<Text>();
        }

        void Start()
        {
            UpdateDisplay();
        }

        public void ToggleCaptureMode()
        {
#if ALLOW_ASYNC
        if(gifScript.captureMode == GifScript.GifCaptureMode.Async) {
#else
            if (gifScript.captureMode == GifScript.GifCaptureMode.MultiCamera)
            {
#endif
                gifScript.captureMode = GifScript.GifCaptureMode.ScreenCapture;
            }
            else
                gifScript.captureMode = (GifScript.GifCaptureMode)((int)gifScript.captureMode + 1);

            gifScript.CaptureModeChanged();

            UpdateDisplay();
        }

        void UpdateDisplay()
        {
            previewButton.gameObject.SetActive(!gifScript.prepareForPreview);
            previewText.gameObject.SetActive(!gifScript.prepareForPreview);

            if (gifScript.captureMode == GifScript.GifCaptureMode.SingleCamera)
                text.text = "Single Camera";
            else if (gifScript.captureMode == GifScript.GifCaptureMode.MultiCamera)
                text.text = "MultiCamera";
            else if (gifScript.captureMode == GifScript.GifCaptureMode.ScreenCapture)
                text.text = "Screen Capture";
#if ALLOW_ASYNC
        else if (gifScript.captureMode == GifScript.GifCaptureMode.Async)
            text.text = "Async";
#endif
        }
    }
}