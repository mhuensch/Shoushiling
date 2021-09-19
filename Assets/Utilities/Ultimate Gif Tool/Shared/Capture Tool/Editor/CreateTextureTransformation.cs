using UnityEngine;
using UnityEditor;

namespace TRS.CaptureTool
{
    public class CreateTextureTransformation : EditorWindow
    {
        const string TEXTURE_TRANSFORMATIONS_FOLDER_RELATIVE_TO_ASSETS = "Packages/CaptureTool/TextureTransformations";
        public const string CONFIG_FOLDER = "Assets/" + TEXTURE_TRANSFORMATIONS_FOLDER_RELATIVE_TO_ASSETS;

        [MenuItem("Assets/Create/Texture Transformation/Cutout")]
        public static TextureTransformation CreateCutoutTextureTransformation()
        {
            CutoutTextureTransformation asset = ScriptableObject.CreateInstance<CutoutTextureTransformation>();
            return CreateWithFileName(asset, "CutoutTextureTransformation.asset");
        }

        [MenuItem("Assets/Create/Texture Transformation/Layer Behind")]
        public static TextureTransformation CreateLayerBehindTextureTransformation()
        {
            LayerBehindTextureTransformation asset = ScriptableObject.CreateInstance<LayerBehindTextureTransformation>();
            return CreateWithFileName(asset, "LayerBehindTextureTransformation.asset");
        }

        [MenuItem("Assets/Create/Texture Transformation/Layer In Front")]
        public static TextureTransformation CreateLayerInFrontTextureTransformation()
        {
            LayerInFrontTextureTransformation asset = ScriptableObject.CreateInstance<LayerInFrontTextureTransformation>();
            return CreateWithFileName(asset, "LayerInFrontTextureTransformation.asset");
        }

        [MenuItem("Assets/Create//Texture Transformation/Resize")]
        public static TextureTransformation CreateResizeTextureTransformation()
        {
            ResizeTextureTransformation asset = ScriptableObject.CreateInstance<ResizeTextureTransformation>();
            return CreateWithFileName(asset, "ResizeTextureTransformation.asset");
        }

        [MenuItem("Assets/Create/Texture Transformation/Solidify")]
        public static TextureTransformation CreateSolidiftTextureTransformation()
        {
            SolidifyTextureTransformation asset = ScriptableObject.CreateInstance<SolidifyTextureTransformation>();
            return CreateWithFileName(asset, "SolidifyTextureTransformation.asset");
        }

        public static TextureTransformation CreateWithFileName(TextureTransformation asset, string fileName)
        {
            if (!fileName.EndsWith(".asset", System.StringComparison.InvariantCulture))
                fileName += ".asset";

            string separatorString = System.IO.Path.DirectorySeparatorChar.ToString();
            string configFolderInNativeFormat = Application.dataPath + separatorString + string.Join(separatorString, TEXTURE_TRANSFORMATIONS_FOLDER_RELATIVE_TO_ASSETS.Split('/'));
            System.IO.Directory.CreateDirectory(configFolderInNativeFormat);
            string finalFilePath = AssetDatabase.GenerateUniqueAssetPath(CONFIG_FOLDER + "/" + fileName);
            AssetDatabase.CreateAsset(asset, finalFilePath);
            AssetDatabase.SaveAssets();
            return asset;
        }
    }
}