using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using TRS.CaptureTool.Extras;
namespace TRS.CaptureTool
{
    [RequireComponent(typeof(RawImage))]
    public class DisplayGifScript : MonoBehaviour
    {
        public bool loadGifAfterTaken = true;
        public bool loadGifAfterSaved = false;
        public bool copyBeforeReset = false;

        public float framesPerSecond = 10.0f;
        float timePerFrame;
        public Texture2D[] frames;
        public Text errorText;

        RawImage rawImage;
        UniGifImage uniGifImage;
        AspectFitScript aspectFitScript;
        UniGifImageAspectController uniGifImageAspectController;

        bool setup;
        Color transparentColor;
        IEnumerator copyCoroutine;

        void Awake()
        {
            if (!setup)
                Setup();
        }

        void Setup()
        {
            setup = true;
            rawImage = GetComponent<RawImage>();
            uniGifImage = GetComponent<UniGifImage>();
            aspectFitScript = GetComponent<AspectFitScript>();
            uniGifImageAspectController = GetComponent<UniGifImageAspectController>();

            transparentColor = new Color(0, 0, 0, 0);
            rawImage.color = transparentColor;
        }

        void OnEnable()
        {
            if (loadGifAfterTaken)
                GifScript.GifTaken += GifTaken;
            if (loadGifAfterSaved)
                GifScript.GifSaved += GifSaved;

            GifScript.WillResetGif += WillReset;
        }

        void OnDisable()
        {
            if (loadGifAfterTaken)
                GifScript.GifTaken -= GifTaken;
            if (loadGifAfterSaved)
                GifScript.GifSaved -= GifSaved;

            GifScript.WillResetGif -= WillReset;
        }

        void Update()
        {
            if (frames == null || frames.Length == 0)
            {
                if (errorText != null)
                    errorText.gameObject.SetActive(true);
                return;
            }

            int index = Mathf.FloorToInt(Time.time / timePerFrame) % frames.Length;
            rawImage.texture = frames[index];

            if (errorText != null)
                errorText.gameObject.SetActive(rawImage.texture == null);
        }

        void WillReset(GifScript gifScript)
        {
            if (copyBeforeReset)
                CopyFrames(frames, false, false);
            else if (frames != null && frames.Length > 0) // If we were previously using the frames
                rawImage.color = transparentColor;
        }

        void GifTaken(GifScript gifScript, Texture2D[] newFrames, int newFramesPerSecond)
        {
            LoadGif(newFrames, newFramesPerSecond);
        }

        void GifSaved(GifScript gifScript, string fullFilePath)
        {
            LoadSavedGif(fullFilePath);
        }

        public void LoadGif(Texture2D[] newFrames, int newFramesPerSecond)
        {
            if (!setup)
                Setup();

            frames = newFrames;
            framesPerSecond = newFramesPerSecond;
            timePerFrame = Mathf.Round(100f / framesPerSecond) / 100f;

            rawImage.color = Color.white;
            if (aspectFitScript != null)
                aspectFitScript.SetTexture(newFrames[0]);
            else if (uniGifImageAspectController != null)
                uniGifImageAspectController.FixAspectRatioWithinBounds(newFrames[0].width, newFrames[0].height);
        }

        public void LoadSavedGifFromUrl(string url)
        {
            ClearFrames();
            AddUniGifImageIfNecessary();
            uniGifImage.SetGifFromUrl(url, true);
        }

        public void LoadSavedGif(string fullFilePath)
        {
            ClearFrames();
            AddUniGifImageIfNecessary();
            uniGifImage.SetGifFromUrl(fullFilePath.Replace(Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar, ""), true);
        }

        void CopyFrames(Texture2D[] newFrames, bool async = true, bool inPlace = true)
        {
            if (async)
            {
                if (copyCoroutine != null)
                    StopCoroutine(copyCoroutine);
                copyCoroutine = CopyFramesAsync(newFrames, inPlace);
                StartCoroutine(copyCoroutine);
            }
            else
                SynchronousCopyFrames(newFrames, inPlace);
        }

        void SynchronousCopyFrames(Texture2D[] newFrames, bool inPlace = true)
        {
            IEnumerator copyFunc = CopyFramesAsync(newFrames, inPlace);
            while (copyFunc.MoveNext()) { }
        }

        IEnumerator CopyFramesAsync(Texture2D[] newFrames, bool inPlace = true)
        {
            Texture2D[] replacementFrames;
            if (inPlace)
            {
                ClearFrames();
                rawImage.color = transparentColor;
                replacementFrames = frames = new Texture2D[newFrames.Length];
            }
            else
                replacementFrames = new Texture2D[newFrames.Length];

            for (int i = 0; i < replacementFrames.Length; ++i)
            {
                Texture2D texture = newFrames[i];
                Texture2D textureCopy = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1);
                Graphics.CopyTexture(texture, textureCopy);
                textureCopy.Apply(false);

                replacementFrames[i] = textureCopy;
                yield return null;
            }

            if (!inPlace)
            {
                ClearFrames();
                frames = replacementFrames;
            }

            rawImage.color = Color.white;
        }

        void ClearFrames()
        {
            Debug.Log("HERE ClearFrames");

            if (copyCoroutine != null)
            {
                StopCoroutine(copyCoroutine);
                copyCoroutine = null;
            }

            if (frames != null)
            {
                foreach (Texture2D texture in frames)
                    Destroy(texture);
            }

            frames = null;
        }

        void AddUniGifImageIfNecessary()
        {
            // Aspect controller added first, so UniGifImage grabs it on init
            if (uniGifImageAspectController == null)
                uniGifImageAspectController = gameObject.AddComponent<UniGifImageAspectController>();
            if (uniGifImage == null)
                uniGifImage = gameObject.AddComponent<UniGifImage>();
        }
    }
}