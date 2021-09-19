using UnityEngine;

namespace TRS.CaptureTool
{
    [System.Serializable]
    public class FileSettings
    {
        protected const string CONNECTING_TEXT = "_";
        protected const string COUNT_KEY = "TRS_FS_COUNT";
        protected const string BASE_EDITOR_DIRECTORY_KEY = "TRS_FS_EDITOR_SAVE_PATH";

        public string directory
        {
            get
            {
#if UNITY_EDITOR
                return editorDirectory;
#else
#if UNITY_STANDALONE
                string applicationPath = useStreamingAssetsPath ? Application.streamingAssetsPath : Application.dataPath;
#else
                string applicationPath = Application.persistentDataPath; // Cannot save to streaming assets on Android, iOS, or WebGL
#endif

#if UNITY_STANDALONE_LINUX
                return System.IO.Path.Combine(applicationPath, linuxDirectory);
#elif UNITY_STANDALONE_OSX
                return System.IO.Path.Combine(applicationPath, macDirectory);
#elif UNITY_STANDALONE_WIN
                return System.IO.Path.Combine(applicationPath, windowsDirectory);
#elif UNITY_ANDROID
                return System.IO.Path.Combine(applicationPath, androidDirectory);
#elif UNITY_IOS
                return System.IO.Path.Combine(applicationPath, iosDirectory);
#else
                return applicationPath;
#endif
#endif
            }
        }

        public virtual string encoding { get; set; }
        public string extension
        {
            get
            {
                return "." + encoding;
            }
        }

        public string prefix;
        public bool includeProject = true;
        public bool includeDate;
        public string dateFormat;
        public bool includeCounter = true;

        int cachedCount;
        bool countLoaded;
        [SerializeField]
        protected string countKey = COUNT_KEY;
        public int count { get { if (countLoaded) return cachedCount; return LoadCount(); } }
        public bool useStreamingAssetsPath;

        [UnityEngine.Serialization.FormerlySerializedAs("folderType")]
        public string saveType;
        public string editorDirectory { get { if (editorDirectoryLoaded) return cachedEditorDirectory; return LoadEditorDirectory(); } }

        protected int uniqueId;
        public bool editorDirectoryLoaded;
        public string cachedEditorDirectory;
        public string editorDirectoryKey;

        public string linuxDirectory;
        public string macDirectory;
        public string windowsDirectory;

        public bool persistLocallyMobile;
        public string androidDirectory;
        public string iosDirectory;

        [UnityEngine.Serialization.FormerlySerializedAs("openImageInNewTab")]
        public bool openInNewTab;
        [UnityEngine.Serialization.FormerlySerializedAs("downloadImage")]
        public bool download;

        public string webFileName;
        public string fullWebFileName { get { return webFileName + extension; } }

        public bool persistLocallyWeb;
        public string webDirectory;

        [SerializeField]
        protected bool setup;

#if UNITY_EDITOR
#pragma warning disable 0414
        [SerializeField]
        bool fileNameSettingsChanged;
        [SerializeField]
        bool showStandaloneSettings;
        [SerializeField]
        bool showMobileSettings;
        [SerializeField]
        bool showWebSettings;
#pragma warning restore 0414
#endif

        // SetUp is used instead of init to prevent error:
        // get_dataPath is not allowed to be called from a MonoBehaviour constructor(or instance field initializer), call it in Awake or Start instead.Called from MonoBehaviour 'GifScript' on game object 'Screenshot Tool(Clone) (1)'.
        public virtual void SetUp(int uniqueId, string saveType = "Files")
        {
            // Prefab must be refreshed as machine may be different from the one that last saved the prefab
            LoadCount();
            LoadEditorDirectory();
            if (setup)
                return;

            setup = true;
            includeProject = true;
            includeCounter = true;
            countKey = COUNT_KEY;
            dateFormat = "yyyy-MM-dd-HH-mm-ss";

            this.uniqueId = uniqueId;
            this.saveType = saveType;
            editorDirectoryKey = BASE_EDITOR_DIRECTORY_KEY + uniqueId;
            LoadEditorDirectory();

            string windowsPath = "..";
            string unixPath = System.IO.Path.Combine(windowsPath, "..");
            string folder = Application.productName.Replace(" ", "") + "-" + saveType;
            linuxDirectory = System.IO.Path.Combine(unixPath, folder);
            macDirectory = System.IO.Path.Combine(unixPath, folder);
            windowsDirectory = System.IO.Path.Combine(windowsPath, folder);
            webDirectory = folder;

            openInNewTab = true;
            webFileName = folder;
        }

