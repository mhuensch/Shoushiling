using UnityEngine;

namespace TRS.CaptureTool.Extras
{
    public static class Texture2DExtensions
    {
        public static Texture2D ApplyTransformation(this Texture2D texture, TextureTransformation textureTransformation, bool apply = true, bool destroyOriginal = true)
        {
            return textureTransformation.ApplyTransformation(texture, apply, destroyOriginal);
        }

        public static Texture2D ApplyTransformations(this Texture2D texture, TextureTransformation[] textureTransformations, bool apply = true, bool destroyOriginals = true)
        {
            Texture2D editableTexture = EditableTexture(texture, true);
            for (int i = 0; i < textureTransformations.Length; ++i)
            {
                TextureTransformation frameTransformation = textureTransformations[i];
                editableTexture = frameTransformation.ApplyTransformation(editableTexture, false, destroyOriginals);
            }

            if(apply)
                editableTexture.Apply(false);
            return editableTexture;
        }

        public static Texture2D Cutout(this Texture2D original, Rect cutoutRect, bool apply = true, bool destroyOriginal = true)
        {
            if (cutoutRect == Rect.zero || cutoutRect == new Rect(0, 0, original.width, original.height))
                return original;

            bool editableOriginal = original.format == original.EditableTextureFormat();
            Color[] pixels = original.GetPixels((int)cutoutRect.x, (int)cutoutRect.y, (int)cutoutRect.width, (int)cutoutRect.height);
            Texture2D resultTexture = new Texture2D((int)cutoutRect.width, (int)cutoutRect.height, original.EditableTextureFormat(), original.mipmapCount > 1);
            resultTexture.SetPixels(pixels);
            if (apply)
                resultTexture.Apply(false);


            if (editableOriginal && destroyOriginal)
                MonoBehaviourExtended.FlexibleDestroy(original);

            return resultTexture;
        }

        public static Texture2D Solidify(this Texture2D texture, bool apply = true)
        {
            if (texture.format == TextureFormat.RGB24)
                return texture;

            var resultPixels = texture.GetPixels();
            for (var i = 0; i < resultPixels.Length; ++i)
                resultPixels[i].a = 1f;

            Texture2D resultTexture = texture;
            if (texture.format != texture.EditableTextureFormat())
                resultTexture = new Texture2D(texture.width, texture.height, texture.EditableTextureFormat(), texture.mipmapCount > 1);

            resultTexture.SetPixels(resultPixels);
            if (apply)
                resultTexture.Apply(false);
            return resultTexture;
        }

        public static Texture2D Resize(this Texture2D texture, Resolution resizeResolution, bool apply = true)
        {
            if (!resizeResolution.HasSize() || (texture.width == resizeResolution.width && texture.height == resizeResolution.height))
                return texture;

            Texture2D editableTexture = EditableTexture(texture);
            TextureScale.Bilinear(texture, resizeResolution.width, resizeResolution.height, apply);
            return editableTexture;
        }

        // Based on : https://answers.unity.com/questions/1008802/merge-multiple-png-images-one-on-top-of-the-other.html
        public static Texture2D Blend(this Texture2D background, Texture2D foreground, bool alphaBlend, bool apply = true, bool destroyOriginals = true)
        {
            return background.Blend(foreground, new Vector2Int(0, 0), Color.clear, alphaBlend, apply, destroyOriginals);
        }

        public static Texture2D AlphaBlend(this Texture2D background, Texture2D foreground, bool apply = true, bool destroyOriginals = true)
        {
            return background.Blend(foreground, new Vector2Int(0, 0), Color.clear, true, apply, destroyOriginals);
        }

        public static Texture2D SLBlend(this Texture2D background, Texture2D foreground, bool apply = true, bool destroyOriginals = true)
        {
            return background.Blend(foreground, new Vector2Int(0, 0), Color.clear, false, apply, destroyOriginals);
        }

        public static Texture2D AlphaBlend(this Texture2D background, Texture2D foreground, Vector2Int position, Color emptySpaceFillColor, bool apply = true, bool destroyOriginals = true)
        {
            return background.Blend(foreground, position, emptySpaceFillColor, true, apply, destroyOriginals);
        }

