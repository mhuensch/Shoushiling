using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_5_4_OR_NEWER
using UnityEngine.SceneManagement;
#endif

// Saves to Mobile with: https://github.com/yasirkula/UnityNativeGallery

using TRS.CaptureTool.Share;
using TRS.CaptureTool.Extras;
namespace TRS.CaptureTool
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
    [CanEditMultipleObjects]
    [DisallowMultipleComponent]
#endif
    [System.Serializable]
    public abstract class CaptureScript : MonoBehaviour
    {
        public List<GameObject> tempEnabledObjects = new List<GameObject>();
        public List<GameObject> tempDisabledObjects = new List<GameObject>();
        public float delayBeforeCapture;
        public bool stopTimeDuringCapture = true;

        public List<Camera> cameras = new List<Camera>();

        public bool overrideBackground;
        public Camera backgroundCamera;
        public Color backgroundColor;

        public bool useCutout;
        public CutoutScript cutoutScript;
        public List<RectTransform> cutoutAdjustedRectTransforms = new List<RectTransform>();
        List<Transform> originalCutoutAdjustedParents = new List<Transform>();
        List<bool> originalCutoutAdjustedActive = new List<bool>();
        List<bool> originalCutoutChildrenActive = new List<bool>();

#if ENABLE_INPUT_SYSTEM && !TRS_FORCE_LEGACY_INPUT
        public HotKeySet previewCutoutKeySet = new HotKeySet { key = UnityEngine.InputSystem.Key.C };
#elif ENABLE_LEGACY_INPUT_MANAGER
        public HotKeySet previewCutoutKeySet = new HotKeySet { keyCode = KeyCode.C };
#endif
        public TextureTransformation[] captureTransformations;
        [UnityEngine.Serialization.FormerlySerializedAs("transformations")]
        public TextureTransformation[] editTransformations;

        public bool autoSwitchRenderMode = true;
        [UnityEngine.Serialization.FormerlySerializedAs("overlayCamera")]
        public Camera uiCamera;
        public bool overridePlaneDistance;
        public float planeDistanceOverride;
#pragma warning disable 0649
        public bool useAltScreenCapture;
#pragma warning restore 0649

        public string lastSaveFilePath;

        public bool showOriginalMouse;
        public bool showInGameMouse;
        public MouseFollowScript mouseFollowScript;

        // Used to automatically update when scene switches
#pragma warning disable 0649
        [SerializeField]
        protected bool dontDestroyOnLoad = true;
#pragma warning restore 0649
        public abstract int GetMaxInstances();
        public abstract void SetMaxInstances(int newValue);
        public bool autoUpdateCameras = true;
        public bool autoUpdateCamerasByTag;
        public List<string> camerasNameOrTags = new List<string>();
        [UnityEngine.Serialization.FormerlySerializedAs("overlayCameraNameOrTag")]
        public string uiCameraNameOrTag;
        public string backgroundCameraNameOrTag;

        protected virtual bool useCanvasesAdjuster
        {
            get
            {
                return autoSwitchRenderMode && uiCamera != null;
            }
        }

#region Editor variables
#if UNITY_EDITOR
        public virtual void RefreshSubComponents()
        {
            SetShareScript(GetComponent<ShareScript>());
        }

        public System.WeakReference weakShareScript;
        public void SetShareScript(ShareScript shareScript)
        {
            if (shareScript == null)
            {
                weakShareScript = null;
                return;
            }

            shareScript.subWindowMode = true;
            shareScript.hiddenMode = shareScript.gameObject == gameObject;
            weakShareScript = new System.WeakReference(shareScript);
        }

        public bool editorWindowMode;
        Resolution lastUpdatedResolution;
        public float timeScaleOverride = 1f;

        public Queue<System.Action> editorUpdateQueue = new Queue<System.Action>();

        // These values are necessary to persist inspector state such as which dropdowns are open
#pragma warning disable 0414
        [SerializeField]
        int selectedTabIndex;
        [SerializeField]
        bool showTiming;
        [SerializeField]
        bool showEdit = true;
        [SerializeField]
        bool backgroundCameraSelected;
        [SerializeField]
        bool showCameraList;
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("showBackground")]
        bool showBackground;
        [SerializeField]
        bool showUICamera;
        [SerializeField]
        bool showCutout;
        [SerializeField]
        bool showEnabledObjects;
        [SerializeField]
        bool showDisabledObjects;
        [SerializeField]
        bool showCaptureTransformations = true;
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("showTransformations")]
        bool showEditTransformations = true;
        [SerializeField]
        bool showSaveSettings = true;
        [SerializeField]
        bool showHotKeys;
        [SerializeField]
        bool showAudioSettings;
        [SerializeField]
        bool showMouseSettings;
        [SerializeField]
        bool showDontDestroyOnLoadSettings;
        [SerializeField]
        bool showAdvancedSettings;
        [SerializeField]
        bool showSupportSettings;
        [SerializeField]
        bool uploadingEditorLogs;
        [SerializeField]
        string editorLogsUrl;
