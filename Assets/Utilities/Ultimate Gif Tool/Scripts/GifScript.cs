// Parts of this code was inspired by the Recorder in the included Moments project by Thomas Hourdel

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UniGif;
using Moments;
using Moments.Encoder;
using ThreadPriority = System.Threading.ThreadPriority;

#if UNITY_EDITOR
using UnityEditor;
#endif

using TRS.CaptureTool.Extras;
namespace TRS.CaptureTool
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
    [CanEditMultipleObjects]
    [DisallowMultipleComponent]
#endif
    public sealed class GifScript : CaptureScript
    {
        public const string version = "3.27";

        /** Note: There is no recorder state for saving as recording and saving
         * may happen simultaneously. Instead subscribe to GifToSave, 
         * GifSaveProgress, or GifSaved for save state changes. */
        public static System.Action<GifScript, RecorderState> RecorderStateChanged;

        public static System.Action<GifScript, int, int> WillTakeGif;
        public static System.Action<GifScript, Texture2D[], int> GifTaken;
        public static System.Action<GifScript, Texture2D[], int> GifToSave;
        public static System.Action<GifScript, float> GifSaveProgress;
        public static System.Action<GifScript, string> GifSaved;

        public static System.Action<GifScript> WillResetGif;
        public static System.Action<GifScript> GifReset;

        public static System.Action<int, float> WorkerGifSaveProgress;
        public static System.Action<int, string> WorkerSavedGif;

        [System.Serializable]
        public enum GifCaptureMode
        {
            ScreenCapture,
            SingleCamera,
            MultiCamera,
#if UNITY_2018_2_OR_NEWER
            Async
#endif
        };

        [System.Serializable]
        public enum GifCaptureRate
        {
            FPS,
            EveryXFrames,
            NaturalTiming,
            VRHybrid,
        };

        public enum RecorderState
        {
            None,
            Recording,
            Paused,
            PreProcessing,
        };

        public GifCaptureMode captureMode = GifCaptureMode.ScreenCapture;
        public GifCaptureRate captureRate = GifCaptureRate.FPS;
        public RecorderState recorderState
        {
            get { return _recorderState; }
            private set { if (_recorderState != value) { _recorderState = value; if (RecorderStateChanged != null) RecorderStateChanged(this, _recorderState); } }
        }
        private RecorderState _recorderState;

        public List<TransformableTexture> frames = new List<TransformableTexture>();
        public int maxFrameCount { get { return Mathf.FloorToInt((float)durationInSeconds / captureTimePerFrame); } }

        public Texture2D[] preparedFrames = null;
        public bool hasPreparedFrames { get { return preparedFrames != null && preparedFrames.Length > 0; } }

        public bool stopAutomatically = true;
        public bool oneFrameOnly;

        public bool adjustTimeForCapture = true;
        public int altScreenCaptureModeScale = 1;
        public float delayStartTime = -1f;

        #region Capture Rate variables
        [SerializeField]
        int captureFramesPerSecond = 15;
        [SerializeField]
        public float captureTimePerFrame { get; private set; }
        public int captureEveryXFrame;
        public int durationInSeconds = 5;
        #endregion

        #region Playback Time variables
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("differentPlaybackSpeed")]
        bool useDifferentPlaybackSpeed;
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("playbackFramesPerSecond")]
        int differentPlaybackFramesPerSecond = 15;
        public int playbackFramesPerSecond { get { return useDifferentPlaybackSpeed ? differentPlaybackFramesPerSecond : captureFramesPerSecond; } }
        [SerializeField]
        public float playbackTimePerFrame { get; private set; }
        #endregion

        #region Resize variables
        public bool resizeBeforeRecording = true;
        public bool useScaleToResizeBeforeRecording;
        public float recordWidthScale = 1f;
        public float recordHeightScale = 1f;
        public int recordWidth = 506;
        public int recordHeight = 253;
        public Resolution recordResolution;

        public bool resizeAfterRecording;
        public bool useScaleToResizeAfterRecording;
        public float resizeScale = 1f;
        public int resizeWidth = 506;
        public int resizeHeight
        {
            get
            {
                Resolution resolution;
                if (resizeBeforeRecording)
                    resolution = recordResolution;
                else
                    resolution = ScreenExtensions.CurrentResolution();

                if (useCutout)
                {
                    Vector2 cutoutSize = cutoutScript.rect.size;
                    return Mathf.FloorToInt((float)resizeWidth / cutoutSize.x * (float)cutoutSize.y);
                }

                return Mathf.FloorToInt((float)resizeWidth / (float)resolution.width * (float)resolution.height);
            }
        }
        public Resolution resizeResolution;
        #endregion

        #region Prepare variables
        public bool prepareForPreviewDuringFrameCapture;
        public bool prepareForPreviewDuringFrameCaptureInEditor = true;
        public bool prepareForPreview { get { return prepareForPreviewDuringFrameCapture || (prepareForPreviewDuringFrameCaptureInEditor && Application.isEditor); } }
        #endregion

        #region Format & Save variables
        public ThreadPriority workerPriority = ThreadPriority.BelowNormal;
        public bool pingPongMode;
        public bool reverseMode;
        public bool infiniteLoop = true;
        public int repeatCount;
        [Range(1, 100)]
        public int quality = 75;
        public bool resetAfterSave;
        public bool useTransparency;
        public Color32 transparencyColor = new Color32(255, 0, 230, 255);
        public GifFileSettings fileSettings = new GifFileSettings();

        public float processProgress { get; private set; }
        public float saveProgress { get; private set; }
        public bool saving { get { return saveProgress != 1.0; } }
        #endregion

        #region Hot Keys
