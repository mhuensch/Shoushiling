using UnityEngine;
using UnityEngine.UI;

namespace TRS.CaptureTool.Share
{
    public class InstantGifShareButton : MonoBehaviour
    {
        [Tooltip("ShareScript with the share settings.")]
        public ShareScript shareScript;
        [Tooltip("DisplayGifScript that will preview the gif.")]
        public DisplayGifScript displayGifScript;
        [Tooltip("InstantTwitterShareScript that will display the UI for sharing the gif.")]
        public InstantTwitterShareScript instantTwitterShareScript;

        [UnityEngine.Serialization.FormerlySerializedAs("username")]
        [Tooltip("Optional username to override the value from the ShareScript's config's twitterUsername property.")]
        public string usernameOverride;
        [Tooltip("Comma-separated list of additional hashtags. (Ex. giffriday,screenshotsaturday,indiedevhour)")]
        public string extraHashtags;

        [Tooltip("Reference to the button. Used to disable interactions when a saved gif is not available.")]
        public Button button;

        void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
            if (button != null)
                button.interactable = false;

            GifScript.GifToSave += GifToSave;
            GifScript.GifSaved += GifSaved;
        }

        void OnDestroy()
        {
            GifScript.GifToSave -= GifToSave;
            GifScript.GifSaved -= GifSaved;
        }

        public void OnClick()
        {
            instantTwitterShareScript.gameObject.SetActive(true);
        }

        void GifToSave(GifScript gifScript, Texture2D[] frames, int framesPerSecond)
        {
            if (button != null)
                button.interactable = false;
            displayGifScript.LoadGif(frames, framesPerSecond);
        }

        void GifSaved(GifScript gifScript, string filePath)
        {
            if (button != null)
                button.interactable = true;

            string fullHashtagString = shareScript.twitterHashtagsText;
            foreach (string hashtag in extraHashtags.Split(','))
                fullHashtagString += " #" + hashtag;

            instantTwitterShareScript.filePath = filePath;
            if (!string.IsNullOrEmpty(usernameOverride))
                instantTwitterShareScript.username = usernameOverride;
            instantTwitterShareScript.defaultText = shareScript.twitterText + fullHashtagString;
        }
    }
}