        public static Texture2D SLBlend(this Texture2D background, Texture2D foreground, Vector2Int position, Color emptySpaceFillColor, bool apply = true, bool destroyOriginals = true)
        {
            return background.Blend(foreground, position, emptySpaceFillColor, false, apply, destroyOriginals);
        }

        // Inspired by this method: https://answers.unity.com/questions/1008802/merge-multiple-png-images-one-on-top-of-the-other.html
        // Modified to allow different sized textures.
        // Position is from top left. Negative offsets are supported.
        public static Texture2D Blend(this Texture2D background, Texture2D foreground, Vector2Int position, Color emptySpaceFillColor, bool alphaBlend = true, bool apply = true, bool destroyOriginals = true)
        {
            if (emptySpaceFillColor == null) emptySpaceFillColor = Color.clear;

            Color[] backgroundPixels = background.GetPixels();
            Color[] foregroundPixels = foreground.GetPixels();

            int resultWidth;
            if (position.x < 0)
                resultWidth = Mathf.Abs(position.x) + Mathf.Max(foreground.width + position.x, background.width);
            else
                resultWidth = Mathf.Max(position.x + foreground.width, background.width);

            int resultHeight;
            if (position.y < 0)
                resultHeight = Mathf.Abs(position.y) + Mathf.Max(foreground.height + position.y, background.height);
            else
                resultHeight = Mathf.Max(position.y + foreground.height, background.height);

            Color[] resultPixels = new Color[resultWidth * resultHeight];
            for (int row = 0; row < resultHeight; row++)
            {
                int backgroundPixelRow = -1;
                if (row + position.y >= 0)
                {
                    backgroundPixelRow = position.y < 0 ? row + position.y : row;
                    if (backgroundPixelRow >= background.height) backgroundPixelRow = -1;
                }

                int foregroundPixelRow = -1;
                if(position.y <= row)
                {
                    foregroundPixelRow = position.y < 0 ? row : row - position.y;
                    if (foregroundPixelRow >= foreground.height) foregroundPixelRow = -1;
                }

                for (int col = 0; col < resultWidth; col++)
                {
                    int backgroundPixelIndex = -1;
                    if(backgroundPixelRow >= 0 && col + position.x >= 0)
                    {
                        int backgroundPixelCol = position.x < 0 ? col + position.x : col;
                        if (backgroundPixelCol >= background.width) backgroundPixelCol = -1;
                        if (backgroundPixelCol >= 0) backgroundPixelIndex = backgroundPixelRow * background.width + backgroundPixelCol;
                    }

                    int foregroundPixelIndex = -1;
                    if(foregroundPixelRow >= 0 && position.x <= col)
                    {
                        int foregroundPixelCol = position.x < 0 ? col : col - position.x;
                        if (foregroundPixelCol >= foreground.width) foregroundPixelCol = -1;
                        if (foregroundPixelCol >= 0) foregroundPixelIndex = foregroundPixelRow * foreground.width + foregroundPixelCol;
                    }

                    int resultPixelIndex = row * resultWidth + col;
                    if (backgroundPixelIndex < 0 && foregroundPixelIndex < 0)
                        resultPixels[resultPixelIndex] = emptySpaceFillColor;
                    else if (foregroundPixelIndex < 0)
                        resultPixels[resultPixelIndex] = backgroundPixels[backgroundPixelIndex];
                    else if (backgroundPixelIndex < 0)
                        resultPixels[resultPixelIndex] = foregroundPixels[foregroundPixelIndex];
                    else
                    {
                        Color backgroundColor = backgroundPixels[backgroundPixelIndex];
                        Color foregroundColor = foregroundPixels[foregroundPixelIndex];
                        if (alphaBlend)
                            resultPixels[resultPixelIndex] = backgroundColor.AlphaBlend(foregroundColor);
                        else
                            resultPixels[resultPixelIndex] = backgroundColor.SLBlend(foregroundColor);
                    }

                    if (resultPixels[resultPixelIndex].a <= 0)
                        resultPixels[resultPixelIndex] = Color.clear;
                }
            }

            Texture2D resultTexture = new Texture2D(resultWidth, resultHeight, TextureFormat.ARGB32, background.mipmapCount > 1 || foreground.mipmapCount > 1);
            resultTexture.SetPixels(resultPixels);
            if (apply)
                resultTexture.Apply(false);

            if (destroyOriginals)
            {
                background.DestroyIfPossible();
                foreground.DestroyIfPossible();
            }
            return resultTexture;
        }

