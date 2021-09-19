using UnityEngine;

namespace TRS.CaptureTool
{
    public class ShowDuringGifScript : MonoBehaviour
    {
        public bool disableAfterAwake;

        void Awake()
        {
            GifScript.WillTakeGif += Enable;
            GifScript.GifTaken += Disable;
            if (disableAfterAwake) gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            GifScript.WillTakeGif -= Enable;
            GifScript.GifTaken -= Disable;
        }

        void Enable(GifScript gifScript, int width, int height)
        {
            gameObject.SetActive(true);
        }

        void Disable(GifScript gifScript, Texture2D[] gifFrames, int fps)
        {
            gameObject.SetActive(false);
        }
    }
}