#pragma warning restore 0414
#endif
#endregion

        protected virtual void Awake()
        {
            UpdateMouse();
            SetUpDefaultValues();
#if UNITY_EDITOR
            uploadingEditorLogs = false;
            // Called here as script will likely not be active in editor and otherwise may not get updated prior to build. (Calling in constructor also won't work.
            DebugInfoScript.UpdateBuildVersion();
#endif
        }

        protected virtual void Start()
        {
            RefeshCanvasList();
        }

        protected virtual void OnEnable()
        {
            UpdateDontDestroyOnLoad();

#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded += OnSceneLoaded;
#endif
        }

        protected virtual void OnDisable()
        {
#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
        }

        protected virtual void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
#if UNITY_EDITOR
            if (Screen.width != lastUpdatedResolution.width || Screen.height != lastUpdatedResolution.height)
                ResolutionChanged();

            while (editorUpdateQueue.Count > 0)
                editorUpdateQueue.Dequeue()();

            if (editorWindowMode)
                return;
#endif
            if (FlexibleInput.AnyKeyDown() && !UIStatus.InputFieldFocused())
            {
                if (previewCutoutKeySet.MatchesInput())
                    cutoutScript.preview = !cutoutScript.preview;
            }
#endif
        }

#region Helpers
#region Set Up
        public void UpdateMouse()
        {
            if (Application.isPlaying)
                Cursor.visible = showOriginalMouse;
            else
                Cursor.visible = true;
            if (mouseFollowScript != null)
                mouseFollowScript.gameObject.SetActive(showInGameMouse);
        }

        protected virtual void SetUpDefaultValues()
        {
            if (cameras == null || cameras.Count == 0 || backgroundCamera == null || uiCamera == null)
            {
                Camera[] allCameras = Camera.allCameras.OrderBy(camera => camera.depth).ToArray(); // Stable sort as opposed to tradional Sort()
                if (cameras == null || cameras.Count == 0)
                    cameras = new List<Camera>(allCameras);
                // Bit of a hack to handle the case where cameras is initialized with UI Camera, but prefab still needs initialize with scene cameras
                else if (cameras[0] == null)
                    AddAllActiveCameras();

                if (backgroundCamera == null)
                {
#if UNITY_EDITOR
                    backgroundCameraSelected = false;
#endif
                    backgroundCamera = allCameras[0];
                }
                if (uiCamera == null)
                    uiCamera = allCameras[allCameras.Length - 1];
            }

            if (cutoutScript == null)
                cutoutScript = GetComponentInChildren<CutoutScript>();

            AnyCameraChanged();
        }

        public void AddAllActiveCameras()
        {
            foreach (Camera camera in Camera.allCameras)
            {
                if (!cameras.Contains(camera))
                    cameras.Add(camera);
            }
        }
#endregion

