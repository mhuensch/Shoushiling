using UnityEngine;

using TRS.CaptureTool.Extras;

namespace TRS.CaptureTool
{
    [System.Serializable]
    public class LayerInFrontTextureTransformation : TextureTransformation
    {
        public Texture2D otherLayer;
        public Vector2 otherLayerPosition;
        public TextureTransformation.LayerPositionPoint otherLayerPositionPoint;
        public bool otherLayerPositionIsRelative;
        public Color emptySpaceFillColor;
        public bool useAlphaBlend = true;

        public void Init(Texture2D otherLayer, Vector2 otherLayerPosition, TextureTransformation.LayerPositionPoint otherLayerPositionPoint, bool otherLayerPositionIsRelative, Color emptySpaceFillColor, bool useAlphaBlend = true)
        {
            this.otherLayer = otherLayer;
            this.otherLayerPosition = otherLayerPosition;
            this.otherLayerPositionPoint = otherLayerPositionPoint;
            this.otherLayerPositionIsRelative = otherLayerPositionIsRelative;
            this.emptySpaceFillColor = emptySpaceFillColor;
            this.useAlphaBlend = useAlphaBlend;
        }

        public override string TransformationName()
        {
            return "Layer In Front";
        }

        public override string[] PropertyNames()
        {
            return new string[] { "otherLayer", "otherLayerPosition", "otherLayerPositionPoint", "otherLayerPositionIsRelative", "emptySpaceFillColor", "useAlphaBlend" };
        }

        public override string LabelForPropertyName(string propertyName)
        {
            switch (propertyName)
            {
                case "otherLayer":
                    return "Other Layer";
                case "otherLayerPosition":
                    return "Other Layer Position";
                case "otherLayerPositionPoint":
                    return "Other Layer Position Point";
                case "otherLayerPositionIsRelative":
                    return "Relative Position";
                case "emptySpaceFillColor":
                    return "Empty Space Fill Color";
                case "useAlphaBlend":
                    return "Use Alpha Blend";
                default:
                    throw new UnityException("Unhandled property name");
            }
        }

        public override Texture2D ApplyTransformation(Texture2D texture, bool apply = true, bool destroyOriginal = true)
        {
            if (!active) return texture;

            Texture2D background = texture;
            Texture2D foreground = otherLayer;

            Vector2Int position;
            if (otherLayerPositionIsRelative)
                position = new Vector2Int(Mathf.FloorToInt(background.width * otherLayerPosition.x),
                                          Mathf.FloorToInt(background.height * otherLayerPosition.y));
            else
                position = new Vector2Int(Mathf.FloorToInt(otherLayerPosition.x), Mathf.FloorToInt(otherLayerPosition.y));

            if (otherLayerPositionPoint == LayerPositionPoint.Center)
                position = new Vector2Int(position.x + (background.width - foreground.width) / 2,
                                          position.y + (background.height - foreground.height) / 2);

            Debug.Log("HERE LAYERF");


            Texture2D result = background.Blend(foreground, position, emptySpaceFillColor, useAlphaBlend, apply, false);
            if (destroyOriginal) texture.DestroyIfPossible();
            return result;
        }

        public override TextureTransformation Clone()
        {
            LayerInFrontTextureTransformation clone = ScriptableObject.CreateInstance<LayerInFrontTextureTransformation>();
            clone.active = this.active;
            clone.otherLayer = this.otherLayer;
            clone.otherLayerPosition = this.otherLayerPosition;
            clone.otherLayerPositionPoint = this.otherLayerPositionPoint;
            clone.otherLayerPositionIsRelative = this.otherLayerPositionIsRelative;
            clone.emptySpaceFillColor = this.emptySpaceFillColor;
            clone.useAlphaBlend = this.useAlphaBlend;
            return clone;
        }
    }
}