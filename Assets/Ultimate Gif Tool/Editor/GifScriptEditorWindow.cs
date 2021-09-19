using UnityEngine;
using UnityEditor;

using TRS.CaptureTool.Share;
namespace TRS.CaptureTool
{
    public class GifScriptEditorWindow : EditorWindow
    {
        GifScript gifScript;
        Editor gifScriptEditor;

        ShareScript shareScript;
        UpdateShareWithGifScript updateShareWithGifScript;

        GameObject temp;
        Vector2 scrollPos;

        GUIStyle labelGUIStyle;

        [MenuItem("Tools/Ultimate Gif Tool/Editor Window", false, 0)]
        static void Init()
        {
            GifScriptEditorWindow editorWindow = (GifScriptEditorWindow)GetWindow(typeof(GifScriptEditorWindow));
            GUIContent titleContent = new GUIContent("Gif");
            editorWindow.autoRepaintOnSceneChange = true;
            editorWindow.titleContent = titleContent;
            editorWindow.Show();
        }

        void OnEnable()
        {
            if (temp == null)
                temp = new GameObject { hideFlags = HideFlags.HideAndDontSave };
            if (gifScript == null)
                gifScript = temp.AddComponent<GifScript>();
            if (gifScriptEditor == null)
                gifScriptEditor = Editor.CreateEditor(gifScript);

            if (shareScript == null)
                shareScript = temp.AddComponent<ShareScript>();
            if (updateShareWithGifScript == null)
                updateShareWithGifScript = temp.AddComponent<UpdateShareWithGifScript>();

            gifScript.editorWindowMode = true;
        }

        void OnDestroy()
        {
            if (temp != null)
                DestroyImmediate(temp);
        }

        void OnGUI()
        {
            if (labelGUIStyle == null)
            {
                labelGUIStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
                labelGUIStyle.fontStyle = FontStyle.Bold;
                labelGUIStyle.fontSize = 12;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            gifScriptEditor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();
        }
    }
}