using UnityEngine;

namespace TRS.CaptureTool
{
    [System.Serializable]
    public class TextureTransformation : ScriptableObject
    {
        public bool active = true;

        [System.Serializable]
        public enum LayerPositionPoint
        {
            TopLeft,
            Center
        }

        public virtual string TransformationName()
        {
            throw new UnityException("Unhandled virtual method");
        }

        public virtual string[] PropertyNames()
        {
            throw new UnityException("Unhandled virtual method");
        }

        public virtual string LabelForPropertyName(string propertyName)
        {
            throw new UnityException("Unhandled virtual method");
        }

        public static TextureTransformation SolidifyTextureTransformation()
        {
            SolidifyTextureTransformation textureTransformation = ScriptableObject.CreateInstance<SolidifyTextureTransformation>();
            textureTransformation.Init();
            return textureTransformation;
        }

        public static TextureTransformation CutoutTextureTransformation(Rect rect, bool positionRelative)
        {
            CutoutTextureTransformation textureTransformation = ScriptableObject.CreateInstance<CutoutTextureTransformation>();
            textureTransformation.Init(rect, positionRelative);
            return textureTransformation;
        }

        public static TextureTransformation ResizeTextureTransformation(Resolution resolution)
        {
            ResizeTextureTransformation textureTransformation = ScriptableObject.CreateInstance<ResizeTextureTransformation>();
            textureTransformation.Init(resolution);
            return textureTransformation;
        }

        public static TextureTransformation LayerBehindTextureTransformation(Texture2D otherLayer, Vector2 otherLayerPosition, LayerPositionPoint otherLayerPositionPoint, bool otherLayerPositionIsRelative, Color emptySpaceFillColor, bool useAlphaBlend = true)
        {
            LayerBehindTextureTransformation textureTransformation = ScriptableObject.CreateInstance<LayerBehindTextureTransformation>();
            textureTransformation.Init(otherLayer, otherLayerPosition, otherLayerPositionPoint, otherLayerPositionIsRelative, emptySpaceFillColor, useAlphaBlend);
            return textureTransformation;
        }

        public static TextureTransformation LayerInFrontTextureTransformation(Texture2D otherLayer, Vector2 otherLayerPosition, LayerPositionPoint otherLayerPositionPoint, bool otherLayerPositionIsRelative, Color emptySpaceFillColor, bool useAlphaBlend = true)
        {
            LayerInFrontTextureTransformation textureTransformation = ScriptableObject.CreateInstance<LayerInFrontTextureTransformation>();
            textureTransformation.Init(otherLayer, otherLayerPosition, otherLayerPositionPoint, otherLayerPositionIsRelative, emptySpaceFillColor, useAlphaBlend);
            return textureTransformation;
        }

        public virtual Texture2D ApplyTransformation(Texture2D texture, bool apply = true, bool destroyOriginal = true)
        {
            throw new UnityException("Unhandled virtual method");
        }

        public virtual TextureTransformation Clone()
        {
            throw new UnityException("Unhandled virtual method");
        }
    }
}