#if ENABLE_INPUT_SYSTEM && !TRS_FORCE_LEGACY_INPUT
        public HotKeySet takeGifKeySet = new HotKeySet { key = UnityEngine.InputSystem.Key.G };
        public HotKeySet takeOneFrameKeySet = new HotKeySet { key = UnityEngine.InputSystem.Key.Y };
        public HotKeySet saveGifKeySet = new HotKeySet { key = UnityEngine.InputSystem.Key.X };
        public HotKeySet resetGifKeySet = new HotKeySet { key = UnityEngine.InputSystem.Key.T };
#elif ENABLE_LEGACY_INPUT_MANAGER
        public HotKeySet takeGifKeySet = new HotKeySet { keyCode = KeyCode.G };
        public HotKeySet takeOneFrameKeySet = new HotKeySet { keyCode = KeyCode.Y };
        public HotKeySet saveGifKeySet = new HotKeySet { keyCode = KeyCode.X };
        public HotKeySet resetGifKeySet = new HotKeySet { keyCode = KeyCode.T };
#endif
        #endregion

        #region Record/Pause Audio
        public bool playGifRecordAudioInGame = true;
        public bool playGifRecordAudioInEditor = true;
        public bool playGifRecordAudio { get { return ((playGifRecordAudioInEditor && Application.isEditor) || (playGifRecordAudioInGame && !Application.isEditor)); } }
        public AudioSource gifRecordAudioSource;

        public bool playGifPauseAudioInGame = true;
        public bool playGifPauseAudioInEditor = true;
        public bool playGifPauseAudio { get { return ((playGifPauseAudioInEditor && Application.isEditor) || (playGifPauseAudioInGame && !Application.isEditor)); } }
        public AudioSource gifPauseAudioSource;
#endregion

#region Private variables
        int originalCaptureFrameRate;
        bool originalResolutionLockedIn;
        Resolution originalResolution;
        bool originalRunInBackground;

        float timeSinceLastFrame;
        int lastFrameRecorded;
        int workerId;

        IEnumerator captureCoroutine;
#pragma warning disable 0649
        AsyncCaptureScript asyncCaptureScript;
#pragma warning restore 0649
        OnRenderImageScript onRenderImageScript;
        protected override bool useCanvasesAdjuster
        {
            get
            {
                return base.useCanvasesAdjuster && captureMode != GifCaptureMode.ScreenCapture;
            }
        }
#endregion

#region Editor variables
#if UNITY_EDITOR
        int originalSelectedSizeIndex;
#pragma warning disable 0414
        [SerializeField]
        bool showSizeSettings;
        [SerializeField]
        bool showRecordSettings = true;
#pragma warning restore 0414
#endif
#endregion

#region Unity events

        protected override void Awake()
        {
            base.Awake();

            SetCaptureFramesPerSecond(captureFramesPerSecond);
            SetPlaybackFramesPerSecond(playbackFramesPerSecond);
            stopTimeDuringCapture = false;
            processProgress = 1f;
            saveProgress = 1f;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            UnityToolbag.Dispatcher.Prepare(gameObject);
            fileSettings.SetUp(gameObject.GetInstanceID());

            WorkerGifSaveProgress += OnWorkerGifSaveProgress;
            WorkerSavedGif += OnWorkerSavedGif;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            WorkerGifSaveProgress -= OnWorkerGifSaveProgress;
            WorkerSavedGif -= OnWorkerSavedGif;
        }

        void OnDestroy()
        {
            Reset();
        }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        protected override void Update()
        {
            base.Update();

#if UNITY_EDITOR
                if (editorWindowMode)
                return;
#endif
            if (FlexibleInput.AnyKeyDown() && !UIStatus.InputFieldFocused())
            {
                if (takeGifKeySet.MatchesInput())
                    ToggleRecording();
                if (takeOneFrameKeySet.MatchesInput())
                    RecordOneFrame();
                if(saveGifKeySet.MatchesInput())
                    Save();
                if (resetGifKeySet.MatchesInput())
                {
                    Pause();
                    Reset();
                }
            }
        }
