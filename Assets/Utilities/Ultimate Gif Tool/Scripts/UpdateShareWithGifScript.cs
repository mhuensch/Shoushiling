using UnityEngine;

using TRS.CaptureTool.Share;
namespace TRS.CaptureTool
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ShareScript))]
    public class UpdateShareWithGifScript : MonoBehaviour
    {
        int cachedWidth;
        int cachedHeight;

        ShareScript shareScript;

        void OnEnable()
        {
            if (shareScript == null)
                shareScript = GetComponent<ShareScript>();
            SubscribeToMediaEvents();
        }

        void OnDisable()
        {
            UnsubscribeFromMediaEvents();
        }

        void SubscribeToMediaEvents()
        {
            GifScript.WillTakeGif += UpdateMediaSize;
            GifScript.GifSaved += UpdateGifMediaPath;
        }

        void UnsubscribeFromMediaEvents()
        {
            GifScript.WillTakeGif -= UpdateMediaSize;
            GifScript.GifSaved -= UpdateGifMediaPath;
        }

        void UpdateMediaSize(GifScript gifScript, int width, int height)
        {
            cachedWidth = width;
            cachedHeight = height;
        }

        void UpdateGifMediaPath(GifScript gifScript, string filePath)
        {
            UpdateShareScript(filePath);
        }

        void UpdateShareScript(string filePath)
        {
            shareScript.mediaWidth = cachedWidth;
            shareScript.mediaHeight = cachedHeight;
            shareScript.mediaToUploadPath = filePath;
        }
    }
}