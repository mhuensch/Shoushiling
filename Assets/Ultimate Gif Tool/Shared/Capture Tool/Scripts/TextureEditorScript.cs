using UnityEngine;
using System.Collections.Generic;

using TRS.CaptureTool.Extras;
namespace TRS.CaptureTool
{
    public class TextureEditorScript : MonoBehaviour
    {
        public Texture2D texture;
        public bool copyBeforeEdit = true;

        public TextureTransformation[] frameTransformations;

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
            Texture2D textureToTransform = texture;
            if (copyBeforeEdit)
                textureToTransform = texture.EditableTexture(true);
            return textureToTransform.ApplyTransformations(frameTransformations, true);
        }

        public void Save()
        {
            Texture2D resultTexture = Texture();
            string fullFilePath = fileSettings.FullFilePath("", fileSettings.FileNameWithCaptureDetails("", resultTexture.width + "x" + resultTexture.height));
            SaveToFilePath(resultTexture, fullFilePath);
        }

        public void SaveToFilePath(Texture2D textureToSave, string filePath)
        {
            bool savedSuccessfully = textureToSave.SaveToFilePath(filePath);
#if UNITY_EDITOR
            if (savedSuccessfully)
                Debug.Log("Saved combined texture to: " + filePath);
#endif
            lastSaveFilePath = filePath;
        }
    }
}