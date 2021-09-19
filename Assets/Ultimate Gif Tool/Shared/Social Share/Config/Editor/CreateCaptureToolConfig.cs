using UnityEngine;
using UnityEditor;

namespace TRS.CaptureTool.Share
{
    public class CreateCaptureToolConfig : EditorWindow
    {
        const string CONFIG_FOLDER_RELATIVE_TO_ASSETS = "Packages/CaptureTool/Configs";
        public const string CONFIG_FOLDER = "Assets/" + CONFIG_FOLDER_RELATIVE_TO_ASSETS;

        [MenuItem("Assets/Create/Capture Tool Config")]
        public static CaptureToolConfig Create()
        {
            return CreateWithFileName("DefaultConfig.asset");
        }

        public static CaptureToolConfig CreateWithFileName(string fileName)
        {
            if (!fileName.EndsWith(".asset", System.StringComparison.InvariantCulture))
                fileName += ".asset";

            CaptureToolConfig asset = ScriptableObject.CreateInstance<CaptureToolConfig>();

            string separatorString = System.IO.Path.DirectorySeparatorChar.ToString();
            string configFolderInNativeFormat = Application.dataPath + separatorString + string.Join(separatorString, CONFIG_FOLDER_RELATIVE_TO_ASSETS.Split('/'));
            System.IO.Directory.CreateDirectory(configFolderInNativeFormat);
            string finalFilePath = AssetDatabase.GenerateUniqueAssetPath(CONFIG_FOLDER + "/" + fileName);
            AssetDatabase.CreateAsset(asset, finalFilePath);
            AssetDatabase.SaveAssets();
            return asset;
        }
    }
}