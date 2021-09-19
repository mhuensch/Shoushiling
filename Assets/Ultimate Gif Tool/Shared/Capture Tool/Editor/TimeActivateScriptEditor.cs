using UnityEngine;
using UnityEditor;

using TRS.CaptureTool;
using TRS.CaptureTool.Extras;
namespace TRS.CaptureTool.Share
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(TimeActivateScript))]
    public class TimeActivateScriptEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            /*
                        TimeActivateScript timeActivateScript = (TimeActivateScript)target;
                        if (GUILayout.Button(timeActivateScript.forceHidden ? "Show" : "Force Hide"))
                        {
                            timeActivateScript.forceHidden = !timeActivateScript.forceHidden;
                            EditorUtility.SetDirty(target);
                        }
            */
            serializedObject.ApplyModifiedProperties();
        }
    }
}