#endif
#endregion

#region Public Methods
        public void SetUseDifferentPlaybackSpeed(bool differentPlaybackSpeed)
        {
            useDifferentPlaybackSpeed = differentPlaybackSpeed;
            if (!useDifferentPlaybackSpeed)
                SetPlaybackFramesPerSecond(captureFramesPerSecond);
        }

        public void SetPlaybackFramesPerSecond(int fps)
        {
            useDifferentPlaybackSpeed |= fps != captureFramesPerSecond;
            differentPlaybackFramesPerSecond = fps;
            playbackTimePerFrame = Mathf.Round(100f / differentPlaybackFramesPerSecond) / 100f;
        }

        public void SetCaptureFramesPerSecond(int fps)
        {
            captureFramesPerSecond = fps;
            captureTimePerFrame = Mathf.Round(100f / captureFramesPerSecond) / 100f;
            if (!useDifferentPlaybackSpeed)
                SetPlaybackFramesPerSecond(captureFramesPerSecond);
        }

        /** Note: This method can only be used on gifs within the Application.streamingAssetsPath in builds. */
        public void LoadGifAtFilePath(string gifFilePath)
        {
            byte[] gifBytes = null;
            try
            {
                gifBytes = System.IO.File.ReadAllBytes(gifFilePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Exception loading gif: " + e);
            }

            StartCoroutine(UniGif.GetTextureListCoroutine(gifBytes, (gifTextureList, loopCount, width, height) =>
            {
                if (gifTextureList == null)
                {
                    Debug.LogError("Failed to load gif at: " + gifFilePath);
                    return;
                }

                List<TransformableTexture> newFrames = new List<TransformableTexture>();
                foreach (GifTexture gifTexture in gifTextureList)
                {
                    TransformableTexture newFrame = new TransformableTexture(gifTexture.m_texture2d, null);
                    newFrames.Add(newFrame);
                }

                frames = newFrames;
                preparedFrames = null;
            }));
        }

        public void ToggleRecording()
        {
            if (recorderState == RecorderState.PreProcessing)
            {
                Debug.LogWarning("Attempting to toggle recording during the pre-processing step.");
                return;
            }

            if (recorderState == RecorderState.None || recorderState == RecorderState.Paused)
                Record();
            else
                Pause();
        }

        public void Record()
        {
            if (AlreadyRecording())
                return;

            if (playGifRecordAudio && gifRecordAudioSource != null && gifRecordAudioSource.clip != null)
                gifRecordAudioSource.Play();

            StartCoroutine(PrepareAndRecord());
        }

        public void RecordOneFrame()
        {
            if (recorderState == RecorderState.PreProcessing)
            {
                Debug.LogWarning("Attempting to record during the pre-processing step.");
                return;
            }

            oneFrameOnly = true;
            if (recorderState != RecorderState.Recording)
                Record();
        }

        public bool AlreadyRecording()
        {
            if (recorderState == RecorderState.PreProcessing)
            {
                Debug.LogWarning("Attempting to record during the pre-processing step.");
                return true;
            }

            if (recorderState == RecorderState.Recording)
            {
                Debug.LogWarning("Attempting to record while already recording.");
                return true;
            }

            return false;
        }

        IEnumerator PrepareAndRecord()
        {
            delayStartTime = Time.realtimeSinceStartup;
            yield return new WaitForSecondsRealtime(delayBeforeCapture);
            delayStartTime = -1;

            yield return StartCoroutine(SwitchToRecordResolution());

            // Yield to be sure ScreenCapture occurs at right time, that any ResolutionUpdated events have taken place and generally that the resizing is finished
            yield return new WaitForEndOfFrame();

            if (recorderState == RecorderState.None)
            {
                Resolution resolution = ScreenExtensions.CurrentResolution();
                if (WillTakeGif != null)
                    WillTakeGif(this, resolution.width, resolution.height);
            }

            CleanUpCameraList();
            PrepareToCapture();
            timeSinceLastFrame = 0;
            lastFrameRecorded = Time.frameCount;
            recorderState = RecorderState.Recording;
            if (captureMode == GifCaptureMode.MultiCamera || captureMode == GifCaptureMode.ScreenCapture)
            {
                if (captureCoroutine != null)
                    StopCoroutine(captureCoroutine);
                captureCoroutine = CaptureFrames();
                StartCoroutine(captureCoroutine);
            }
#if UNITY_2018_2_OR_NEWER
            else if (captureMode == GifCaptureMode.Async)
                asyncCaptureScript.AsyncCaptureListeners += NewAsyncCapture;
#endif
        }

        public void Pause()
        {
            if (recorderState == RecorderState.PreProcessing)
            {
                Debug.LogWarning("Attempting to pause recording during the pre-processing step. The recorder is automatically paused when pre-processing.");
                return;
            }

            if (recorderState == RecorderState.None || recorderState == RecorderState.Paused)
                return;

            if (playGifPauseAudio && gifPauseAudioSource != null && gifPauseAudioSource.clip != null)
                gifPauseAudioSource.Play();

            recorderState = RecorderState.Paused;
            if (captureCoroutine != null)
            {
                StopCoroutine(captureCoroutine);
                captureCoroutine = null;
            }
#if UNITY_2018_2_OR_NEWER
            else if (captureMode == GifCaptureMode.Async)
                asyncCaptureScript.AsyncCaptureListeners -= NewAsyncCapture;
#endif

            originalResolutionLockedIn = false;
            if (resizeBeforeRecording)
            {
#if UNITY_EDITOR
                if (GameView.GetSelectedSizeIndex() != originalSelectedSizeIndex)
                    GameView.SetSelecedSizeIndex(originalSelectedSizeIndex);
#else
                Resolution currentResolution = ScreenExtensions.CurrentResolution();
                if (!currentResolution.IsSameSizeAs(originalResolution))
                    ScreenExtensions.UpdateResolution(originalResolution);
#endif
            }
            RestoreAfterCapture();

            if (prepareForPreview)
                PrepareFrames();
        }

        [System.Obsolete("PrepareForPreview is deprecated. Please use PrepareFrames")]
        public void PrepareForPreview()
        {
            if (recorderState == RecorderState.PreProcessing)
            {
                Debug.LogWarning("Attempting to preview during the pre-processing step.");
                return;
            }

            PrepareFrames();
        }

        public void RemoveFrames(int firstFrame, int lastFrame)
        {
            RemoveFramesRange(firstFrame, (lastFrame - firstFrame + 1));
        }

        public void RemoveFramesRange(int firstFrame, int count)
        {
            frames.RemoveRange(firstFrame, count);
            preparedFrames = null;
        }

        public void RemoveFrame(int frameIndex)
        {
            frames.RemoveAt(frameIndex);
            preparedFrames = null;
        }

        public void RequestFrames(int firstFrame = 0, int lastFrame = -1)
        {
            if (recorderState == RecorderState.PreProcessing)
            {
                Debug.LogWarning("Attempting to request frames during the pre-processing step. Listen to GifTaken event for frames.");
                return;
            }

            if (recorderState == RecorderState.Recording)
            {
                Debug.LogWarning("Attempting to request frames while recording. Frames currently being recorded may not be included in results.");
            }

            if (hasPreparedFrames) return;

            StartCoroutine(PreProcess(false, "", firstFrame, lastFrame));
        }

        public Texture2D[] GetFrames(int firstFrame = 0, int lastFrame = -1)
        {
            if (recorderState == RecorderState.PreProcessing)
            {
                Debug.LogWarning("Attempting to get frames during the pre-processing step. Listen to GifTaken event for frames.");
                return null;
            }

            if (recorderState == RecorderState.Recording)
            {
                Debug.LogWarning("Attempting to request frames while recording. Frames currently being recorded may not be included in results.");
            }

            if(hasPreparedFrames)
            {
                int[] frameRange = ValidatedFrameRange(firstFrame, lastFrame);
                firstFrame = frameRange[0];
                lastFrame = frameRange[1];
                int count = lastFrame - firstFrame + 1;
                return new List<Texture2D>(preparedFrames).GetRange(firstFrame, count).ToArray();
            }

            return SynchronousPreProcess(false, "", firstFrame, lastFrame);
        }

        public Texture2D[] PrepareFrames()
        {
            List<Texture2D> finalFrames = new List<Texture2D>();
            foreach (TransformableTexture frame in frames)
            {
                if (!frame.finalized)
                    frame.Finalize();

                finalFrames.Add(frame.finalTexture);
            }

            preparedFrames = finalFrames.ToArray();
            return preparedFrames;
        }

        // Necessary to call save from button (only 0-1 arguments available in OnClick)
        public void Save()
        {
            Save("", 0, -1);
        }

#pragma warning disable RECS0137 // Method with optional parameter is hidden by overload
        public void Save(string fullFilePath = "", int firstFrame = 0, int lastFrame = -1)
#pragma warning restore RECS0137 // Method with optional parameter is hidden by overload
        {
            if (recorderState == RecorderState.PreProcessing)
            {
                Debug.LogWarning("Attempting to save during the pre-processing step.");
                return;
            }

            if (frames.Count <= 0)
            {
                Debug.LogWarning("Nothing to save.");
                return;
            }

            if (recorderState == RecorderState.Recording)
                Pause();
            StartCoroutine(PreProcess(true, fullFilePath, firstFrame, lastFrame, false));
        }

        public void Reset()
        {
            if (WillResetGif != null)
                WillResetGif(this);

            recorderState = RecorderState.None;
            FlushMemory();

            if (GifReset != null)
                GifReset(this);
        }
#endregion

        void NewAsyncCapture(Color32[] buffer)
        {
            if (!ShouldAddFrame())
                return;

            Resolution currentResolution = ScreenExtensions.CurrentResolution();
            Rect cutoutRect = (useCutout && cutoutScript != null) ? cutoutScript.rect : new Rect(0, 0, currentResolution.width, currentResolution.height);
            Resolution resizeResolutionToUse = resizeAfterRecording ? resizeResolution : ResolutionExtensions.EmptyResolution();
            RawFrameData newRawData = new RawFrameData(buffer, currentResolution, cutoutRect, resizeResolutionToUse, !useTransparency);
            TransformableTexture newFrame = new TransformableTexture(newRawData, captureTransformations);
            if (prepareForPreview)
                newFrame.Finalize();
            frames.Add(newFrame);
            preparedFrames = null;
        }

        void NewRenderTexture(RenderTexture source)
        {
            if (!ShouldAddFrame())
                return;

            Resolution currentResolution = ScreenExtensions.CurrentResolution();
            Rect cutoutRect = (useCutout && cutoutScript != null) ? cutoutScript.rect : new Rect(0, 0, currentResolution.width, currentResolution.height);
            Resolution resizeResolutionToUse = resizeAfterRecording ? resizeResolution : ResolutionExtensions.EmptyResolution();
            RawFrameData newRawData = new RawFrameData(source, cutoutRect, resizeResolutionToUse, !useTransparency);
            newRawData.destroyOriginal = false;
            TransformableTexture newFrame = new TransformableTexture(newRawData, captureTransformations);
            if (prepareForPreview)
                newFrame.Finalize();
            frames.Add(newFrame);
            preparedFrames = null;
        }

        IEnumerator CaptureFrames()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (!ShouldAddFrame())
                    continue;

                Resolution currentResolution = ScreenExtensions.CurrentResolution();
                Rect cutoutRect = (useCutout && cutoutScript != null) ? cutoutScript.rect : new Rect(0, 0, currentResolution.width, currentResolution.height);
                Resolution resizeResolutionToUse = resizeAfterRecording ? resizeResolution : ResolutionExtensions.EmptyResolution();

                RawFrameData newRawData = null;
                if (captureMode == GifCaptureMode.MultiCamera && cameras.Count > 0)
                    newRawData = CaptureRawFrame.CamerasRenderTexture(cameras.ToArray(), cutoutRect, resizeResolution, !useTransparency);
                else
                {
                    if (captureMode != GifCaptureMode.ScreenCapture)
                        Debug.LogError("Forced to use Screen Capture mode as cameras are not set.");

#if UNITY_2017_3_OR_NEWER
                    if (useAltScreenCapture)
                        newRawData = CaptureRawFrame.AltScreenCapture(cutoutRect, resizeResolution, !useTransparency, altScreenCaptureModeScale);
                        else
#endif
                        newRawData = CaptureRawFrame.ScreenCapture(cutoutRect, resizeResolution, !useTransparency);
                }

                TransformableTexture newFrame = new TransformableTexture(newRawData,captureTransformations);
                if (prepareForPreview)
                    newFrame.Finalize();
                frames.Add(newFrame);
                preparedFrames = null;
            }
        }

        Texture2D[] SynchronousPreProcess(bool save, string fullFilePath = "", int firstFrame = 0, int lastFrame = -1, bool reset = false)
        {
            List<Texture2D> result = new List<Texture2D>();
            IEnumerator processFunc = PreProcess(save, fullFilePath, firstFrame, lastFrame, reset);
            while (processFunc.MoveNext())
                result.Add(processFunc.Current as Texture2D);
            return result.ToArray();
        }

        IEnumerator PreProcess(bool save, string fullFilePath = "", int firstFrame = 0, int lastFrame = -1, bool reset = false)
        {
            if (frames.Count == 0)
                yield break;

            int[] frameRange = ValidatedFrameRange(firstFrame, lastFrame);
            firstFrame = frameRange[0];
            lastFrame = frameRange[1];

            reset |= resetAfterSave && save;
            recorderState = RecorderState.PreProcessing;

            List<GifFrame> gifFrames = new List<GifFrame>();
            List<Texture2D> frameTextures = new List<Texture2D>();

            processProgress = 0f;
            int framesToProcessCount = lastFrame - firstFrame + 1;
            for (int i = 0; i < framesToProcessCount; ++i)
            {
                int trueIndex;
                if (reverseMode)
                    trueIndex = lastFrame - i;
                else
                    trueIndex = firstFrame + i;

                TransformableTexture rawFrame = frames[trueIndex];
                if (!rawFrame.finalized)
                    rawFrame.Finalize();
                frameTextures.Add(rawFrame.finalTexture);

                if (save)
                {
                    GifFrame frame = new GifFrame() { Width = frameTextures[i].width, Height = frameTextures[i].height, Data = frameTextures[i].GetPixels32() };
                    gifFrames.Add(frame);
                }
                processProgress = (float)frameTextures.Count / (float)framesToProcessCount;
                yield return frameTextures[i];
            }

            if (pingPongMode)
            {
                // Subtract 2: 1 to get last index, 1 to avoid repeating last frame
                for (int i = frameTextures.Count - 2; i >= 0; --i)
                {
                    Texture2D texture = frameTextures[i];
                    frameTextures.Add(texture);
                    if (save)
                    {
                        GifFrame frame = new GifFrame() { Width = texture.width, Height = texture.height, Data = texture.GetPixels32() };
                        gifFrames.Add(frame);
                    }

                    yield return texture;
                }
            }

            if (GifTaken != null)
                GifTaken(this, frameTextures.ToArray(), playbackFramesPerSecond);

            if (save && GifToSave != null)
                GifToSave(this, frameTextures.ToArray(), playbackFramesPerSecond);

            // Switch the state to pause, let the user choose to keep recording or not
            recorderState = RecorderState.Paused;
            if (reset)
                Reset();

            if (save)
            {
                originalRunInBackground = Application.runInBackground;
                Application.runInBackground = true;

                saveProgress = 0f;

                // Setup a worker thread and let it do its magic
                int adjustRepeatCount = infiniteLoop ? 0 : (repeatCount <= 0 ? -1 : repeatCount);
                GifEncoder encoder = new GifEncoder(adjustRepeatCount, 101 - quality); // Inverse quality for GifEncorder
                encoder.SetDelay(Mathf.RoundToInt(playbackTimePerFrame * 1000f));

                if(fullFilePath.Length <= 0)
                    fullFilePath = fileSettings.FullFilePath();
                if (useCutout && cutoutScript != null)
                    fullFilePath = fileSettings.FullFilePath(cutoutScript.name);

                if (useTransparency)
                    encoder.SetTransparencyColor(transparencyColor);
                Worker worker = new Worker(workerPriority)
                {
                    m_Encoder = encoder,
                    m_Frames = gifFrames,
                    m_FilePath = fullFilePath,
                    m_OnFileSaved = WorkerSavedGif,
                    m_OnFileSaveProgress = WorkerGifSaveProgress
                };
                workerId = worker.m_Id;

#if UNITY_EDITOR || !UNITY_WEBGL
                worker.Start();
#else
                StartCoroutine(worker.EncodeFrames());
#endif
            }
        }

        void OnWorkerGifSaveProgress(int workerId, float progress)
        {
            if (workerId != this.workerId) return;

            saveProgress = progress;
            if (GifSaveProgress != null)
                GifSaveProgress(this, progress);
        }

        void OnWorkerSavedGif(int workerId, string fullFilePath)
        {
            if (workerId != this.workerId) return;

#if !UNITY_EDITOR
#if UNITY_IOS || UNITY_ANDROID
            if (fileSettings.saveToGallery)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(fullFilePath) + fileSettings.extension;
                NativeGallery.SaveImageToGallery(fullFilePath, fileSettings.album, fileName, (success, path) => { if (!fileSettings.persistLocallyMobile) { System.IO.File.Delete(fullFilePath); } if(!success) { Debug.LogError("Save to NativeGallery.SaveImageToGallery Failed"); } });
            }
