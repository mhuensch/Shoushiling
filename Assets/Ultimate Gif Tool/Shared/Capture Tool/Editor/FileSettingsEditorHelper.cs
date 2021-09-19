using UnityEngine;
using UnityEditor;

using TRS.CaptureTool.Extras;
namespace TRS.CaptureTool
{
    public static class FileSettingsEditorHelper
    {
        public const string toggleStyle = "ToolbarButton";

        public static void CaptureFileSettingsFields(CaptureFileSettings fileSettings, SerializedProperty fileSettingsProperty, bool includeUseStreamingAssets = false, bool editorWindowMode = false, bool includeCameraSetting = true)
        {
            string saveType = fileSettingsProperty.FindPropertyRelative("saveType").stringValue;

            string saveTypeSingular = saveType;
            if (saveTypeSingular.EndsWith("s", System.StringComparison.Ordinal))
                saveTypeSingular = saveTypeSingular.Substring(0, saveTypeSingular.Length - 1); // remove s

            GUILayout.Label("Save Path", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            string currentDirectory = fileSettingsProperty.FindPropertyRelative("cachedEditorDirectory").stringValue;
            fileSettingsProperty.FindPropertyRelative("cachedEditorDirectory").stringValue = EditorGUILayout.TextField(currentDirectory, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
                OpenFolderPanelForSavePath(fileSettings, fileSettingsProperty);
            EditorGUILayout.EndHorizontal();

            string folderHelpText = "A folder name ending with ~ (like /" + saveType + "~) will be ignored by Unity and is useful for excluding files and avoiding reimports when switching builds.";
            if (includeUseStreamingAssets)
                folderHelpText = "Some file types must be saved to the StreamingAssets folder to be loaded in game from a save file. " + folderHelpText;
            EditorGUILayout.HelpBox(folderHelpText, MessageType.Info);

            if (!editorWindowMode)
            {
                bool showStandaloneSettings = CustomEditorGUILayout.BoldFoldoutForProperty(fileSettingsProperty.FindPropertyRelative("showStandaloneSettings"), "Standalone Save Settings");
                if (showStandaloneSettings)
                {
                    if (includeUseStreamingAssets)
                    {
                        CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("useStreamingAssetsPath"));
                        if (fileSettingsProperty.FindPropertyRelative("useStreamingAssetsPath").boolValue)
                            EditorGUILayout.HelpBox("All directories are relative to the streaming asset path. (Required to load some file types in game from a save file.)", MessageType.Info);
                        else
                            EditorGUILayout.HelpBox("All directories are relative to the data path. (Files may not be loaded in game from a save file.)", MessageType.Info);
                    }

                    CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("linuxDirectory"));
                    CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("macDirectory"));
                    CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("windowsDirectory"));

                    if (!includeUseStreamingAssets)
                        EditorGUILayout.HelpBox("All directories are relative to the data path.", MessageType.Info);
                }
                bool showMobileSettings = CustomEditorGUILayout.BoldFoldoutForProperty(fileSettingsProperty.FindPropertyRelative("showMobileSettings"), "Mobile Save Settings");
                if (showMobileSettings)
                {
                    CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("saveToGallery"));
                    if (fileSettingsProperty.FindPropertyRelative("saveToGallery").boolValue)
                    {
                        CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("androidAlbum"));
                        CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("iosAlbum"), new GUIContent("iOS Album"));
                    }

                    CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("persistLocallyMobile"), new GUIContent("Persist Locally"));
                    if (fileSettingsProperty.FindPropertyRelative("persistLocallyMobile").boolValue)
                    {
                        CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("androidDirectory"));
                        CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("iosDirectory"), new GUIContent("iOS Directory"));
                    }
                    EditorGUILayout.HelpBox("Persist to use the " + saveTypeSingular.ToLower() + " later (gallery photos aren't directly accessible). Please remember to delete unused " + saveType.ToLower() + " from the user's device.\n\nDirectories are relative to the persistant data path.", MessageType.Info);
                }

                bool showWebSettings = CustomEditorGUILayout.BoldFoldoutForProperty(fileSettingsProperty.FindPropertyRelative("showWebSettings"), "Web Save Settings");
                if (showWebSettings)
                {
                    CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("openInNewTab"));
                    CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("download"));
                    if (fileSettingsProperty.FindPropertyRelative("download").boolValue)
                        CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("webFileName"), new GUIContent("File Name"));

                    CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("persistLocallyWeb"), new GUIContent("Persist Locally"));
                    if (fileSettingsProperty.FindPropertyRelative("persistLocallyWeb").boolValue)
                        CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("webDirectory"));
                    EditorGUILayout.HelpBox("Persist to use the " + saveTypeSingular.ToLower() + " later (not directly accessible otherwise). Please remember to delete unused " + saveType.ToLower() + " from the user's device.\n\nDirectories are relative to the persistant data path.", MessageType.Info);
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("File Name Settings", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Example File Name:     " + fileSettings.ExampleFileName());

            CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("prefix"), new GUIContent("Custom Prefix"));

            Color originalColor = GUI.color;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            fileSettingsProperty.FindPropertyRelative("includeProject").boolValue = GUILayout.Toggle(fileSettingsProperty.FindPropertyRelative("includeProject").boolValue, "Project", toggleStyle);
            if(includeCameraSetting)
                fileSettingsProperty.FindPropertyRelative("includeCamera").boolValue = GUILayout.Toggle(fileSettingsProperty.FindPropertyRelative("includeCamera").boolValue, "Camera", toggleStyle);
            fileSettingsProperty.FindPropertyRelative("includeDate").boolValue = GUILayout.Toggle(fileSettingsProperty.FindPropertyRelative("includeDate").boolValue, "Date", toggleStyle);
            fileSettingsProperty.FindPropertyRelative("includeResolution").boolValue = GUILayout.Toggle(fileSettingsProperty.FindPropertyRelative("includeResolution").boolValue, "Resolution", toggleStyle);
            fileSettingsProperty.FindPropertyRelative("includeCounter").boolValue = GUILayout.Toggle(fileSettingsProperty.FindPropertyRelative("includeCounter").boolValue, "Counter", toggleStyle);
            if (EditorGUI.EndChangeCheck())
                fileSettingsProperty.FindPropertyRelative("fileNameSettingsChanged").boolValue = true;
            GUI.color = originalColor;
            if (fileSettingsProperty.FindPropertyRelative("includeCounter").boolValue)
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("Reset Counter", toggleStyle))
                {
                    fileSettings.ResetCount();
                    fileSettings.SaveCount();
                }
            }

            EditorGUILayout.EndHorizontal();
            if (!fileSettingsProperty.FindPropertyRelative("fileNameSettingsChanged").boolValue)
                EditorGUILayout.HelpBox("Toggle the above tabs to add or remove that component from the filename. (And to dismiss this message.)", MessageType.Info);

            EditorGUILayout.Space();

            if (fileSettingsProperty.FindPropertyRelative("includeDate").boolValue)
            {
                EditorGUILayout.Space();
                CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("dateFormat"));
            }
        }

        public static void ScreenshotFileSettingsFields(ScreenshotFileSettings screenshotFileSettings, SerializedProperty fileSettingsProperty, bool includeUseStreamingAssets = false, bool editorWindowMode = false, bool includeCameraSetting = true)
        {
            CaptureFileSettingsFields(screenshotFileSettings, fileSettingsProperty, includeUseStreamingAssets, editorWindowMode, includeCameraSetting);

            EditorGUILayout.BeginHorizontal();
            ScreenshotFileSettings.FileType fileType = (ScreenshotFileSettings.FileType)fileSettingsProperty.FindPropertyRelative("fileType").intValue;

            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle(fileType == ScreenshotFileSettings.FileType.PNG, "PNG", toggleStyle);
            if (EditorGUI.EndChangeCheck())
                fileSettingsProperty.FindPropertyRelative("fileType").intValue = (int)ScreenshotFileSettings.FileType.PNG;

            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle(fileType == ScreenshotFileSettings.FileType.JPG, "JPG", toggleStyle);
            if (EditorGUI.EndChangeCheck())
                fileSettingsProperty.FindPropertyRelative("fileType").intValue = (int)ScreenshotFileSettings.FileType.JPG;
            EditorGUILayout.EndHorizontal();

            if (fileSettingsProperty.FindPropertyRelative("fileType").intValue == (int)ScreenshotFileSettings.FileType.JPG)
            {
                int currentQuality = fileSettingsProperty.FindPropertyRelative("jpgQuality").intValue;
                fileSettingsProperty.FindPropertyRelative("jpgQuality").intValue = EditorGUILayout.IntSlider("JPG Quality", currentQuality, 1, 100);
            }
            else
            {
                CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("allowTransparency"));
                EditorGUILayout.HelpBox("With Unity's standard alpha blending, projects with transparency will create partially transparent screenshots. Leave unchecked to ensure an alpha of 1. (See ReadMe to learn more and get the effect you want.)", MessageType.Info);
            }

            CustomEditorGUILayout.PropertyField(fileSettingsProperty.FindPropertyRelative("includeLanguageInPath"));
        }

        public static void RequestSavePath(FileSettings fileSettings, SerializedProperty fileSettingsProperty)
        {
            string currentDirectory = fileSettingsProperty.FindPropertyRelative("cachedEditorDirectory").stringValue;
            if (currentDirectory == "")
                OpenFolderPanelForSavePath(fileSettings, fileSettingsProperty);
        }

        public static void OpenFolderPanelForSavePath(FileSettings fileSettings, SerializedProperty fileSettingsProperty)
        {
            string saveType = fileSettingsProperty.FindPropertyRelative("saveType").stringValue;
            string currentDirectory = fileSettingsProperty.FindPropertyRelative("cachedEditorDirectory").stringValue;
            string newDirectory = EditorUtility.SaveFolderPanel("Path to " + saveType + " Taken in Editor", currentDirectory, Application.dataPath);
            if (newDirectory.Length > 0)
                fileSettingsProperty.FindPropertyRelative("cachedEditorDirectory").stringValue = newDirectory;
            fileSettingsProperty.serializedObject.ApplyModifiedProperties();

            fileSettings.SaveEditorDirectory();
            EditorGUIUtility.ExitGUI();
        }
    }
}