#region Prepare/Restore Capture
        Color originalBackgroundColor = Color.white;
        CameraClearFlags originalClearFlags = CameraClearFlags.Skybox;
        List<bool> originalTempEnabledObjectsState;
        List<bool> originalTempDisabledObjectsState;
        float originalTimeScale;

        protected virtual void PrepareToCapture()
        {
            if (overrideBackground)
            {
                if (backgroundCamera != null)
                {
                    originalBackgroundColor = backgroundCamera.backgroundColor;
                    originalClearFlags = backgroundCamera.clearFlags;

                    backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
                    backgroundCamera.backgroundColor = backgroundColor;
                }
                else
                    Debug.LogError("Camera containing background must be set for background override to work.");
            }

            if (useCutout && cutoutScript != null)
            {
                originalCutoutChildrenActive.Clear();
                foreach (Transform childTransform in cutoutScript.transform)
                    originalCutoutChildrenActive.Add(childTransform.gameObject.activeSelf);

                originalCutoutAdjustedActive.Clear();
                originalCutoutAdjustedParents.Clear();
                foreach (RectTransform cutoutRectTransform in cutoutAdjustedRectTransforms)
                {
                    if (cutoutRectTransform != null)
                    {
                        originalCutoutAdjustedParents.Add(cutoutRectTransform.parent);
                        originalCutoutAdjustedActive.Add(cutoutRectTransform.gameObject.activeSelf);
                    }
                    else
                    {
                        originalCutoutAdjustedParents.Add(null);
                        originalCutoutAdjustedActive.Add(false);
                    }
                }
            }

            tempEnabledObjects.RemoveAll(tempEnabledObject => tempEnabledObject == null);
            originalTempEnabledObjectsState = new List<bool>(tempEnabledObjects.Count);
            foreach (GameObject tempEnabledObject in tempEnabledObjects)
            {
                originalTempEnabledObjectsState.Add(tempEnabledObject.activeSelf);
                tempEnabledObject.SetActive(true);
            }

            tempDisabledObjects.RemoveAll(tempDisabledObject => tempDisabledObject == null);
            originalTempDisabledObjectsState = new List<bool>(tempDisabledObjects.Count);
            foreach (GameObject tempDisabledObject in tempDisabledObjects)
            {
                originalTempDisabledObjectsState.Add(tempDisabledObject.activeSelf);
                tempDisabledObject.SetActive(false);
            }

            if (useCanvasesAdjuster)
            {
                if (uiCamera != null)
                    CanvasesAdjuster.ForceCameraRenderMode(uiCamera, overridePlaneDistance, planeDistanceOverride);
                else if (CanvasesAdjuster.AnyOverlayCameras())
                    Debug.LogError("Overlay canvases will not display properly without overlay camera set: Fix in Advanced Settings");
            }

            if (cutoutScript != null)
                cutoutScript.Hide();
            else if (useCutout)
                Debug.LogError("Cutout Error: Cannot use cutout without CutoutScript set.");

            if (useCutout && cutoutScript != null)
            {
                foreach (Transform childTransform in cutoutScript.transform)
                    childTransform.gameObject.SetActive(true);

                foreach (RectTransform cutoutRectTransform in cutoutAdjustedRectTransforms)
                {
                    if (cutoutRectTransform != null)
                    {
                        cutoutRectTransform.gameObject.SetActive(true);
                        cutoutRectTransform.SetParent(cutoutScript.transform, false);
                    }
                }
            }

            if (stopTimeDuringCapture)
            {
                originalTimeScale = Time.timeScale;
                Time.timeScale = 0;
            }
        }

        protected virtual void RestoreAfterCapture()
        {
            if (overrideBackground && backgroundCamera != null)
            {
                backgroundCamera.clearFlags = originalClearFlags;
                backgroundCamera.backgroundColor = originalBackgroundColor;
            }

            for (int i = 0; i < originalTempEnabledObjectsState.Count; ++i)
                tempEnabledObjects[i].SetActive(originalTempEnabledObjectsState[i]);

            for (int i = 0; i < originalTempDisabledObjectsState.Count; ++i)
                tempDisabledObjects[i].SetActive(originalTempDisabledObjectsState[i]);

            if (useCanvasesAdjuster)
                CanvasesAdjuster.RestoreOriginalRenderModes();

            if (cutoutScript != null)
                cutoutScript.Show();

            if (useCutout && cutoutScript != null)
            {
                int i = 0;
                for (i = 0; i < cutoutAdjustedRectTransforms.Count; ++i)
                {
                    RectTransform cutoutRectTransform = cutoutAdjustedRectTransforms[i];
                    if (cutoutRectTransform != null)
                    {
                        bool wasActive = originalCutoutAdjustedActive[i];
                        Transform originalParent = originalCutoutAdjustedParents[i];
                        cutoutRectTransform.SetParent(originalParent, false);
                        cutoutRectTransform.gameObject.SetActive(wasActive);
                    }
                }

                i = 0;
                foreach (Transform childTransform in cutoutScript.transform)
                    childTransform.gameObject.SetActive(originalCutoutChildrenActive[i++]);

                originalCutoutChildrenActive.Clear();
                originalCutoutAdjustedActive.Clear();
                originalCutoutAdjustedParents.Clear();
            }

            if (stopTimeDuringCapture)
                Time.timeScale = originalTimeScale;
        }
#endregion