        public static void SetColor(this Texture2D texture, Color color, bool apply = true)
        {
            Color[] resultPixels = texture.GetPixels();
            for (int i = 0; i < resultPixels.Length; ++i)
                resultPixels[i] = color;

            texture.SetPixels(resultPixels);
            if (apply)
                texture.Apply(false);
        }

        #region Utilities
        public static TextureFormat EditableTextureFormat(this Texture2D texture)
        {
            if (texture.format == TextureFormat.ARGB32 || texture.format == TextureFormat.RGBA32 || texture.format == TextureFormat.RGB24)
                return texture.format;
            return TextureFormat.ARGB32;
        }

        public static Texture2D EditableTexture(this Texture2D texture, bool forceCopy = false, bool apply = false)
        {
            if (!forceCopy && texture.format == texture.EditableTextureFormat())
                return texture;

            if(!forceCopy)
                Debug.LogWarning("Texture is not editable, so making an editable copy.");

            Color[] resultPixels = texture.GetPixels();
            Texture2D resultTexture = new Texture2D(texture.width, texture.height, texture.EditableTextureFormat(), texture.mipmapCount > 1);
            resultTexture.SetPixels(resultPixels);
            if (apply)
                resultTexture.Apply(false);
            return resultTexture;
        }

        public static byte[] ToBytes(this Texture2D texture, string extension, int jpgQuality = 100)
        {
            byte[] textureBytes = null;
            try
            {
                if (extension.Equals(".png"))
                    textureBytes = texture.EncodeToPNG();
                else if (extension.Equals(".jpg") || extension.Equals(".jpeg"))
                    textureBytes = texture.EncodeToJPG(jpgQuality);
            }
            catch (UnityException)
            {
                return texture.EditableTexture(true, true).ToBytes(extension, jpgQuality);
            }
            catch (System.ArgumentException)
            {
                return texture.EditableTexture(true, true).ToBytes(extension, jpgQuality);
            }
            return textureBytes;
        }

        // Load from any texture. Texture size does not matter, since LoadImage will replace with with incoming image size.
        // Example: Texture2D fileTexture = (new Texture2D(0, 0)).LoadFromFilePath(filePath);
        public static Texture2D LoadFromFilePath(this Texture2D texture, string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError("No texture exists at: " + filePath);
                return null;
            }
            byte[] textureBytes = System.IO.File.ReadAllBytes(filePath);
            texture.LoadImage(textureBytes);
            return texture;
        }

        /* Save texture to file path. Return value bool indicates if successful. */
        public static bool SaveToFilePath(this Texture2D texture, string filePath, int jpgQuality = 100)
        {
            string extension = System.IO.Path.GetExtension(filePath);
            return SaveToFilePath(texture.ToBytes(extension, jpgQuality), filePath);
        }

        static bool SaveToFilePath(byte[] textureBytes, string filePath)
        {
            try
            {
                System.IO.File.WriteAllBytes(filePath, textureBytes);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Exception attempting to save texture: " + e);
                return false;
            }

            return true;
        }

