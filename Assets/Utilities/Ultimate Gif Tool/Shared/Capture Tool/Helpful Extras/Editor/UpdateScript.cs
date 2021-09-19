using UnityEngine;
using UnityEditor;
#if UNITY_5_4_OR_NEWER
using UnityEditor.SceneManagement;
#endif

using TRS.CaptureTool.Share;
using TRS.CaptureTool.Extras;
namespace TRS.CaptureTool
{
    [InitializeOnLoad]
    public static class UpdateScript
    {
        const string TOOL_VERSION = "TRS_TOOL_VERSION_KEY";

#if UNITY_5_4_OR_NEWER
        private static string currentScene;

        static UpdateScript()
        {
            currentScene = EditorSceneManager.GetActiveScene().name;
#if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged += HierarchyWindowChanged;
#else
            EditorApplication.hierarchyWindowChanged += HierarchyWindowChanged;
#endif
        }
#endif

        public static CaptureToolConfig MigratedConfig()
        {
            System.Type captureToolKeysClass = System.Reflection.Assembly.GetAssembly(typeof(CaptureScript)).GetType("TRS.CaptureTool.Share.CaptureToolKeys");
            if (captureToolKeysClass != null)
            {
                CaptureToolConfig config = CreateCaptureToolConfig.CreateWithFileName("MigratedConfig");

                config.imgurFreeMode = (bool)captureToolKeysClass.GetField("IMGUR_FREE_MODE").GetValue(null);
                config.imgurAnonymousMode = (bool)captureToolKeysClass.GetField("IMGUR_ANONYMOUS_MODE").GetValue(null);

                config.imgurClientId = captureToolKeysClass.GetField("IMGUR_CLIENT_ID").GetValue(null) as string;
                config.imgurClientSecret = captureToolKeysClass.GetField("IMGUR_CLIENT_SECRET").GetValue(null) as string;
                config.imgurRefreshToken = captureToolKeysClass.GetField("IMGUR_REFRESH_TOKEN").GetValue(null) as string;
                config.imgurXMashapeKey = captureToolKeysClass.GetField("IMGUR_X_MASHAPE_KEY").GetValue(null) as string;
                config.imgurAccessToken = captureToolKeysClass.GetMethod("ImgurAccessToken").Invoke(null, null) as string;

                config.giphyApiKey = captureToolKeysClass.GetField("GIPHY_API_KEY").GetValue(null) as string;
                config.giphyUsername = captureToolKeysClass.GetField("GIPHY_USERNAME").GetValue(null) as string;

                config.twitterConsumerKey = captureToolKeysClass.GetField("TWITTER_CONSUMER_KEY").GetValue(null) as string;
                config.twitterConsumerSecret = captureToolKeysClass.GetField("TWITTER_CONSUMER_SECRET").GetValue(null) as string;

                config.twitterDeveloperUsername = captureToolKeysClass.GetField("TWITTER_USERNAME").GetValue(null) as string;
                config.twitterDeveloperAccessToken = captureToolKeysClass.GetField("TWITTER_ACCESS_TOKEN").GetValue(null) as string;
                config.twitterDeveloperAccessTokenSecret = captureToolKeysClass.GetField("TWITTER_ACCESS_TOKEN_SECRET").GetValue(null) as string;

                config.facebookAppId = captureToolKeysClass.GetField("FB_APP_ID").GetValue(null) as string;
                return config;
            }

            return null;
        }