        public string FullFilePath(string folderName = "", string fileName = "", bool createDirectory = true)
        {
            string fullFilePath = System.IO.Path.Combine(FullFolderPath(folderName, createDirectory), fileName == "" ? FileName() : fileName);
            fullFilePath = fullFilePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
            fullFilePath = fullFilePath.Replace('\\', System.IO.Path.DirectorySeparatorChar);
            return fullFilePath;
        }

        public string FullFolderPath(string folderName = "", bool createDirectory = true)
        {
            string fullFolderPath = directory;
            if (folderName.Length > 0)
                fullFolderPath = System.IO.Path.Combine(fullFolderPath, folderName);

            fullFolderPath = fullFolderPath.Replace('/', System.IO.Path.DirectorySeparatorChar);
            fullFolderPath = fullFolderPath.Replace('\\', System.IO.Path.DirectorySeparatorChar);
            foreach (char invalidChar in System.IO.Path.GetInvalidPathChars())
                fullFolderPath = fullFolderPath.Replace(invalidChar.ToString(), "");

            if (createDirectory)
            {
                try
                {
                    System.IO.Directory.CreateDirectory(fullFolderPath);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Encountered exception attempting to create directory: " + e);
                }
            }
            return fullFolderPath;
        }

        public string FolderNameFromComponents(string[] components)
        {
            string folderName = CombineComponents(components);
            folderName = folderName.Replace(":", "x").Replace("\"", "in");
            folderName = folderName.Replace('/', System.IO.Path.DirectorySeparatorChar);
            folderName = folderName.Replace('\\', System.IO.Path.DirectorySeparatorChar);
            foreach (char invalidChar in System.IO.Path.GetInvalidPathChars())
                folderName = folderName.Replace(invalidChar.ToString(), "");
            return folderName;
        }

        public string FileName(string[] additionalComponents)
        {
            string additionalComponent = "";
            for(int i = 0; i < additionalComponents.Length; ++i) {
                additionalComponent += additionalComponents[i];
                if (i != additionalComponents.Length - 1)
                    additionalComponent += CONNECTING_TEXT;
            }

            return FileName(additionalComponent);
        }

        public string FileName(string additionalComponent = "")
        {
            string fileName = prefix != null ? prefix : "";

            if (includeProject)
                fileName += CONNECTING_TEXT + Application.productName;

            fileName += additionalComponent;

            if (includeDate)
                fileName += CONNECTING_TEXT + System.DateTime.Now.ToString(dateFormat);

            if (includeCounter)
                fileName += CONNECTING_TEXT + count.ToString("D3");

            if (fileName.StartsWith(CONNECTING_TEXT, System.StringComparison.Ordinal) && fileName.Length > CONNECTING_TEXT.Length)
                fileName = fileName.Substring(CONNECTING_TEXT.Length);

            fileName += extension;

            fileName = fileName.Replace(" ", "");
            foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalidChar.ToString(), "");

            return fileName;
        }

        protected string ConnectParts(params string[] parts)
        {
            string result = "";
            for (int i = 0; i < parts.Length; ++i)
            {
                if (result.Length > 0)
                    result += CONNECTING_TEXT;
                result += parts[i];
            }

            result = result.Replace(" ", "");
            return result;
        }
        protected string CombineComponents(params string[] components)
        {
            if (components.Length == 0)
                return "";

            string result = components[0];
            for (int i = 1; i < components.Length; ++i)
                result = System.IO.Path.Combine(result, components[i]);

            result = result.Replace(" ", "");
            return result;
        }

        #region Track Editor Directory
        string LoadEditorDirectory()
        {
            if (PlayerPrefs.HasKey(editorDirectoryKey))
                cachedEditorDirectory = PlayerPrefs.GetString(editorDirectoryKey);
            if (string.IsNullOrEmpty(cachedEditorDirectory))
            {
                cachedEditorDirectory = System.IO.Path.Combine(Application.dataPath, saveType + "~");
                System.IO.Directory.CreateDirectory(cachedEditorDirectory);
                SaveEditorDirectory();
            }

            editorDirectoryLoaded = true;
            return cachedEditorDirectory;
        }

        public void SaveEditorDirectory()
        {
            PlayerPrefs.SetString(editorDirectoryKey, cachedEditorDirectory);
            PlayerPrefs.Save();
        }
        #endregion

        #region Track Count
        int LoadCount()
        {
            if (PlayerPrefs.HasKey(countKey))
                cachedCount = PlayerPrefs.GetInt(countKey);
            else
                cachedCount = 0;

            countLoaded = true;
            return cachedCount;
        }

        public void ResetCount()
        {
            cachedCount = 0;
        }

        public void IncrementCount()
        {
            ++cachedCount;
        }

        public void SetCount(int newCount)
        {
            cachedCount = newCount;
        }

        public void SaveCount()
        {
            PlayerPrefs.SetInt(countKey, cachedCount);
            PlayerPrefs.Save();
        }
        #endregion
    }
}