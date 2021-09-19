using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using TRS.CaptureTool.Extras;

namespace TRS.CaptureTool
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(GifScript))]
    public class GifScriptEditor : CaptureScriptEditor
    {
        List<TransformableTexture> backupFrames = null;

        GUIStyle buttonGUIStyle;
        bool everAnythingToSave;
        bool anythingToSave;
        int lastFrameCount;

        bool playingPreview;
        bool reversingPreview;
        float lastFrameTime;

        int selectedFrame;
        int firstFrame;
        int lastFrame;

        string cachedFileSize;
        GifScript.RecorderState prevRecorderState = GifScript.RecorderState.None;

        // Force repaint if we need to update the preview frame or the recording or save progress bar
        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        protected override void OnEnable()
        {
            if (target == null)
                return;

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            if (buttonGUIStyle == null)
                buttonGUIStyle = new GUIStyle(GUI.skin.GetStyle("Button"));

            base.OnInspectorGUI();
        }

        #region Tabs
        protected override void CaptureTab()
        {
            bool showCaptureTransformations = CustomEditorGUILayout.BoldFoldoutForProperty(serializedObject, "showCaptureTransformations", "Capture Transformations");
            if (showCaptureTransformations)
                captureTransformationList.DoLayoutList();

            TempEnabledObjects();

            TempDisabledObjects();

            Timing(false);

            bool originalGUIEnabled = GUI.enabled;
            GifScript.RecorderState recorderState = ((GifScript)target).recorderState;
            bool fullyReset = recorderState == GifScript.RecorderState.None;
            GUI.enabled &= fullyReset;

            SizeSettings();
            RecordSettings();

            GUI.enabled = originalGUIEnabled;

            RecordButtons();
        }

        protected override void SaveTab()
        {
            Edit();

            SaveSettings();

            SaveProgressBar();

            SaveButtons();

            if (everAnythingToSave)
                EditorGUILayout.Space();

            OpenFileOrFolderButtons();
        }

        protected override void SettingsTab()
        {
            CaptureMode();

            GifScript.GifCaptureMode captureMode = (GifScript.GifCaptureMode)serializedObject.FindProperty("captureMode").intValue;
            if (captureMode != GifScript.GifCaptureMode.ScreenCapture)
                Cameras(captureMode == GifScript.GifCaptureMode.MultiCamera);

            if (captureMode != GifScript.GifCaptureMode.ScreenCapture)
                UICamera(serializedObject.FindProperty("fileSettings").FindPropertyRelative("saveType").stringValue);

            if (!((GifScript)target).editorWindowMode)
                HotKeys();

            AudioSettings();
            MouseSettings();
            DontDestroyOnLoadSettings();

            SupportButtons();
        }
        #endregion

        #region Capture Tab Settings
        void SizeSettings()
        {
            bool showSizeSettings = CustomEditorGUILayout.BoldFoldoutForProperty(serializedObject, "showSizeSettings", "Size Settings");
            if (showSizeSettings)
            {
                EditorGUI.BeginChangeCheck();
                bool originalGUIEnabled = GUI.enabled;
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("resizeBeforeRecording"), new GUIContent("Resize Before Recording", "Resizes screen before recording to increase quality of output and speed of capture. Resizing screen does not work on mobile."));
                if (serializedObject.FindProperty("resizeBeforeRecording").boolValue)
                {

                    string buttonName = serializedObject.FindProperty("useScaleToResizeBeforeRecording").boolValue ? "Resize By Scaling" : "Resize To Resolution";
                    if (GUILayout.Button(buttonName))
                        serializedObject.FindProperty("useScaleToResizeBeforeRecording").boolValue = !serializedObject.FindProperty("useScaleToResizeBeforeRecording").boolValue;

                    if (serializedObject.FindProperty("useScaleToResizeBeforeRecording").boolValue)
                    {
                        CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("recordWidthScale"));
                        CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("recordHeightScale"));
                        GUI.enabled = false;
                        EditorGUILayout.IntField("Record Width", ((GifScript)target).recordResolution.width);
                        EditorGUILayout.IntField("Record Height", ((GifScript)target).recordResolution.height);
                        GUI.enabled = originalGUIEnabled;
                    }
                    else
                    {
                        CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("recordWidth"));
                        CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("recordHeight"));
                    }
                }

                CutoutSetting();

                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("resizeAfterRecording"), new GUIContent("Resize After Recording", "Resizes each frame after recording. Resizing each frame does work on mobile."));
                if (serializedObject.FindProperty("resizeAfterRecording").boolValue)
                {
                    string buttonName = serializedObject.FindProperty("useScaleToResizeAfterRecording").boolValue ? "Resize By Scaling" : "Resize To Resolution";
                    if (GUILayout.Button(buttonName))
                        serializedObject.FindProperty("useScaleToResizeAfterRecording").boolValue = !serializedObject.FindProperty("useScaleToResizeAfterRecording").boolValue;

                    if (serializedObject.FindProperty("useScaleToResizeAfterRecording").boolValue)
                    {
                        CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("resizeScale"));
                        GUI.enabled = false;
                        EditorGUILayout.IntField("Resize Width", ((GifScript)target).resizeResolution.width);
                        EditorGUILayout.IntField("Resize Height", ((GifScript)target).resizeResolution.height);
                        GUI.enabled = originalGUIEnabled;
                        EditorGUILayout.HelpBox("Final size is relative to size during recording after any cutout is applied.", MessageType.Info);
                    }
                    else
                    {
                        CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("resizeWidth"));
                        GUI.enabled = false;
                        EditorGUILayout.IntField("Resize Height", ((GifScript)target).resizeHeight);
                        GUI.enabled = originalGUIEnabled;
                        EditorGUILayout.HelpBox("Resize height is automatically set to match the aspect ratio of the recorded frame after any cutout is applied.", MessageType.Info);
                    }
                }

                /*
                GifScript.RecorderState recorderState = ((GifScript)target).recorderState;
                bool fullyReset = recorderState == GifScript.RecorderState.None;
                if (!fullyReset)
                    EditorGUILayout.HelpBox("Reset before adjusting size settings.", MessageType.Info);
                */

                GUI.enabled = originalGUIEnabled;
                if (EditorGUI.EndChangeCheck())
                    localEditorUpdateQueue.Enqueue(((GifScript)target).ResolutionsChanged);
            }
        }

        protected override string CutoutTooltip()
        {
            return "Cutouts are applied after the frame is taken and before it is resized.";
        }

        void RecordSettings()
        {
            bool showRecordSettings = CustomEditorGUILayout.BoldFoldoutForProperty(serializedObject, "showRecordSettings", "Record Settings");
            if (showRecordSettings)
            {
                string adjustTimeForCaptureTooltip;
                if (serializedObject.FindProperty("adjustTimeForCapture").boolValue)
                    adjustTimeForCaptureTooltip = "Forces consistent delta times for best looking gifs. Toggle for smoother gameplay.";
                else
                    adjustTimeForCaptureTooltip = "May be better for recording without affecting gameplay. Toggle for a more consistent frame rate (better looking).";
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("adjustTimeForCapture"), new GUIContent("Adjust Time for Capture", adjustTimeForCaptureTooltip));

                string prepareTooltip = "Additional processing like resizing the frame can either be done when the frame is captured to save memory or when saving to lower processing during capture. ";
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("prepareForPreviewDuringFrameCapture"), new GUIContent("Auto Prepare for Preview", prepareTooltip));
                if (!serializedObject.FindProperty("prepareForPreviewDuringFrameCapture").boolValue)
                    CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("prepareForPreviewDuringFrameCaptureInEditor"), new GUIContent("Auto Prepare In Editor", prepareTooltip));

                Background();

                string useTransparencyTooltip;
                if (serializedObject.FindProperty("useTransparency").boolValue)
                    useTransparencyTooltip = "Gifs use a single color of transparency. Pixels with an alpha value of zero will be replaced with the transparent color. Ensure the transparency color is not a color you use or the sections in that color will also be transparent.";
                else
                    useTransparencyTooltip = "Toggle to enable transparency.";
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("useTransparency"), new GUIContent("Use Transparency", useTransparencyTooltip));

                if (serializedObject.FindProperty("useTransparency").boolValue)
                    CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("transparencyColor"));

                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("stopAutomatically"));
                if (serializedObject.FindProperty("stopAutomatically").boolValue)
                    EditorGUILayout.HelpBox("Will stop recording automatically when duration is reached.", MessageType.Info);
                else
                    EditorGUILayout.HelpBox("Will record until paused or saved. Always keeps the last " + serializedObject.FindProperty("durationInSeconds").intValue + " seconds.", MessageType.Info);

                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("captureRate"));
                GifScript.GifCaptureRate captureRate = (GifScript.GifCaptureRate)serializedObject.FindProperty("captureRate").intValue;
                bool vrHybrid = captureRate == GifScript.GifCaptureRate.VRHybrid;
                if (captureRate == GifScript.GifCaptureRate.FPS || vrHybrid)
                {
                    int oldCaptureFramesPerSecond = serializedObject.FindProperty("captureFramesPerSecond").intValue;
                    int newCaptureFramesPerSecond = EditorGUILayout.IntField("Capture Frames Per Second", oldCaptureFramesPerSecond);
                    if (newCaptureFramesPerSecond <= 0)
                        newCaptureFramesPerSecond = 1;
                    if (newCaptureFramesPerSecond != oldCaptureFramesPerSecond)
                    {
                        ((GifScript)target).SetCaptureFramesPerSecond(newCaptureFramesPerSecond);
                        EditorUtility.SetDirty(target);
                    }

                    if (!vrHybrid)
                        EditorGUILayout.HelpBox("Capture roughly every 1/framerate seconds according to unscaledDeltaTime.", MessageType.Info);
                }
                if (captureRate == GifScript.GifCaptureRate.EveryXFrames || vrHybrid)
                {
                    int oldCaptureEveryXFrame = serializedObject.FindProperty("captureEveryXFrame").intValue;
                    int newCaptureEveryXFrame = EditorGUILayout.IntField("Capture Every X Frame", oldCaptureEveryXFrame);
                    if (newCaptureEveryXFrame <= 0)
                        newCaptureEveryXFrame = 1;
                    if (newCaptureEveryXFrame != oldCaptureEveryXFrame)
                    {
                        ((GifScript)target).captureEveryXFrame = newCaptureEveryXFrame;
                        EditorUtility.SetDirty(target);
                    }

                    if (!vrHybrid)
                        EditorGUILayout.HelpBox("Capture every xth frame. Based on % frameCount. E.g. 2 would be every other frame", MessageType.Info);
                }
                else if (captureRate == GifScript.GifCaptureRate.NaturalTiming)
                    EditorGUILayout.HelpBox("Captures at every update. May slow down the frame rate.", MessageType.Info);

                if (vrHybrid)
                    EditorGUILayout.HelpBox("Captures a frame when it has been at least 1/fps seconds and % everyXFrame frameCount. Useful hybrid for VR to captureevery other frame at roughly a certain FPS. If using adjust time for capture, multiply your FPS by everyXFrame and use different playback speed in Eit & Save.", MessageType.Info);


                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("durationInSeconds"), new GUIContent("Duration (in seconds)"));
                if (serializedObject.FindProperty("durationInSeconds").intValue <= 0)
                    serializedObject.FindProperty("durationInSeconds").intValue = 1;

                //EditorGUILayout.HelpBox(estimatedPeakMemoryUseString(), MessageType.Info);
            }
        }

        void RecordButtons()
        {
            bool originalGUIEnabled = GUI.enabled;

            GifScript.RecorderState recorderState = ((GifScript)target).recorderState;
            bool processing = recorderState == GifScript.RecorderState.PreProcessing;
            bool recording = recorderState == GifScript.RecorderState.Recording;
            bool fullyReset = recorderState == GifScript.RecorderState.None;
            GUI.enabled &= !processing && Application.isPlaying;

            Rect position = EditorGUILayout.GetControlRect(false, 20);
            float delayStartTime = serializedObject.FindProperty("delayStartTime").floatValue;
            if (delayStartTime >= 0)
            {
                float timeSinceDelayStart = Time.realtimeSinceStartup - delayStartTime;
                float delayBeforeCapture = serializedObject.FindProperty("delayBeforeCapture").floatValue;
                EditorGUI.ProgressBar(position, timeSinceDelayStart / delayBeforeCapture, "Waiting " + timeSinceDelayStart.ToString("N2") + " / " + delayBeforeCapture.ToString("N2") + " seconds");
            }
            else
            {
                int totalFrameCount = ((GifScript)target).frames.Count;
                int maxFrameCount = ((GifScript)target).maxFrameCount;
                EditorGUI.ProgressBar(position, (float)totalFrameCount / (float)maxFrameCount, "Recorded " + totalFrameCount + " / " + maxFrameCount + " Frames");
            }

            if (GUILayout.Button("Record One Frame"))
                ((GifScript)target).RecordOneFrame();

            EditorGUILayout.BeginHorizontal();
            string buttonTitle = recording ? "Pause" : "Record";
            if (GUILayout.Button(buttonTitle, GUILayout.MinHeight(40)))
            {
                ((GifScript)target).ToggleRecording();
                if (((GifScript)target).saveProgress < 1f && !fullyReset)
                    Debug.LogWarning("Additional frames added after saving has started will not be added to the save file in progress.");
            }

            using (new EditorGUI.DisabledScope(fullyReset))
            {
                if (GUILayout.Button("Reset", GUILayout.MinHeight(40)))
                    ((GifScript)target).Reset();
            }
            EditorGUILayout.EndHorizontal();

            if (((GifScript)target).frames.Count > 0)
            {
                if (GUILayout.Button("Show Preview", GUILayout.MinHeight(40)))
                    SelectTab(Tab.Save);
            }

            GUI.enabled = originalGUIEnabled;

            if (!fullyReset)
                EditorGUILayout.HelpBox("Reset before adjusting record settings.", MessageType.Info);
        }
        #endregion

        #region Edit & Save Tab Settings
        void Edit()
        {
            bool showEdit = CustomEditorGUILayout.BoldFoldoutForProperty(serializedObject, "showEdit", "Edit");
            if (showEdit)
            {
                if (GUILayout.Button(new GUIContent("Edit Saved GIf", "Automatically edits last captured gif. Click to edit a gif saved to the streaming assets directory. This will overwrite any gif stored in memory. It will take a second to load.")))
                {
                    string saveDirectory = System.IO.Path.GetDirectoryName(((GifScript)target).fileSettings.FullFilePath());
                    string gifToEditFilePath = EditorUtility.OpenFilePanel("Gif to Edit", saveDirectory, "gif");
                    if (gifToEditFilePath != null && gifToEditFilePath.Length > 0)
                    {
                        ((GifScript)target).LoadGifAtFilePath(gifToEditFilePath);
                        EditorUtility.SetDirty(target);
                    }
                }

                // Automatically prepare for preview when loading the tab.
                if (!((GifScript)target).hasPreparedFrames)
                {
                    ((GifScript)target).PrepareFrames();
                    EditorUtility.SetDirty(target);
                }

                if (((GifScript)target).hasPreparedFrames)
                {
                    Texture2D[] preparedFrames = ((GifScript)target).preparedFrames;
                    if (lastFrameCount == 0)
                    {
                        firstFrame = 0;
                        lastFrame = preparedFrames.Length - 1;
                    }
                    else if (lastFrame == lastFrameCount - 1)
                        lastFrame = preparedFrames.Length - 1;
                    lastFrameCount = preparedFrames.Length;

                    selectedFrame = Mathf.Min(selectedFrame, preparedFrames.Length - 1);
                    firstFrame = Mathf.Min(firstFrame, preparedFrames.Length - 1);
                    lastFrame = Mathf.Min(lastFrame, preparedFrames.Length - 1);

                    if (playingPreview)
                    {
                        float playbackTimePerFrame = ((GifScript)target).playbackTimePerFrame;
                        if (Time.unscaledTime - lastFrameTime >= playbackTimePerFrame)
                        {
                            bool forceReverseMode = ((GifScript)target).reverseMode && !((GifScript)target).pingPongMode;
                            float excessTime = (Time.unscaledTime - lastFrameTime) - playbackTimePerFrame;
                            lastFrameTime = Time.unscaledTime - excessTime;
                            if (reversingPreview || forceReverseMode)
                                --selectedFrame;
                            else
                                ++selectedFrame;

                            if (selectedFrame > lastFrame)
                            {
                                if (serializedObject.FindProperty("pingPongMode").boolValue)
                                {
                                    selectedFrame = lastFrame - 1;
                                    reversingPreview = true;
                                }
                                else
                                    selectedFrame = firstFrame;
                            }

                            // Not else if so we can handle case where selectedFrame = lastFrame - 1 = -1
                            if (selectedFrame < firstFrame)
                            {
                                if(forceReverseMode)
                                    selectedFrame = lastFrame;
                                else {
                                    reversingPreview = false;
                                    selectedFrame = firstFrame + 1;
                                    if (selectedFrame > lastFrame)
                                        selectedFrame = lastFrame;
                                }
                            }
                        }
                    }

                    float previewScale = EditorGUIUtility.currentViewWidth / (float)((GifScript)target).recordWidth;
                    Rect position = EditorGUILayout.GetControlRect(false, ((GifScript)target).recordHeight * previewScale);
                    EditorGUI.DrawPreviewTexture(position, preparedFrames[selectedFrame], null, ScaleMode.ScaleToFit);

                    EditorGUILayout.BeginHorizontal();
                    buttonGUIStyle.fontSize = 24;
                    if (GUILayout.Button("↦", buttonGUIStyle, GUILayout.Height(35)))
                        firstFrame = selectedFrame;
                    buttonGUIStyle.fontSize = 14;
                    if (!playingPreview && GUILayout.Button("►", buttonGUIStyle, GUILayout.Height(35)))
                    {
                        playingPreview = true;
                        lastFrameTime = Time.unscaledTime;
                    }
                    buttonGUIStyle.fontSize = 28;
                    if (playingPreview && GUILayout.Button("■", buttonGUIStyle, GUILayout.Height(35)))
                        playingPreview = false;
                    buttonGUIStyle.fontSize = 24;
                    if (GUILayout.Button("↤", buttonGUIStyle, GUILayout.Height(35)))
                        lastFrame = selectedFrame;
                    EditorGUILayout.EndHorizontal();

                    selectedFrame = EditorGUILayout.IntSlider("Frame", selectedFrame, 0, preparedFrames.Length - 1);

                    EditorGUI.BeginChangeCheck();
                    float firstFrameFloat = (float)firstFrame;
                    float lastFrameFloat = (float)lastFrame;
                    EditorGUILayout.MinMaxSlider("Frames", ref firstFrameFloat, ref lastFrameFloat, 0, preparedFrames.Length - 1);
                    firstFrame = (int)firstFrameFloat;
                    lastFrame = (int)lastFrameFloat;

                    firstFrame = EditorGUILayout.IntField("First Frame", firstFrame);
                    lastFrame = EditorGUILayout.IntField("Last Frame", lastFrame);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (selectedFrame < firstFrame)
                            selectedFrame = firstFrame;
                        else if (selectedFrame > lastFrame)
                        {
                            if (serializedObject.FindProperty("pingPongMode").boolValue)
                                selectedFrame = lastFrame;
                            else
                                selectedFrame = firstFrame;
                        }

                        if (selectedFrame < firstFrame || selectedFrame > lastFrame)
                            reversingPreview = selectedFrame == lastFrame;
                    }

                    bool originalGUIEnabled = GUI.enabled;
                    GUI.enabled = false;
                    int totalFrames = lastFrame - firstFrame + 1;
                    EditorGUILayout.TextField("Total Frames", totalFrames.ToString());
                    EditorGUILayout.TextField("Total Duration", ((float)totalFrames * ((GifScript)target).playbackTimePerFrame).ToString("N2") + " seconds");
                    GUI.enabled = originalGUIEnabled;

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Remove Selected Frame"))
                    {
                        ((GifScript)target).RemoveFrame(selectedFrame);
                        if (selectedFrame >= totalFrames - 2)
                            selectedFrame = selectedFrame - 1;
                        if (selectedFrame < 0)
                            selectedFrame = 0;
                        EditorUtility.SetDirty(target);
                    }
                    if (GUILayout.Button("Trim to Selected Frames"))
                    {
                        bool eitherEndTrimed = false;
                        if (firstFrame > 0)
                        {
                            ((GifScript)target).RemoveFrames(0, firstFrame - 1);
                            lastFrameCount -= firstFrame;
                            lastFrame -= firstFrame;
                            if (lastFrame < 0)
                                lastFrame = 0;
                            firstFrame = 0;
                            eitherEndTrimed = true;
                        }

                        if (lastFrame < lastFrameCount - 1)
                        {
                            ((GifScript)target).RemoveFrames(lastFrame + 1, lastFrameCount - 1);
                            lastFrameCount = lastFrame + 1;
                            eitherEndTrimed = true;
                        }

                        if (eitherEndTrimed)
                            EditorUtility.SetDirty(target);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            bool showEditTransformations = CustomEditorGUILayout.BoldFoldoutForProperty(serializedObject, "showEditTransformations", "Edit Transformations");
            if (showEditTransformations)
            {
                editTransformationList.DoLayoutList();

                bool originalGUIEnabled = GUI.enabled;
                GUI.enabled &= ((GifScript)target).hasPreparedFrames;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply Transformations", GUILayout.MinHeight(40)))
                {
                    backupFrames = new List<TransformableTexture>(((GifScript)target).frames);
                    List<TransformableTexture> newFrames = new List<TransformableTexture>();
                    for (int i = 0; i < backupFrames.Count; ++i)
                    {
                        newFrames.Add(new TransformableTexture(backupFrames[i].Finalize().EditableTexture(true), ((GifScript)target).editTransformations));
                        newFrames[i].Finalize();
                    }
                    ((GifScript)target).frames = newFrames;
                    ((GifScript)target).PrepareFrames();
                    EditorUtility.SetDirty(target);
                }

                GUI.enabled &= backupFrames != null;
                if (GUILayout.Button("Restore", GUILayout.MinHeight(40)))
                {
                    ((GifScript)target).frames = backupFrames;
                    ((GifScript)target).PrepareFrames();
                    backupFrames = null;
                    EditorUtility.SetDirty(target);
                }
                GUILayout.EndHorizontal();

                GUI.enabled = originalGUIEnabled;
            }
        }

        void SaveSettings()
        {
            bool showSaveSettings = CustomEditorGUILayout.BoldFoldoutForProperty(serializedObject, "showSaveSettings", "Save Settings");
            if (showSaveSettings)
            {
                SerializedProperty fileSettingsProperty = serializedObject.FindProperty("fileSettings");
                FileSettingsEditorHelper.CaptureFileSettingsFields(((GifScript)target).fileSettings, fileSettingsProperty, true, ((GifScript)target).editorWindowMode);

                ThreadPriority threadPriority = (ThreadPriority)serializedObject.FindProperty("workerPriority").intValue;
                serializedObject.FindProperty("workerPriority").intValue = (int)((ThreadPriority)EditorGUILayout.EnumPopup(new GUIContent("Worker Priority", "Priority of the worker thread that prepares and saves the gif."), threadPriority));

                bool oldUseDifferentPlaybackSpeed = serializedObject.FindProperty("useDifferentPlaybackSpeed").boolValue;
                bool newUseDifferentPlaybackSpeed = EditorGUILayout.Toggle("Use Different Playback Speed", oldUseDifferentPlaybackSpeed);
                if (newUseDifferentPlaybackSpeed != oldUseDifferentPlaybackSpeed)
                {
                    ((GifScript)target).SetUseDifferentPlaybackSpeed(newUseDifferentPlaybackSpeed);
                    EditorUtility.SetDirty(target);
                }

                if (newUseDifferentPlaybackSpeed)
                {
                    int oldDifferentPlaybackFramesPerSecond = serializedObject.FindProperty("differentPlaybackFramesPerSecond").intValue;
                    int newDifferentPlaybackFramesPerSecond = EditorGUILayout.IntField("Playback Frames Per Second", oldDifferentPlaybackFramesPerSecond);
                    if (newDifferentPlaybackFramesPerSecond <= 0)
                        newDifferentPlaybackFramesPerSecond = 1;
                    if (newDifferentPlaybackFramesPerSecond != oldDifferentPlaybackFramesPerSecond)
                    {
                        ((GifScript)target).SetPlaybackFramesPerSecond(newDifferentPlaybackFramesPerSecond);
                        EditorUtility.SetDirty(target);
                    }
                }

                EditorGUI.BeginChangeCheck();
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("pingPongMode"));
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("reverseMode"));
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("infiniteLoop"));
                if (!serializedObject.FindProperty("infiniteLoop").boolValue)
                {
                    CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("repeatCount"));
                    if (serializedObject.FindProperty("repeatCount").intValue < 0)
                        serializedObject.FindProperty("repeatCount").intValue = 0;
                    EditorGUILayout.HelpBox("The number of times the gif should loop.", MessageType.Info);
                }
                serializedObject.FindProperty("quality").intValue = EditorGUILayout.IntSlider("Quality", serializedObject.FindProperty("quality").intValue, 1, 100);
                if (EditorGUI.EndChangeCheck())
                    serializedObject.FindProperty("tempFileSettingsCurrent").boolValue = false;

                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("resetAfterSave"));
            }
        }

        void SaveProgressBar()
        {
            if (everAnythingToSave)
            {
                float processProgress = ((GifScript)target).processProgress;
                float saveProgess = ((GifScript)target).saveProgress;
                if (processProgress < 1f || saveProgess < 1f)
                {
                    Rect position = EditorGUILayout.GetControlRect(false, 20);

                    if (processProgress < 1f)
                        EditorGUI.ProgressBar(position, processProgress, (processProgress * 100).ToString("N1") + "% Processed");
                    else if (saveProgess < 1f)
                    {
                        cachedFileSize = "";
                        string saveText = saveProgess >= 100f ? "Gif Saved" : (saveProgess * 100).ToString("N1") + "% Saved";
                        EditorGUI.ProgressBar(position, saveProgess, saveText);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(cachedFileSize))
                    {
                        string lastGifFilePath = ((GifScript)target).lastSaveFilePath;
                        if (lastGifFilePath != null && lastGifFilePath.Length > 0)
                        {
                            string[] fileNameComponents = lastGifFilePath.Split(System.IO.Path.DirectorySeparatorChar);
                            string fileName = fileNameComponents[fileNameComponents.Length - 1];
                            cachedFileSize = "Last Saved: " + fileName + "     Size: " + ((GifScript)target).SizeOfFile(lastGifFilePath);
                        }
                    }

                    if (!string.IsNullOrEmpty(cachedFileSize))
                        EditorGUILayout.LabelField(cachedFileSize);
                    else
                        EditorGUILayout.Space();
                }
            }
        }

        void SaveButtons()
        {
            bool saving = ((GifScript)target).saving;
            if (anythingToSave || saving)
            {
                bool originalGUIEnabled = GUI.enabled;

                bool processing = ((GifScript)target).recorderState == GifScript.RecorderState.PreProcessing;
                if (!processing)
                    saving &= ((GifScript)target).saveProgress < 1f;

                GUI.enabled &= !saving && !processing;
                if (GUILayout.Button("Save", GUILayout.MinHeight(40)))
                    Save();
                GUI.enabled = originalGUIEnabled;
            }
        }
        #endregion

        #region Settings Tab Settings
        void CaptureMode()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Capture Mode");
            GifScript.GifCaptureMode captureMode = (GifScript.GifCaptureMode)serializedObject.FindProperty("captureMode").intValue;
            serializedObject.FindProperty("captureMode").intValue = (int)((GifScript.GifCaptureMode)EditorGUILayout.EnumPopup(captureMode));
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
                localEditorUpdateQueue.Enqueue(((GifScript)target).CaptureModeChanged);

            captureMode = (GifScript.GifCaptureMode)serializedObject.FindProperty("captureMode").intValue;

            if (captureMode == GifScript.GifCaptureMode.SingleCamera)
                EditorGUILayout.HelpBox("Uses render texture from single camera. Traditional method.", MessageType.Info);
            else if (captureMode == GifScript.GifCaptureMode.MultiCamera)
                EditorGUILayout.HelpBox("Layers each camera's render texture. (Does not handle modified viewports like a split-screen or GUI controls. May be lower quality than other versions.)", MessageType.Info);