#elif UNITY_WEBGL
            if (fileSettings.openInNewTab || fileSettings.download)
                Texture2DExtensions.SaveAccordingToFileSettingsWeb(System.IO.File.ReadAllBytes(fullFilePath), fileSettings);
            if(!fileSettings.persistLocallyWeb)
                System.IO.File.Delete(fullFilePath);
#endif
#endif
            Debug.Log("Gif saved: " + fullFilePath);

            saveProgress = 1f;
            lastSaveFilePath = fullFilePath;
            fileSettings.IncrementCount();
            fileSettings.SaveCount();

            if (GifSaved != null)
                GifSaved(this, fullFilePath);
            Application.runInBackground = originalRunInBackground;
        }

        int[] ValidatedFrameRange(int firstFrame = 0, int lastFrame = -1)
        {
            int lastPossibleFrame = frames.Count - 1;
            if (firstFrame < 0 || firstFrame > lastPossibleFrame)
                throw new UnityException("Invalid first frame index: " + firstFrame);
            if (lastFrame > lastPossibleFrame)
                throw new UnityException("Invalid last frame index: " + lastFrame);
            if (firstFrame > lastFrame)
                throw new UnityException("Invalid frame range  first frame: " + firstFrame + " greater than last frame:"  + lastFrame);
            if (lastFrame < 0)
                lastFrame = lastPossibleFrame;
            return new int[] { firstFrame, lastFrame };
        }

        void FlushMemory()
        {
            if (recorderState == RecorderState.PreProcessing)
            {
                Debug.LogWarning("Attempting to flush memory during the pre-processing step.");
                return;
            }

            if (frames != null)
            {
                foreach (TransformableTexture rawFrame in frames)
                    rawFrame.Destroy();
                frames = new List<TransformableTexture>();
                preparedFrames = null;
            }
        }