#region Texture Helpers
        [System.Obsolete("Please use CaptureRawFrame.ScreenCapture instead.")]
        protected virtual Texture2D FlexibleScreenCaptureWithCutout(Resolution resolution, int scale, TextureFormat textureFormat = TextureFormat.ARGB32, bool apply = true)
        {
#if UNITY_2017_3_OR_NEWER
            return ScreenCaptureWithCutout(scale, textureFormat, apply);
#else
            return LegacyScreenCapture(resolution, scale, textureFormat, apply);
#endif
        }

        [System.Obsolete("Please use CaptureRawFrame.ScreenCapture instead.")]
        protected virtual Texture2D LegacyScreenCapture(Resolution resolution, int scale, TextureFormat textureFormat = TextureFormat.ARGB32, bool apply = true)
        {
            Rect textureRect = new Rect(0, 0, resolution.width * scale, resolution.height * scale);
            if (useCutout)
                textureRect = cutoutScript.rect;

            Texture2D screenCapture = new Texture2D((int)textureRect.width, (int)textureRect.height, textureFormat, false);
            screenCapture.ReadPixels(textureRect, 0, 0, false);
            if (apply)
                screenCapture.Apply(false);

            return screenCapture;
        }

#if UNITY_2017_3_OR_NEWER
        [System.Obsolete("Please use CaptureRawFrame.ScreenCapture instead.")]
        protected virtual Texture2D ScreenCaptureWithCutout(int scale, TextureFormat textureFormat = TextureFormat.ARGB32, bool apply = true)
        {
            Texture2D screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture(scale);
            bool correctTextureFormat = screenshotTexture.format == textureFormat;
            if (useCutout)
                screenshotTexture = screenshotTexture.Cutout(cutoutScript.rect, apply && correctTextureFormat);
            if(!correctTextureFormat)
            {
                Texture2D textureInTextureFormat = new Texture2D(screenshotTexture.width, screenshotTexture.height, textureFormat, false);
                textureInTextureFormat.SetPixels(screenshotTexture.GetPixels());
                if(apply) textureInTextureFormat.Apply();
                MonoBehaviourExtended.FlexibleDestroy(screenshotTexture);
                screenshotTexture = textureInTextureFormat;

            }
            return screenshotTexture;
        }
#endif

        [System.Obsolete("Please use CaptureRawFrame.CameraRenderTexture instead.")]
        protected virtual Texture2D CombinedRenderTextures(Camera[] camerasToUse, Resolution resolution, int scale, TextureFormat textureFormat = TextureFormat.ARGB32, int depth = 24, bool apply = true)
        {
            int width = resolution.width * scale;
            int height = resolution.height * scale;
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, depth);

            int firstActiveCameraIndex;
            for (firstActiveCameraIndex = 0; firstActiveCameraIndex < camerasToUse.Length; ++firstActiveCameraIndex)
            {
                if ((camerasToUse[firstActiveCameraIndex].enabled && camerasToUse[firstActiveCameraIndex].gameObject.activeInHierarchy) || camerasToUse[firstActiveCameraIndex].cameraType == CameraType.SceneView)
                    break;
            }
            bool activeCameraFound = firstActiveCameraIndex < camerasToUse.Length;

            // Documentation seems to say that setting active texture isn't necessary when using Camera.targetTexture. However, I have found it is necessary in practice
            RenderTexture originalActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            Rect textureRect = new Rect(0, 0, width, height);
            if (useCutout)
                textureRect = cutoutScript.rect;

            if (!activeCameraFound)
                return new Texture2D((int)textureRect.width, (int)textureRect.height, textureFormat, false);

            Texture2D finalTexture = new Texture2D((int)textureRect.width, (int)textureRect.height, textureFormat, false);
            if (!activeCameraFound)
                return finalTexture;

            finalTexture.CaptureCameraRenderTexture(camerasToUse[firstActiveCameraIndex], renderTexture, textureRect, false);
            if (camerasToUse.Length > 1)
            {
                Texture2D cameraTexture = new Texture2D((int)textureRect.width, (int)textureRect.height, textureFormat, false);

                for (int i = 1; i < camerasToUse.Length; ++i)
                {
                    Camera cameraToUse = camerasToUse[i];
                    if (!cameraToUse.enabled || !cameraToUse.gameObject.activeInHierarchy)
                        continue;
                    if (useCutout)
                    {
                        Rect newTextureRect = cutoutScript.rect;
                        if (newTextureRect != textureRect)
                        {
                            textureRect = newTextureRect;
                            MonoBehaviourExtended.FlexibleDestroy(cameraTexture);
                            cameraTexture = new Texture2D((int)textureRect.width, (int)textureRect.height, textureFormat, false);
                        }
                    }

                    cameraTexture.CaptureCameraRenderTexture(cameraToUse, renderTexture, textureRect, false);
                    finalTexture = finalTexture.AlphaBlend(cameraTexture, false);
                }
            }

            if (apply)
                finalTexture.Apply(false);

            RenderTexture.active = originalActiveRenderTexture;
            RenderTexture.ReleaseTemporary(renderTexture);
            return finalTexture;
        }

