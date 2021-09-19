using UnityEngine;
using UnityEngine.UI;

namespace TRS.CaptureTool.Share
{
    public class DemoShareScript : MonoBehaviour
    {
        public ShareScript shareScript;

        Button button;

        void Awake()
        {
            button = GetComponent<Button>();
        }

        void Update()
        {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
            button.interactable = !string.IsNullOrEmpty(shareScript.mediaToUploadPath) && !shareScript.uploadingToServer;
#else
            button.interactable = !string.IsNullOrEmpty(shareScript.urlToShare);
#endif
        }

        public void DemoShare()
        {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
            APIShare.NativeShare(shareScript.mediaToUploadPath, shareScript.defaultText, shareScript.defaultUrl);
#else
            WebShare.ShareByEmail(shareScript.urlToShare, shareScript.emailBody, "jacob@tangledrealitystudios.com", shareScript.emailSubject);
            WebShare.ShareToTwitter(shareScript.urlToShare, "@tangled_reality " + shareScript.twitterText, shareScript.twitterHashtags);
#endif
        }
    }
}