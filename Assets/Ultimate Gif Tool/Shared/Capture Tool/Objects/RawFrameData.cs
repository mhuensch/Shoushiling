using UnityEngine;

using TRS.CaptureTool.Extras;
namespace TRS.CaptureTool
{
    [System.Serializable]
    public class RawFrameData
    {
        [SerializeField]
        Texture2D rawTexture;
        [SerializeField]
        RenderTexture renderTexture;
        [SerializeField]
        Color32[] colors;

        public bool destroyOriginal = true;

        public Texture2D processedTexture;

        public bool processed { get { return processedTexture != null; } }

        // Track record resolution to know dimensions of colors array.
        [SerializeField]
        Resolution recordResolution;

        Rect cutout;
        bool solidify;
        Resolution resizeResolution;

        public Resolution resolution
        {
            get
            {
                if (rawTexture != null)
                    return new Resolution { width = rawTexture.width, height = rawTexture.height };
                else if (renderTexture != null)
                    return new Resolution { width = renderTexture.width, height = renderTexture.height };
                else if (colors != null)
                    return recordResolution;

                throw new UnityException("Attempting to get resolution, but raw frame data has no data.");
            }
        }

        RawFrameData(Rect cutout, Resolution resizeResolution, bool solidify)
        {
            this.cutout = cutout;
            this.solidify = solidify;
            this.resizeResolution = resizeResolution;
        }

        public RawFrameData(Texture2D rawTexture, Rect cutout, Resolution resizeResolution, bool solidify) : this(cutout, resizeResolution, solidify)
        {
            this.rawTexture = rawTexture;
        }

        public RawFrameData(RenderTexture renderTexture, Rect cutout, Resolution resizeResolution, bool solidify) : this(cutout, resizeResolution, solidify)
        {
            this.renderTexture = renderTexture;
        }

        public RawFrameData(Color32[] colors, Resolution recordResolution, Rect cutout, Resolution resizeResolution, bool solidify) : this(cutout, resizeResolution, solidify)
        {
            this.colors = colors;
            this.recordResolution = recordResolution;
        }

        public RawFrameData(Texture2D processedTexture)
        {
            this.processedTexture = processedTexture;
        }

        public Texture2D Process(bool apply = true)
        {
            if (processed) return processedTexture;

            if (rawTexture != null)
                processedTexture = ProcessRawTexture();
            else if (renderTexture != null)
                processedTexture = ProcessRenderTexture();
            else if (colors != null && colors.Length > 0)
                processedTexture = ProcessColor32Array();
            else
                // If you're hitting this error, you may be deleting the processedTexture elsewhere or using a destroyed RawDataFrame.
                throw new UnityException("Attempting to get process frame, but raw frame data has no data.");

            if(resizeResolution.HasSize())
                processedTexture = processedTexture.Resize(resizeResolution);
            if (apply) processedTexture.Apply(false);
            return processedTexture;
        }

        Texture2D ProcessRawTexture()
        {
            Texture2D resultTexture = rawTexture.Cutout(cutout, false);
            if (!solidify || resultTexture.format == TextureFormat.RGB24)
                return resultTexture;

            Texture2D solidTexture = new Texture2D(resultTexture.width, resultTexture.height, TextureFormat.RGB24, false);
            solidTexture.SetPixels(resultTexture.GetPixels());
            if(destroyOriginal) MonoBehaviourExtended.FlexibleDestroy(resultTexture);
            return solidTexture;
        }

        Texture2D ProcessRenderTexture()
        {
            RenderTexture.active = renderTexture;

            Rect textureRect = cutout;
            if (cutout == Rect.zero)
                textureRect = new Rect(0, 0, resolution.width, resolution.height);
            Texture2D resultTexture = new Texture2D((int)textureRect.width, (int)textureRect.height, solidify ? TextureFormat.RGB24 : TextureFormat.ARGB32, false);
            resultTexture.ReadPixels(textureRect, 0, 0, false);

            RenderTexture.active = null;
            if(destroyOriginal) MonoBehaviourExtended.FlexibleDestroy(renderTexture);
            renderTexture = null;

            return resultTexture;
        }

        Texture2D ProcessColor32Array()
        {
            if(!recordResolution.HasSize())
                throw new UnityException("A record resolution is required for a color array as the one dimensional array does not indicate width and height.");

            Texture2D resultTexture = new Texture2D(recordResolution.width, recordResolution.height, solidify ? TextureFormat.RGB24 : TextureFormat.ARGB32, false);
            resultTexture.SetPixels32(colors);
            resultTexture = resultTexture.Cutout(cutout, false);
            colors = null;

            return resultTexture;
        }

        public void Destroy()
        {
            colors = null;

            if (!destroyOriginal) return;

            if (renderTexture != null)
                MonoBehaviourExtended.FlexibleDestroy(renderTexture);

            if (rawTexture != null)
                MonoBehaviourExtended.FlexibleDestroy(rawTexture);

            if(processedTexture != null)
                MonoBehaviourExtended.FlexibleDestroy(processedTexture);
        }
    }
}