        public static void UpdateIfNecessary()
        {
            double savedToolVersion = System.Convert.ToDouble(SavedToolVersion());
            double currentToolVersion = System.Convert.ToDouble(ToolInfo.ToolVersion());
            if (savedToolVersion < currentToolVersion)
            {
                if (savedToolVersion < 3.26)
                    DeleteScriptWithName("AbstractGifFrame");

                if (savedToolVersion < 3.25) {
                    string[] filePaths = System.IO.Directory.GetFiles("Assets/", "*.jar", System.IO.SearchOption.AllDirectories);
                    foreach (string filePath in filePaths)
                    {
                        string filename = System.IO.Path.GetFileName(filePath);

                        // Delete old jar file that has become an aar file.
                        if (filename == "NativeGallery.jar")
                        {
                           System.IO.File.Delete(filePath);
                           System.IO.File.Delete(filePath + ".meta");
                        }
                    }

                    filePaths = System.IO.Directory.GetFiles("Assets/", "*.cs", System.IO.SearchOption.AllDirectories);
                    foreach (string filePath in filePaths)
                    {
                        string filename = System.IO.Path.GetFileName(filePath);

                        // Delete old copy of file that was moved to shared.
                        if (filename == "MultiLangScreenshotScript.cs")
                        {
                            if(!filePath.Contains("Shared")) {
                                System.IO.File.Delete(filePath);
                                System.IO.File.Delete(filePath + ".meta");
                            }
                        }

                        // Delete old copy of file that was moved out of shared.
                        if (filename == "ScreenshotResolutionTransformationReorderableList.cs")
                        {
                            if (filePath.Contains("Shared"))
                            {
                                System.IO.File.Delete(filePath);
                                System.IO.File.Delete(filePath + ".meta");
                            }
                        }
                    }
                }

                if (savedToolVersion <= 3.21)
                    DeleteScriptWithName("FrameTransformationReorderableList");

                if (savedToolVersion <= 2.07)
                {
                    // Delete CutoutRenderer.cs, Cutout.cs, and CaptureToolKeys.cs
                    // Also delete Renderer folder containing CutoutRenderer.cs if possible
                    string[] csFileNames = { "CutoutRenderer.cs", "Cutout.cs", "CaptureToolKeys.cs" };
                    foreach (string fileName in csFileNames)
                    {
                        string filePath = PathExtensions.GetFilePath("Assets/", "*.cs", fileName);
                        if (string.IsNullOrEmpty(filePath))
                            continue;

                        if (fileName == "CutoutRenderer.cs")
                        {
                            string parentDirectory = System.IO.Path.GetDirectoryName(filePath);
                            if (string.IsNullOrEmpty(parentDirectory))
                            {
                                System.IO.File.Delete(filePath);
                                System.IO.File.Delete(filePath + ".meta");
                                continue;
                            }
                            System.IO.Directory.Delete(parentDirectory, true);
                            System.IO.File.Delete(parentDirectory + ".meta");
                        }
                        else
                        {
                            System.IO.File.Delete(filePath);
                            System.IO.File.Delete(filePath + ".meta");
                        }

                    }
                }

                SaveToolVersion(ToolInfo.ToolVersion());
            }

            UpdateSceneIfNecessary();
        }

        public static void DeleteScriptWithName(string scriptName)
        {
            string filePath = PathExtensions.GetFilePath("Assets/", "*.cs", scriptName);
            if (string.IsNullOrEmpty(filePath)) return;

            System.IO.File.Delete(filePath);
            System.IO.File.Delete(filePath + ".meta");
        }

        public static void UpdateSceneIfNecessary()
        {
            // Migrate from CutoutGraphicScript to CutoutScript
            CutoutGraphicScript[] cutoutGraphicScripts = Object.FindObjectsOfType(typeof(CutoutGraphicScript)) as CutoutGraphicScript[];
            foreach (CutoutGraphicScript cutoutGraphicScript in cutoutGraphicScripts)
            {
                if (cutoutGraphicScript.gameObject.GetComponent<CutoutScript>() == null)
                    cutoutGraphicScript.gameObject.AddComponent<CutoutScript>();
                MonoBehaviourExtended.FlexibleDestroy(cutoutGraphicScript);
            }
        }

#if UNITY_5_4_OR_NEWER
        private static void HierarchyWindowChanged()
        {
            string newScene = EditorSceneManager.GetActiveScene().name;
            if (currentScene != newScene)
            {
                currentScene = newScene;
                UpdateSceneIfNecessary();
            }
        }
#endif

        #region Track Tool Version
        public static string SavedToolVersion()
        {
            if (PlayerPrefs.HasKey(TOOL_VERSION))
                return PlayerPrefs.GetString(TOOL_VERSION);

            return null;
        }

        public static void SaveToolVersion(string toolVersion)
        {
            PlayerPrefs.SetString(TOOL_VERSION, toolVersion);
            PlayerPrefs.Save();
        }
        #endregion
    }
}