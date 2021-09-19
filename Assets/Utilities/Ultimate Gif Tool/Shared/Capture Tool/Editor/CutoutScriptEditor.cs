using UnityEngine;
using UnityEditor;

using TRS.CaptureTool.Extras;
namespace TRS.CaptureTool
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(CutoutScript))]
    public class CutoutScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = !EditorApplication.isCompiling && !EditorApplication.isUpdating;
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            ((CutoutScript)target).preview = EditorGUILayout.Toggle("Preview", ((CutoutScript)target).preview);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(target);

            CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("positionRelative"));
            CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("clickToSelectPivot"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}