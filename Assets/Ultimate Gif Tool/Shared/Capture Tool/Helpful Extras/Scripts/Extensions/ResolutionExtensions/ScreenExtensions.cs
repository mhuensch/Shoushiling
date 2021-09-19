using UnityEngine;

namespace TRS.CaptureTool.Extras
{
    public static class ScreenExtensions
    {
        public static bool resolutionUpdatesComplete { get; private set; }
        public static event System.Action<int, int> ResolutionUpdated;
        public static event System.Action<int, int> ResolutionUpdatesComplete;

        public static Resolution CurrentResolution()
        {
#if UNITY_EDITOR
            Vector2 size = GameView.CurrentSize();
            return new Resolution { width = (int)size.x, height = (int)size.y, refreshRate = Screen.currentResolution.refreshRate };
#else
            return new Resolution { width = Screen.width, height = Screen.height, refreshRate = Screen.currentResolution.refreshRate };
#endif
        }

        public static Rect SafeArea()
        {
#if !UNITY_EDITOR && UNITY_2019_1_2_OR_NEWER
            return Screen.safeArea;
#else
            Resolution currentResolution = ScreenExtensions.CurrentResolution();
            Resolution noRefreshRateResolution = new Resolution { width = currentResolution.width, height = currentResolution.height };
            if (AdditionalResolutions.safeAreaForResolution.ContainsKey(noRefreshRateResolution))
                return AdditionalResolutions.safeAreaForResolution[noRefreshRateResolution];
            return new Rect(0, 0, currentResolution.width, currentResolution.height);
#endif
        }

        public static void UpdateResolution(Resolution resolution)
        {
            UpdateResolution(resolution.width, resolution.height);
        }

        public static void UpdateResolution(Resolution resolution, bool fullScreen)
        {
            UpdateResolution(resolution.width, resolution.height, fullScreen);
        }

        public static void UpdateResolution(int width, int height)
        {
            bool inFullscreen = Screen.fullScreen;
            UpdateResolution(width, height, inFullscreen);
        }

        public static void UpdateResolution(int width, int height, bool fullscreen)
        {
            resolutionUpdatesComplete = false;

#if UNITY_EDITOR
            GameView.SetSize(GameView.GameViewSizeType.FixedResolution, width, height);
#else
		    Screen.SetResolution(width, height, fullscreen);
#endif

            ((System.Action)(() =>
            {
                if (ResolutionUpdated != null)
                    ResolutionUpdated(width, height);
                if (ResolutionUpdatesComplete != null)
                    ResolutionUpdatesComplete(width, height);
                resolutionUpdatesComplete = true;
            })).PerformAfterCoroutine<WaitForEndOfFrame>();
        }
    }
}