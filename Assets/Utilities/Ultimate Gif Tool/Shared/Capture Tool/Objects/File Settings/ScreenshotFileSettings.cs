namespace TRS.CaptureTool
{
    [System.Serializable]
    public class ScreenshotFileSettings : CaptureFileSettings
    {
        public enum FileType { PNG, JPG }
        public FileType fileType;

        public bool allowTransparency;
        public int jpgQuality = 100;

        public bool includeLanguageInPath = false;
        UnityEngine.SystemLanguage language;

        public ScreenshotFileSettings()
        {
            // MultiLangScreenshotScript.LanguageChanged += LanguageChanged;
        }

        ~ScreenshotFileSettings()
        {
            // MultiLangScreenshotScript.LanguageChanged -= LanguageChanged;
        }

        public override string encoding
        {
            get
            {
                if (fileType == FileType.JPG)
                    return "jpg";
                return "png";
            }
        }

        public override void SetUp(int uniqueId, string saveType = "Screenshots")
        {
            base.SetUp(uniqueId, saveType);

            if (setup)
                return;

            jpgQuality = 100;
        }

        public override string FullFilePathWithCaptureDetails(string cameraName, string resolutionName, string resolutionString)
        {
            if (includeLanguageInPath)
            {
                string folderName = FolderName(language.ToString(), ConnectParts(resolutionName, resolutionString));
                string fileName = FileNameWithCaptureDetails(cameraName, resolutionString);
                return FullFilePath(folderName, fileName);
            }

            return base.FullFilePathWithCaptureDetails(cameraName, resolutionName, resolutionString);
        }

        public void LanguageChanged(UnityEngine.SystemLanguage language)
        {
            this.language = language;
        }
    }
}