#if !UNITY_EDITOR && UNITY_WEBGL
        public virtual void Process(byte[] bytes, CaptureFileSettings fileSettings)
        {
            if (!fileSettings.openInNewTab && !fileSettings.download)
                return;
        
            string encodedText = System.Convert.ToBase64String(bytes);
            processImage(encodedText, fileSettings.fullWebFileName, fileSettings.encoding, fileSettings.openInNewTab, fileSettings.download);
        }

        [System.Runtime.InteropServices.DllImport("__Internal")]
        static extern void processImage(string url, string fileName, string type, bool display, bool download);
#endif
#endregion

#region Scene Change Updates
#if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            NewSceneLoaded();
        }
#else
        void OnLevelWasLoaded(int level)
        {
            NewSceneLoaded();
        }
#endif

        protected virtual void NewSceneLoaded()
        {
            if (camerasNameOrTags.Count == 0)
                AnyCameraChanged();

            if (dontDestroyOnLoad)
            {
                CanvasesAdjuster.Setup();

                if (autoUpdateCameras)
                {
                    for (int i = 0; i < cameras.Count; ++i)
                    {
                        GameObject cameraObject = autoUpdateCamerasByTag ? GameObject.FindWithTag(camerasNameOrTags[i]) : GameObject.Find(camerasNameOrTags[i]);
                        if (cameraObject != null)
                            cameras[i] = cameraObject.GetComponent<Camera>();
                    }

                    GameObject backgroundCameraObject = null;
                    GameObject uiCameraObject = null;
                    if (backgroundCameraNameOrTag != null && backgroundCameraNameOrTag.Length > 0)
                        backgroundCameraObject = autoUpdateCamerasByTag ? GameObject.FindWithTag(backgroundCameraNameOrTag) : GameObject.Find(backgroundCameraNameOrTag);
                    if (uiCameraNameOrTag != null && uiCameraNameOrTag.Length > 0)
                        uiCameraObject = autoUpdateCamerasByTag ? GameObject.FindWithTag(uiCameraNameOrTag) : GameObject.Find(uiCameraNameOrTag);

                    if (backgroundCameraObject != null)
                        backgroundCamera = backgroundCameraObject.GetComponent<Camera>();
                    if (uiCameraObject != null)
                        uiCamera = uiCameraObject.GetComponent<Camera>();
                }
            }
        }

        public void RefeshCanvasList()
        {
            CanvasesAdjuster.Setup();
        }

        public virtual void AnyCameraChanged()
        {
            camerasNameOrTags = new List<string>();
            foreach (Camera camera in cameras)
            {
                if (camera == null)
                    camerasNameOrTags.Add("");
                else if (autoUpdateCamerasByTag)
                    camerasNameOrTags.Add(camera.tag);
                else
                    camerasNameOrTags.Add(camera.name);
            }

            if (autoUpdateCamerasByTag)
            {
                backgroundCameraNameOrTag = backgroundCamera != null ? backgroundCamera.tag : "";
                uiCameraNameOrTag = uiCamera != null ? uiCamera.tag : "";
            }
            else
            {
                backgroundCameraNameOrTag = backgroundCamera != null ? backgroundCamera.name : "";
                uiCameraNameOrTag = uiCamera != null ? uiCamera.name : "";
            }
        }

        public virtual void UpdateDontDestroyOnLoad()
        {
            if (!CanvasesAdjuster.automaticallyTrackDontDestroyOnLoadGameObjects)
            {
                if (dontDestroyOnLoad)
                    CanvasesAdjuster.AddDontDestroyOnLoadGameObject(gameObject);
                else
                    CanvasesAdjuster.RemoveDontDestroyOnLoadGameObject(gameObject);
            }
        }
#endregion

#region Clean Up Inputs
        protected virtual void CleanUpCameraList()
        {
            cameras.RemoveAll(camera => camera == null);
            cameras = cameras.OrderBy(camera => camera.depth).ToList(); // Stable sort as opposed to tradional Sort()
            AnyCameraChanged();
        }
#endregion

#region Editor Variable Change Updates
#if UNITY_EDITOR
        protected virtual void ResolutionChanged()
        {
            lastUpdatedResolution = new Resolution { width = Screen.width, height = Screen.height };
        }

        public virtual void CutoutValueChanged()
        {

        }

        public virtual void BackgroundCameraChanged()
        {
            backgroundCameraSelected = true;
            AnyCameraChanged();
        }

        public void UICameraChanged()
        {
            AnyCameraChanged();
        }

        public void TimeScaleOverrideChanged()
        {
            Time.timeScale = timeScaleOverride;
        }
#endif
#endregion
#endregion
    }
}