        public static bool SaveAccordingToFileSettings(this Texture2D texture, ScreenshotFileSettings fileSettings, string overrideFilePath = "")
        {
            string fullFilePath = string.IsNullOrEmpty(overrideFilePath) ? fileSettings.FullFilePath() : overrideFilePath;
            string extension = System.IO.Path.GetExtension(fullFilePath);
            byte[] textureBytes = null;
            if (fileSettings.fileType == ScreenshotFileSettings.FileType.PNG)
                textureBytes = texture.EncodeToPNG();
            else if (fileSettings.fileType == ScreenshotFileSettings.FileType.JPG)
                textureBytes = texture.EncodeToJPG(fileSettings.jpgQuality);
            else
                textureBytes = texture.ToBytes(extension, fileSettings.jpgQuality);

            bool persist = true;
#if !UNITY_EDITOR
#if UNITY_IOS || UNITY_ANDROID
            persist &= fileSettings.persistLocallyMobile;
#elif UNITY_WEBGL
            persist &= fileSettings.persistLocallyWeb;
#endif
#endif
#pragma warning disable
            bool savedSuccessfully = false;
#pragma warning restore
            if (persist)
                savedSuccessfully = SaveToFilePath(textureBytes, fullFilePath);

#if !UNITY_EDITOR
            string fileName = System.IO.Path.GetFileNameWithoutExtension(fullFilePath) + fileSettings.extension;
#if UNITY_IOS || UNITY_ANDROID
            if (fileSettings.saveToGallery)
            {
                if (persist)
                    NativeGallery.SaveImageToGallery(fullFilePath, fileSettings.album, fileName, (success, path) => { if(!success) { Debug.LogError("Save to NativeGallery.SaveImageToGallery Failed"); } });
                else
                    NativeGallery.SaveImageToGallery(textureBytes, fileSettings.album, fileName, (success, path) => { if(!success) { Debug.LogError("Save to NativeGallery.SaveImageToGallery Failed"); } });
            }
#elif UNITY_WEBGL
            SaveAccordingToFileSettingsWeb(textureBytes, fileSettings, fileName);
#endif
#endif

            return savedSuccessfully;
        }

#if !UNITY_EDITOR && UNITY_WEBGL
        [System.Obsolete("Process is deprecated to use a more descriptive name. Please use SaveAccordingToFileSettingsWeb")]
        public static void Process(byte[] bytes, CaptureFileSettings fileSettings, string overrideFileName = "") {
            SaveAccordingToFileSettingsWeb(bytes, fileSettings, overrideFileName);
        }

        public static void SaveAccordingToFileSettingsWeb(byte[] bytes, CaptureFileSettings fileSettings, string overrideFileName = "")
        {
            if (!fileSettings.openInNewTab && !fileSettings.download)
                return;

            string encodedText = System.Convert.ToBase64String(bytes);
            string fileName = string.IsNullOrEmpty(overrideFileName) ? fileSettings.fullWebFileName : overrideFileName;
            processImage(encodedText, fileName, fileSettings.encoding, fileSettings.openInNewTab, fileSettings.download);
        }

        [System.Runtime.InteropServices.DllImport("__Internal")]
        static extern void processImage(string url, string fileName, string type, bool display, bool download);
#endif

        public static bool IsEqual(this Texture2D a, Texture2D b)
        {
            Color[] aPixels = a.GetPixels();
            Color[] bPixels = b.GetPixels();
            if (aPixels.Length != bPixels.Length)
                return false;
            for (int i = 0; i < aPixels.Length; i++)
            {
                if (aPixels[i] != bPixels[i])
                {
                    Debug.Log("I: " + i + "\nA: " + aPixels[i] + "\nB: " + bPixels[i]);
                    return false;
                }
            }

            return true;
        }

        public static void DestroyIfPossible(this Texture2D texture)
        {
            bool editable = texture.format == texture.EditableTextureFormat();
            if (!editable) return;

            MonoBehaviourExtended.FlexibleDestroy(texture);
        }
        #endregion

        #region Obsolete

        [System.Obsolete("SmallerForegroundAlphaBlend is deprecated for more general methods. Use AlphaBlend instead.")]
        public static Texture2D SmallerForegroundAlphaBlend(this Texture2D background, Texture2D foreground, Vector2Int position, bool apply = true, bool destroyOriginals = true)
        {
            return background.Blend(foreground, position, Color.clear, true, apply, destroyOriginals);
        }

        [System.Obsolete("SmallerForegroundSLBlend is deprecated for more general methods. Use SLBlend instead.")]
        public static Texture2D SmallerForegroundSLBlend(this Texture2D background, Texture2D foreground, Vector2Int position, bool apply = true, bool destroyOriginals = true)
        {
            return background.Blend(foreground, position, Color.clear, false, apply, destroyOriginals);
        }