#region Helpers
#region Check if Buffer is Full
        bool ShouldAddFrame()
        {
            if (recorderState != RecorderState.Recording)
                return false;

            timeSinceLastFrame += Time.unscaledDeltaTime;
            bool addFrame = captureRate == GifCaptureRate.NaturalTiming;
            addFrame |= captureRate == GifCaptureRate.EveryXFrames && (Time.frameCount - lastFrameRecorded) % captureEveryXFrame == 0;
            addFrame |= captureRate == GifCaptureRate.FPS && (timeSinceLastFrame >= captureTimePerFrame || adjustTimeForCapture);
            addFrame |= captureRate == GifCaptureRate.VRHybrid && (timeSinceLastFrame >= captureTimePerFrame || adjustTimeForCapture) && (Time.frameCount - lastFrameRecorded) % captureEveryXFrame == 0;

            if (!addFrame)
                return false;

            // Limit the amount of frames stored in memory
            if (frames.Count >= maxFrameCount)
            {
                if (stopAutomatically)
                {
                    Pause();
                    return false;
                }

                frames.RemoveAt(0);
                preparedFrames = null;
            }

            if (oneFrameOnly)
                Pause();

            oneFrameOnly = false;
            lastFrameRecorded = Time.frameCount;
            timeSinceLastFrame -= captureTimePerFrame;
            return true;
        }
