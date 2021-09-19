using UnityEngine;

using TRS.CaptureTool.Extras;
namespace TRS.CaptureTool
{
    [System.Serializable]
    public class TransformableTexture
    {
        public bool destroyRawDataWhenFinalized = true;
        public bool destroyRawTextureWhenFinalized = true;

        public RawFrameData rawFrameData;
        public Texture2D rawTexture;
        public TextureTransformation[] transformations;

        public Texture2D finalTexture;
        public bool finalized {  get { return finalTexture != null;  } }

        public TransformableTexture(RawFrameData rawFrameData, TextureTransformation[] transformations)
        {
            this.rawFrameData = rawFrameData;
            this.transformations = transformations;
        }

        public TransformableTexture(Texture2D rawTexture, TextureTransformation[] transformations)
        {
            if(transformations != null)
            {
                this.rawTexture = rawTexture;
                this.transformations = transformations;
            } else
                this.finalTexture = rawTexture;
        }

        public Texture2D Process()
        {
            if (rawTexture != null) return rawTexture;

            if (rawFrameData != null)
            {
                if (rawFrameData.processed)
                    rawTexture = rawFrameData.processedTexture;
                else
                    rawTexture = rawFrameData.Process();

                if (destroyRawDataWhenFinalized)
                {
                    // Make a copy, so we don't delete it when we delete the rawFrameData.
                    rawTexture = rawTexture.EditableTexture(true);
                    rawFrameData.Destroy();
                }
            }

            return rawTexture;
        }

        public Texture2D Finalize()
        {
            if (finalTexture != null) return finalTexture;

            if (rawFrameData == null && rawTexture == null) throw new UnityException("Cannot finalize without either RawFrameData or a raw Texture2D.");

            Process();

            finalTexture = transformations != null ? rawTexture.ApplyTransformations(transformations, true) : rawTexture;
            if(destroyRawTextureWhenFinalized && rawTexture != finalTexture) MonoBehaviourExtended.FlexibleDestroy(rawTexture);

            return finalTexture;
        }

        public void Destroy()
        {
            if (rawFrameData != null) rawFrameData.Destroy();
            if(rawTexture != null) MonoBehaviourExtended.FlexibleDestroy(rawTexture);
            if(finalTexture != null) MonoBehaviourExtended.FlexibleDestroy(finalTexture);
        }
    }
}