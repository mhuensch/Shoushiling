using System.Collections.Generic;

using UnityEngine;

namespace TRS.CaptureTool
{
    public class DemoGifCleanUpScript : MonoBehaviour
    {
        public GifScript gifScript;

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL)
        const string TEMP_DIRECTORY = "Temp";

        bool originalWebPersistValue;
        string originalWebDirectory;

        bool originalMobilePersistValue;
        string originalIOSDirectory;
        string originalAndroidDirectory;

        List<string> createdMediaFiles = new List<string>();

        void OnEnable()
        {
            // There is not direct access to media saved to gallery or to desktop from web, so we need our own persisted file to access (and upload/share) these screenshots
            originalWebPersistValue = gifScript.fileSettings.persistLocallyWeb;
            originalWebDirectory = gifScript.fileSettings.webDirectory;

            originalMobilePersistValue = gifScript.fileSettings.persistLocallyMobile;
            originalAndroidDirectory = gifScript.fileSettings.androidDirectory;
            originalIOSDirectory = gifScript.fileSettings.iosDirectory;

            gifScript.fileSettings.persistLocallyWeb = true;
            gifScript.fileSettings.webDirectory = TEMP_DIRECTORY;

            gifScript.fileSettings.persistLocallyMobile = true;
            gifScript.fileSettings.androidDirectory = TEMP_DIRECTORY;
            gifScript.fileSettings.iosDirectory = TEMP_DIRECTORY;

            // Two alternatives demonstrated here. Periodically delete all files in a specific folder or track and delete the persisted files.
            // Both are done here as OnDisable may not be called or finished during an app crash, move to the background, or other close event
            foreach (string filePath in System.IO.Directory.GetFiles(gifScript.fileSettings.directory))
                System.IO.File.Delete(filePath);

            GifScript.GifSaved += GifSaved;
        }

        void OnDisable()
        {
            GifScript.GifSaved -= GifSaved;

            foreach (string mediaFilePath in createdMediaFiles)
                System.IO.File.Delete(mediaFilePath);

            gifScript.fileSettings.persistLocallyWeb = originalWebPersistValue;
            gifScript.fileSettings.webDirectory = originalWebDirectory;

            gifScript.fileSettings.persistLocallyMobile = originalMobilePersistValue;
            gifScript.fileSettings.androidDirectory = originalAndroidDirectory;
            gifScript.fileSettings.iosDirectory = originalIOSDirectory;
        }

        void GifSaved(GifScript gifScript, string filePath)
        {
            createdMediaFiles.Add(filePath);
        }
#endif
    }
}