#endregion

#region Prepare/Restore Capture

        protected override void PrepareToCapture()
        {
            base.PrepareToCapture();

            if (adjustTimeForCapture)
            {
                originalCaptureFrameRate = Time.captureFramerate;
                Time.captureFramerate = captureFramesPerSecond;
            }
        }

        protected override void RestoreAfterCapture()
        {
            base.RestoreAfterCapture();

            if (adjustTimeForCapture)
                Time.captureFramerate = originalCaptureFrameRate;
        }
#endregion

#region Update Resolution(s)
        IEnumerator SwitchToRecordResolution()
        {
            UpdateResolutions();
            originalResolutionLockedIn = true;
            if (resizeBeforeRecording)
            {
#if UNITY_EDITOR
                originalSelectedSizeIndex = GameView.GetSelectedSizeIndex();
#endif
                Resolution currentResolution = ScreenExtensions.CurrentResolution();
                if (!recordResolution.IsSameSizeAs(currentResolution))
                {
                    ScreenExtensions.UpdateResolution(recordResolution);
                    yield return new WaitForResolutionUpdates();
                }
            }
        }

        public void UpdateResolutions()
        {
            if (!originalResolutionLockedIn)
                originalResolution = ScreenExtensions.CurrentResolution();
            recordResolution = originalResolution;
            if (resizeBeforeRecording)
            {
                if (useScaleToResizeBeforeRecording)
                    recordResolution = new Resolution { width = Mathf.FloorToInt((float)originalResolution.width * recordWidthScale), height = Mathf.FloorToInt((float)originalResolution.height * recordHeightScale) };
                else
                    recordResolution = new Resolution { width = recordWidth, height = recordHeight };
            }

            resizeResolution = recordResolution;
            if (resizeAfterRecording)
            {
                if (useScaleToResizeAfterRecording)
                    resizeResolution = new Resolution { width = Mathf.FloorToInt((float)recordResolution.width * resizeScale), height = Mathf.FloorToInt((float)recordResolution.height * resizeScale) };
                else
                    resizeResolution = new Resolution { width = recordWidth, height = recordHeight };
            }

            if (!useCutout || cutoutScript == null)
                return;

            Vector2 cutoutSize = cutoutScript.RectForResolution(resizeResolution).size;
            resizeResolution = new Resolution { width = Mathf.FloorToInt(cutoutSize.x), height = Mathf.FloorToInt(cutoutSize.y) };
        }
