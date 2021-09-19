using UnityEngine;
using UnityEngine.UI;

namespace TRS.CaptureTool.Share
{
    public class DemoGifShareTextScript : MonoBehaviour
    {
        public GifScript gifScript;
        public ShareScript shareScript;

        Text text;
#if UNITY_EDITOR || !(UNITY_IOS || UNITY_ANDROID)
        bool gifUploaded = false;
#endif

        void Awake()
        {
            text = GetComponent<Text>();
        }

#if UNITY_EDITOR || !(UNITY_IOS || UNITY_ANDROID)
        void Update()
        {
            if (gifScript.saveProgress < 1f)
            {
                gifUploaded = false;
                text.text = (gifScript.saveProgress * 100f).ToString("N1") + "% Saved";
            }
            else if (shareScript.uploadingToServer)
            {
                gifUploaded = true;
                text.text = "Uploading...";
            }
            else if (gifUploaded)
                text.text = "Ready to Share";
            else if (!string.IsNullOrEmpty(gifScript.lastSaveFilePath))
                text.text = "Saved. Upload to Share";
            else
                text.text = "Save to Upload or Share";
        }
#else
        void Update()
        {
            if (gifScript.saveProgress < 1f)
                text.text = (gifScript.saveProgress * 100f).ToString("N1") + "% Saved";
            else if (!string.IsNullOrEmpty(gifScript.lastSaveFilePath))
                text.text = "Saved. Upload or Share";
            else
                text.text = "Save to Upload or Share";
        }
#endif
    }
}
