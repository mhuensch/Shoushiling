using UnityEngine;

namespace TRS.CaptureTool
{
    public class HideDuringGifScript : MonoBehaviour
    {
        public bool disableAfterAwake;

        void Awake()
        {
            GifScript.WillTakeGif += Disable;
            GifScript.GifTaken += Enable;
            if (disableAfterAwake) gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            GifScript.WillTakeGif -= Disable;
            GifScript.GifTaken -= Enable;
        }

        void Enable(GifScript gifScript, Texture2D[] gifFrames, int fps)
        {
            gameObject.SetActive(true);
        }

        void Disable(GifScript gifScript, int width, int height)
        {
            gameObject.SetActive(false);
        }
    }
}