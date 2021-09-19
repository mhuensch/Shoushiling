﻿// Based on: https://connect.unity.com/p/updating-your-gui-for-the-iphone-x-and-other-notched-devices
using UnityEngine;

namespace TRS.CaptureTool.Extras
{
    [AddComponentMenu("UI/SafeArea")]
    public class SafeArea : MonoBehaviour
    {
        RectTransform panel;
        Rect lastSafeArea = new Rect(0, 0, 0, 0);

        void Awake()
        {
            panel = GetComponent<RectTransform>();
            Refresh();
        }

        void Update()
        {
            Refresh();
        }

        void Refresh()
        {
            Rect safeArea = ScreenExtensions.SafeArea();

            if (safeArea != lastSafeArea)
                ApplySafeArea(safeArea);
        }

        void ApplySafeArea(Rect r)
        {
            lastSafeArea = r;

            // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
            Vector2 anchorMin = r.position;
            Vector2 anchorMax = r.position + r.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;

            //Debug.LogFormat("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}",
            //name, r.x, r.y, r.width, r.height, Screen.width, Screen.height);
        }
    }
}