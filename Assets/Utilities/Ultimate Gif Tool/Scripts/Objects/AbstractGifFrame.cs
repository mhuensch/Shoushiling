using UnityEngine;
using TRS.CaptureTool.Extras;

namespace TRS.CaptureTool
{
    [System.Serializable]
    public class AbstractGifFrame
    {
        [SerializeField]
        RenderTexture renderTexture;
        [SerializeField]
        Texture2D rawTexture;
        [SerializeField]
        Color32[] colors;

        public Texture2D processedFrame;
        public bool processed { get { return processedFrame != null; } }

        public AbstractGifFrame(RenderTexture renderTexture)
        {
            this.renderTexture = renderTexture;
        }

        public AbstractGifFrame(Texture2D rawTexture)
        {
            this.rawTexture = rawTexture;
        }

        public AbstractGifFrame(Color32[] colors)
        {
            this.colors = colors;
        }

        public Texture2D ProcessFrame(Resolution recordResolution, Rect cutout, bool useCutout, bool useTransparency, Resolution resizeResolution)
        {
            if (processedFrame != null)
                return processedFrame;

            if (renderTexture != null)
                processedFrame = ProcessRenderTexture(cutout, useCutout, useTransparency);
            else if (colors != null)
                processedFrame = ProcessColor32Array(recordResolution, cutout, useCutout, useTransparency);
            else
                processedFrame = rawTexture;

            processedFrame = ResizeIfNecessary(processedFrame, resizeResolution);

            return processedFrame;
        }

        public void DestroyFrame()
        {
            if (renderTexture != null)
                MonoBehaviourExtended.FlexibleDestroy(renderTexture);

            if (processedFrame != null)
                MonoBehaviourExtended.FlexibleDestroy(processedFrame);
            else if (rawTexture != null)
                MonoBehaviourExtended.FlexibleDestroy(rawTexture);
        }

        Texture2D ProcessRenderTexture(Rect cutout, bool useCutout, bool useTransparency)
        {
            RenderTexture.active = renderTexture;
            Rect textureRect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            if (useCutout)
                textureRect = cutout;

            Texture2D resultTexture = new Texture2D((int)textureRect.width, (int)textureRect.height, useTransparency ? TextureFormat.ARGB32 : TextureFormat.RGB24, false);
            resultTexture.ReadPixels(textureRect, 0, 0, false);

            RenderTexture.active = null;
            MonoBehaviourExtended.FlexibleDestroy(renderTexture);
            renderTexture = null;

            return resultTexture;
        }


        Texture2D ProcessColor32Array(Resolution recordResolution, Rect cutout, bool useCutout, bool useTransparency)
        {
            Texture2D resultTexture = new Texture2D(recordResolution.width, recordResolution.height, useTransparency ? TextureFormat.ARGB32 : TextureFormat.RGB24, false);
            resultTexture.SetPixels32(colors);
            if (useCutout)
                resultTexture = resultTexture.Cutout(cutout, false);
            colors = null;

            return resultTexture;
        }

        Texture2D ResizeIfNecessary(Texture2D texture, Resolution resizeResolution)
        {
            if (resizeResolution.HasSize())
                TextureScale.Bilinear(texture, resizeResolution.width, resizeResolution.height);
            else
                texture.Apply(false);

            return texture;
        }
    }
}