#if UNITY_2018_2_OR_NEWER
            else if (captureMode == GifScript.GifCaptureMode.Async)
                EditorGUILayout.HelpBox("Asynchronously grabs render texture from a single camera. Experimental. May break when multiple cameras are in the scene. Macs must have Editor in metal mode to use it.", MessageType.Info);
#endif
            else
                EditorGUILayout.HelpBox("Recommended for use with multiple cameras or if having issues with render textures (like with GUI, UI, or viewports).", MessageType.Info);

#if UNITY_EDITOR_OSX
            EditorGUILayout.HelpBox("Launching certain versions of Unity in Metal mode may cause issues. If you're having issues, please either: don't use Metal mode, use async mode with a single active camera, or use Screen Capture mode.", MessageType.Info);
#endif
        }

        void HotKeys()
        {
            bool showHotKeys = CustomEditorGUILayout.BoldFoldoutForProperty(serializedObject, "showHotKeys", "HotKeys");
            if (showHotKeys)
            {
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("takeGifKeySet"), new GUIContent("Toggle Recording Gif Key Set"), true);
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("takeOneFrameKeySet"), true);
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("saveGifKeySet"), true);
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("resetGifKeySet"), true);
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("previewCutoutKeySet"), true);

                EditorGUILayout.HelpBox("Hotkeys that overlap existing Unity Editor hotkeys can cause issues.", MessageType.Info);
                if (GUILayout.Button("Existing Unity Hotkeys"))
                    Application.OpenURL("https://docs.unity3d.com/Manual/UnityHotkeys.html");
            }
        }

        void AudioSettings()
        {
            bool showAudioSettings = CustomEditorGUILayout.BoldFoldoutForProperty(serializedObject, "showAudioSettings", "Audio Settings");
            if (showAudioSettings)
            {
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("gifRecordAudioSource"), new GUIContent("Gif Record Sound"));
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("playGifRecordAudioInEditor"), new GUIContent("Play in Editor"));
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("playGifRecordAudioInGame"), new GUIContent("Play in Game"));

                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("gifPauseAudioSource"), new GUIContent("Gif Pause Sound"));
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("playGifPauseAudioInEditor"), new GUIContent("Play in Editor"));
                CustomEditorGUILayout.PropertyField(serializedObject.FindProperty("playGifPauseAudioInGame"), new GUIContent("Play in Game"));
            }
        }
        #endregion

        #region Helpers
        protected override void GeneralUpdates()
        {
            anythingToSave = ((GifScript)target).frames.Count > 0;
            everAnythingToSave |= anythingToSave;

            GifScript.RecorderState recorderState = ((GifScript)target).recorderState;
            if (recorderState != prevRecorderState)
            {
                if (prevRecorderState == GifScript.RecorderState.Recording && recorderState == GifScript.RecorderState.Paused)
                {
                    if (((GifScript)target).prepareForPreview)
                        SelectTab(Tab.Save);
                }
                prevRecorderState = ((GifScript)target).recorderState;
            }
        }

        void Save()
        {
            cachedFileSize = "";
            FileSettingsEditorHelper.RequestSavePath(((GifScript)target).fileSettings, serializedObject.FindProperty("fileSettings"));

            // If we're saving without a preview, save all the frames
            if (!((GifScript)target).hasPreparedFrames)
                ((GifScript)target).Save("", 0, -1);
            else
                ((GifScript)target).Save("", firstFrame, lastFrame);
        }
        #endregion

        #region Memory Use Estimate
        const int KILOBYTES_PER_MEGABYTE = 1024;
        const int BYTES_PER_KILOBYTE = 1024;
        const int BYTES_PER_COLOR_WITH_ALPHA = 4;

        float estimatedPeakMemoryUse()
        {
           float memory = ((GifScript)target).maxFrameCount;
           if (((GifScript)target).pingPongMode)
              memory = memory * 2 - 1;
           if (((GifScript)target).prepareForPreview)
              memory *= ((GifScript)target).resizeResolution.width * ((GifScript)target).resizeResolution.height;
           else
              memory *= ((GifScript)target).recordResolution.width * ((GifScript)target).recordResolution.height;
           memory *= BYTES_PER_COLOR_WITH_ALPHA;
           memory /= BYTES_PER_KILOBYTE * KILOBYTES_PER_MEGABYTE;
           return memory;
        }

        string estimatedPeakMemoryUseString()
        {
           string result = "Estimated Peak Memory Use: " + estimatedPeakMemoryUse().ToString("N2") + "MB\n";
           result += ((GifScript)target).durationInSeconds + " seconds / " + ((GifScript)target).captureTimePerFrame.ToString("N2") + " seconds per frame x ";
           if (((GifScript)target).pingPongMode)
               result += "2 - 1 (ping-pong mode doubles frame count minus the unrepeated middle frame) x ";
           if (((GifScript)target).prepareForPreview)
               result += ((GifScript)target).resizeResolution.width + "(width) x " + ((GifScript)target).resizeResolution.height + "(height) x ";
           else
               result += ((GifScript)target).recordResolution.width + "(width) x " + ((GifScript)target).recordResolution.height + "(height) x ";
           result += "4 (RGBA) / 1024(to KB) / 1024(to MB)";
           return result;
        }
        #endregion
    }
}