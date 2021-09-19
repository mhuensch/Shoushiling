﻿using UnityEngine;

using TRS.CaptureTool.Extras;
namespace TRS.CaptureTool
{
    public class TextureCombinator : MonoBehaviour
    {
        public Texture2D backgroundTexture;
        public Texture2D foregroundTexture;

        public Vector2 foregroundPosition;
        public TextureTransformation.LayerPositionPoint foregroundPositionPoint;
        public bool positionForegroundRelative = true;

        public Color emptySpaceFillColor;
        public bool useAlphaBlend = true;
        public bool solidify = true;

        public ScreenshotFileSettings fileSettings;

        public string lastSaveFilePath;

        #region Editor variables
#if UNITY_EDITOR
#pragma warning disable 0414
        [SerializeField]
        bool showSaveSettings = true;
#pragma warning restore 0414
#endif
        #endregion

        public Texture2D Texture()
        {
            TextureTransformation layerTransformation = TextureTransformation.LayerInFrontTextureTransformation(foregroundTexture, foregroundPosition, foregroundPositionPoint, positionForegroundRelative, emptySpaceFillColor, useAlphaBlend);
            Texture2D resultTexture = backgroundTexture.ApplyTransformation(layerTransformation, !solidify, false);
            if (solidify && resultTexture != null)
                resultTexture = resultTexture.Solidify(true);

            return resultTexture;
        }

        public void Save()
        {
            Texture2D resultTexture = Texture();
            string fullFilePath = fileSettings.FullFilePath("", fileSettings.FileNameWithCaptureDetails("", resultTexture.width + "x" + resultTexture.height));
            SaveAccordingToFileSettings(resultTexture, fullFilePath);
        }

        public void SaveToFilePath(Texture2D texture2D, string filePath)
        {
            bool savedSuccessfully = texture2D.SaveToFilePath(filePath);
#if UNITY_EDITOR
            if (savedSuccessfully)
                Debug.Log("Saved combined texture to: " + filePath);
#endif
            lastSaveFilePath = filePath;
        }

        public void SaveAccordingToFileSettings(Texture2D texture2D, string filePath)
        {
            bool savedSuccessfully = texture2D.SaveAccordingToFileSettings(fileSettings, filePath);
            fileSettings.IncrementCount();
            fileSettings.SaveCount();

#if UNITY_EDITOR
            if (savedSuccessfully)
                Debug.Log("Saved combined texture to: " + filePath);
#endif
            lastSaveFilePath = filePath;
        }
    }
}