#endregion

#region Calculate Memory Size
        const int FILE_SIZE_LEVEL = 1024;
        static string[] fileSizes = { "B", "KB", "MB", "GB" };
        public string BytesToMaxWholeNumberSize(long bytes, int decimalPrecision = 1)
        {
            int level = 0;
            float finalSize = bytes;
            while (finalSize / (float)FILE_SIZE_LEVEL >= 1f)
            {
                ++level;
                finalSize = finalSize / (float)FILE_SIZE_LEVEL;
            }

            return finalSize.ToString("N" + decimalPrecision) + " " + fileSizes[level];
        }

        public string SizeOfFile(string fullFilePath, int decimalPrecision = 1)
        {
            return BytesToMaxWholeNumberSize(new System.IO.FileInfo(fullFilePath).Length, decimalPrecision);
        }
#endregion

#region Scene Change Helpers
        public override void UpdateDontDestroyOnLoad()
        {
            bool willDestroy = false;
            if (Application.isPlaying && dontDestroyOnLoad)
            {
                if (instances.Count < GetMaxInstances())
                {
                    instances.Add(this);
                    DontDestroyOnLoad(gameObject);
                }
                else
                {
                    MonoBehaviourExtended.FlexibleDestroy(gameObject);
                    willDestroy = true;
                }
            }

            if (!willDestroy)
                base.UpdateDontDestroyOnLoad();
        }
