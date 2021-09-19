using UnityEngine;
using UnityEngine.UI;

namespace TRS.CaptureTool.Share
{
    public class DemoGifUploadTextScript : MonoBehaviour
    {
        public GifScript gifScript;
        public ShareScript shareScript;

        Text text;
        bool gifUploaded = false;

        void Awake()
        {
            text = GetComponent<Text>();
        }

        void Update()
        {
            if (gifScript.saveProgress < 1f)
            {
                gifUploaded = false;
                text.text = "Gif " + (gifScript.saveProgress * 100f).ToString("N1") + "% Saved";
            }
            else if (shareScript.uploadingToServer)
            {
                gifUploaded = true;
                text.text = "Uploading...";
            }
            else if (gifUploaded)
                text.text = "Gif Uploaded";
            else if (!string.IsNullOrEmpty(gifScript.lastSaveFilePath))
                text.text = "Ready to Upload";
            else
                text.text = "Save Gif to Upload";
        }
    }
}