using UnityEngine;
using UnityEditor;

// Add hotkeys:
// https://unity3d.com/learn/tutorials/topics/interface-essentials/unity-editor-extensions-menu-items

namespace TRS.CaptureTool
{
    public class GifPrefabMenu : EditorWindow
    {
        readonly static string GIF_PREFAB_NAME = "GifTool";

        static GameObject gifTool;

        [MenuItem("Tools/Ultimate Gif Tool/Create Gif Prefab", false, 11)]
        static void CreateGifPrefab()
        {
            string[] gifToolGuids = AssetDatabase.FindAssets(GIF_PREFAB_NAME + " t:GameObject", null);
            if (gifToolGuids.Length > 1)
            {
                Debug.LogError("Multiple gif prefabs found. Please do not name another prefab '" + GIF_PREFAB_NAME + "' or change the constant in Ultimate Gif Tool/Editor/GifPrefabMenu.cs to give it a unique name, so the script can find it.");
                return;
            }
            else if (gifToolGuids.Length <= 0)
            {
                Debug.LogError("Gif prefab not found. You may have changed the prefab name. Please leave the prefab as '" + GIF_PREFAB_NAME + "' or update the constant in Ultimate Gif Tool/Editor/GifPrefabMenu.cs, so the script can find it.");
                return;
            }

            GameObject gifToolPrefab = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(gifToolGuids[0]), typeof(GameObject)) as GameObject;
            gifTool = Instantiate(gifToolPrefab) as GameObject;
            Undo.RegisterCreatedObjectUndo(gifTool, "Created Gif Tool");
        }

        [MenuItem("Tools/Ultimate Gif Tool/Select Gif Prefab", false, 12)]
        static void SelectGifPrefab()
        {
            if (gifTool == null)
            {
                GifScript gifScript = GameObject.FindObjectOfType<GifScript>() as GifScript;
                if (gifScript != null)
                    gifTool = gifScript.gameObject;
            }

            if (gifTool != null)
                Selection.activeGameObject = gifTool;
            else
                Debug.LogError("No gif tool in scene.");
        }

        [MenuItem("Tools/Ultimate Gif Tool/Destroy All Gif Prefabs", false, 13)]
        static void DestroyGifPrefabs()
        {
            GifScript[] gifScripts = GameObject.FindObjectsOfType<GifScript>() as GifScript[];
            foreach (GifScript gifScript in gifScripts)
                Undo.DestroyObjectImmediate(gifScript.gameObject);
        }
    }
}