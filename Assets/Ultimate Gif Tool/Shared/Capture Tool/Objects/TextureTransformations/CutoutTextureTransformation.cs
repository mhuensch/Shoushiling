using UnityEngine;

using TRS.CaptureTool.Extras;

namespace TRS.CaptureTool
{
    [System.Serializable]
    public class CutoutTextureTransformation : TextureTransformation
    {
        public Rect rect;
        public bool positionRelative;

        public void Init(Rect rect, bool positionRelative)
        {
            this.rect = rect;
            this.positionRelative = positionRelative;
        }

        public override string TransformationName()
        {
            return "Cutout";
        }

        public override string[] PropertyNames()
        {
            return new string[] { "rect", "positionRelative" };
        }

        public override string LabelForPropertyName(string propertyName)
        {
            switch (propertyName)
            {
                case "rect":
                    return "Rect";
                case "positionRelative":
                    return "Position Relative";
                default:
                    throw new UnityException("Unhandled property name");
            }
        }

        public override Texture2D ApplyTransformation(Texture2D texture, bool apply = true, bool destroyOriginal = true)
        {
            if (!active) return texture;

            Rect finalRect = rect;
            if (positionRelative)
                finalRect = rect.ReverseNormalize(new Vector2(texture.width, texture.height));
            return texture.Cutout(finalRect, apply, destroyOriginal);
        }

        public override TextureTransformation Clone()
        {
            CutoutTextureTransformation clone = ScriptableObject.CreateInstance<CutoutTextureTransformation>();
            clone.active = this.active;
            clone.rect = this.rect;
            clone.positionRelative = this.positionRelative;
            return clone;
        }
    }
}