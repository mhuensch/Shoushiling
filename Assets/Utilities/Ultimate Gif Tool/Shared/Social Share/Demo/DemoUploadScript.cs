using UnityEngine;
using UnityEngine.UI;

namespace TRS.CaptureTool.Share
{
    public class DemoUploadScript : MonoBehaviour
    {
        public ShareScript shareScript;

        Button button;
        Text buttonText;

        int numPeriods;
        float timeSinceLastUpdate;
        const int maxNumPeriods = 3;
        const float timeBetweenPeriodUpdates = 0.25f;

        void Awake()
        {
            button = GetComponent<Button>();
            buttonText = GetComponentInChildren<Text>();
        }

        void Update()
        {
            button.interactable = !string.IsNullOrEmpty(shareScript.mediaToUploadPath) && !shareScript.uploadingToServer;
            buttonText.text = shareScript.uploadingToServer ? "Uploading" + new string('.', numPeriods) : "Upload";

            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate >= timeBetweenPeriodUpdates)
            {
                timeSinceLastUpdate = 0;
                ++numPeriods;
                if (numPeriods > maxNumPeriods)
                    numPeriods = 0;
            }
        }

        public void DemoUpload()
        {
            shareScript.UploadToImgur();
        }
    }
}