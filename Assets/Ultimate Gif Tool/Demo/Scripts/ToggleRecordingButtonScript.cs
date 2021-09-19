using UnityEngine;
using UnityEngine.UI;

namespace TRS.CaptureTool
{
    public class ToggleRecordingButtonScript : MonoBehaviour
    {
        public GifScript gifScript;

        Text text;

        void Awake()
        {
            text = GetComponentInChildren<Text>();
            UpdateText();
        }

        void Update()
        {
            UpdateText();
        }

        public void ToggleRecording()
        {
            gifScript.ToggleRecording();
        }

        void UpdateText()
        {
            if (gifScript.recorderState == GifScript.RecorderState.Recording)
                text.text = "Pause";
            else if (gifScript.recorderState == GifScript.RecorderState.PreProcessing)
                text.text = "PreProcessing";
            else
                text.text = "Record";
        }
    }
}