#endregion

#region Editor Variable Change Updates
        public void CaptureModeChanged()
        {
            Reset();
            AnyCameraChanged();
        }

        public override void AnyCameraChanged()
        {
            base.AnyCameraChanged();

            if (cameras == null || cameras.Count < 1 || cameras[0] == null || captureMode == GifCaptureMode.MultiCamera || captureMode == GifCaptureMode.ScreenCapture)
            {
                if (onRenderImageScript != null)
                    MonoBehaviourExtended.FlexibleDestroy(onRenderImageScript);
                if (asyncCaptureScript != null)
                    MonoBehaviourExtended.FlexibleDestroy(asyncCaptureScript);
                return;
            }

            if (captureMode == GifCaptureMode.SingleCamera)
            {
                // Make sure we're not persisting any extra capture scripts
                if (asyncCaptureScript != null)
                    MonoBehaviourExtended.FlexibleDestroy(asyncCaptureScript);
                asyncCaptureScript = cameras[0].gameObject.GetComponent<AsyncCaptureScript>();
                if (asyncCaptureScript != null)
                    MonoBehaviourExtended.FlexibleDestroy(asyncCaptureScript);

                if (onRenderImageScript == null || cameras[0].gameObject != onRenderImageScript.gameObject)
                {
                    MonoBehaviourExtended.FlexibleDestroy(onRenderImageScript);
                    onRenderImageScript = cameras[0].gameObject.GetComponent<OnRenderImageScript>();
                    if (onRenderImageScript == null)
                        onRenderImageScript = cameras[0].gameObject.AddComponent<OnRenderImageScript>();

                    onRenderImageScript.OnRenderImageListeners += NewRenderTexture;
                }
            }
#if UNITY_2018_2_OR_NEWER
            else if (captureMode == GifCaptureMode.Async)
            {
                // Make sure we're not persisting any extra capture scripts
                if (onRenderImageScript != null)
                    MonoBehaviourExtended.FlexibleDestroy(onRenderImageScript);
                onRenderImageScript = cameras[0].gameObject.GetComponent<OnRenderImageScript>();
                if (asyncCaptureScript != null)
                    MonoBehaviourExtended.FlexibleDestroy(onRenderImageScript);

                if (asyncCaptureScript == null || cameras[0].gameObject != asyncCaptureScript.gameObject)
                {
                    MonoBehaviourExtended.FlexibleDestroy(asyncCaptureScript);
                    asyncCaptureScript = cameras[0].gameObject.GetComponent<AsyncCaptureScript>();
                    if (asyncCaptureScript == null)
                        asyncCaptureScript = cameras[0].gameObject.AddComponent<AsyncCaptureScript>();
                }
            }
#endif
        }

#if UNITY_EDITOR
        protected override void ResolutionChanged()
        {
            base.ResolutionChanged();

            UpdateResolutions();
        }

        public override void CutoutValueChanged()
        {
            base.CutoutValueChanged();

            UpdateResolutions();
        }

        public void ResolutionsChanged()
        {
            UpdateResolutions();
        }
#endif
#endregion

#region Max Instances Count

        const string TRS_GIF_MAX_INSTANCES_KEY = "TRS_GIF_MAX_INSTANCES_KEY";

        static bool maxInstancesLoaded;
        static int cachedMaxInstances = 1;
        static List<GifScript> instances = new List<GifScript>();

        public override int GetMaxInstances()
        {
            if (maxInstancesLoaded)
                return cachedMaxInstances;
            return LoadMaxInstances();
        }

        public override void SetMaxInstances(int newValue)
        {
            maxInstancesLoaded = true;
            cachedMaxInstances = newValue;
            PlayerPrefs.SetInt(TRS_GIF_MAX_INSTANCES_KEY, newValue);
            PlayerPrefs.Save();
        }

        static int LoadMaxInstances()
        {
            if (PlayerPrefs.HasKey(TRS_GIF_MAX_INSTANCES_KEY))
                cachedMaxInstances = PlayerPrefs.GetInt(TRS_GIF_MAX_INSTANCES_KEY);
            else
                cachedMaxInstances = 1;
            maxInstancesLoaded = true;
            return cachedMaxInstances;
        }
#endregion
#endregion
    }
}
 