        [System.Obsolete("SmallerForegroundBlend is deprecated for more general methods. Use Blend instead.")]
        public static Texture2D SmallerForegroundBlend(this Texture2D background, Texture2D foreground, Vector2Int position, bool alphaBlend, bool apply = true, bool destroyOriginals = true)
        {
            return background.Blend(foreground, position, Color.clear, alphaBlend, apply, destroyOriginals);
        }

        [System.Obsolete("CaptureCameraRenderTexture is deprecated for cleaner methods. If using for other methods, copy to another file.")]
        public static void CaptureCameraRenderTexture(this Texture2D texture, Camera camera, bool apply = true)
        {
            Rect captureRect = new Rect(0, 0, camera.pixelWidth, camera.pixelHeight);
            CaptureCameraRenderTexture(texture, camera, captureRect, apply);
        }

        [System.Obsolete("CaptureCameraRenderTexture is deprecated for cleaner methods. If using for other methods, copy to another file.")]
        public static void CaptureCameraRenderTexture(this Texture2D texture, Camera camera, Rect rect, bool apply = true)
        {
            RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 24);
            RenderTexture originalRenderTexture = camera.targetTexture;

            camera.targetTexture = renderTexture;
            camera.Render();
            texture.ReadPixels(rect, 0, 0, false);
            if (apply)
                texture.Apply(false);

            camera.targetTexture = originalRenderTexture;
            RenderTexture.ReleaseTemporary(renderTexture);
        }

        [System.Obsolete("CaptureCameraRenderTexture is deprecated for cleaner methods. If using for other methods, copy to another file.")]
        public static void CaptureCameraRenderTexture(this Texture2D texture, Camera camera, RenderTexture renderTexture, bool apply = true)
        {
            Rect captureRect = new Rect(0, 0, camera.pixelWidth, camera.pixelHeight);
            CaptureCameraRenderTexture(texture, camera, renderTexture, captureRect, apply);
        }

        [System.Obsolete("CaptureCameraRenderTexture is deprecated for cleaner methods. If using for other methods, copy to another file.")]
        public static void CaptureCameraRenderTexture(this Texture2D texture, Camera camera, RenderTexture renderTexture, Rect rect, bool apply = true)
        {
            RenderTexture originalRenderTexture = camera.targetTexture;

            camera.targetTexture = renderTexture;
            camera.Render();
            texture.ReadPixels(rect, 0, 0, false);
            if (apply)
                texture.Apply(false);

            camera.targetTexture = originalRenderTexture;
        }

        [System.Obsolete("CaptureCameraRenderTexture is deprecated for cleaner methods. If using for other methods, copy to another file.")]
        public static void CaptureRenderTexture(this Texture2D texture, RenderTexture renderTexture, bool apply = true)
        {
            Rect captureRect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            CaptureRenderTexture(texture, renderTexture, captureRect, apply);
        }

        [System.Obsolete("CaptureCameraRenderTexture is deprecated for cleaner methods. If using for other methods, copy to another file.")]
        public static void CaptureRenderTexture(this Texture2D texture, RenderTexture renderTexture, Rect rect, bool apply = true)
        {
            RenderTexture originalRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            texture.ReadPixels(rect, 0, 0, false);
            if (apply)
                texture.Apply(false);
            RenderTexture.active = originalRenderTexture;
        }

        [System.Obsolete("CaptureCameraRenderTexture is deprecated for cleaner methods. If using for other methods, copy to another file.")]
        public static void CaptureRenderTexture(this Texture2D texture, bool apply = true)
        {
            Rect captureRect = new Rect(0, 0, texture.width, texture.height);
            CaptureRenderTexture(texture, captureRect, apply);
        }

        [System.Obsolete("CaptureCameraRenderTexture is deprecated for cleaner methods. If using for other methods, copy to another file.")]
        public static void CaptureRenderTexture(this Texture2D texture, Rect rect, bool apply = true)
        {
            RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 24);
            texture.CaptureRenderTexture(renderTexture, rect, apply);
            RenderTexture.ReleaseTemporary(renderTexture);
        }
        #endregion
    }
}