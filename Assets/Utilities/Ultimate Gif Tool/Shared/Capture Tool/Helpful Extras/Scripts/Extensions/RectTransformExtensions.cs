using UnityEngine;

namespace TRS.CaptureTool.Extras
{
    public static class RectTransformExtensions
    {
        public static Rect RectForResolution(this RectTransform transform, Resolution resolution, bool positionRelative)
        {
            Rect resultRect = new Rect();
            RectTransform rectTransform = (RectTransform)transform;
            bool widthRelative = rectTransform.anchorMin.x != rectTransform.anchorMax.x;
            if (widthRelative)
                resultRect.width = resolution.width * (rectTransform.anchorMax.x - rectTransform.anchorMin.x);
            else
                resultRect.width = rectTransform.sizeDelta.x;
            bool heightRelative = rectTransform.anchorMin.y != rectTransform.anchorMax.y;
            if (heightRelative)
                resultRect.height = resolution.height * (rectTransform.anchorMax.y - rectTransform.anchorMin.y);
            else
                resultRect.height = rectTransform.sizeDelta.y;

            if (resultRect.width > resolution.width)
                resultRect.width = resolution.width;
            if (resultRect.height > resolution.height)
                resultRect.height = resolution.height;

            if (positionRelative)
            {
                if (widthRelative)
                    resultRect.x = resolution.width * rectTransform.anchorMin.x;
                else
                    resultRect.x = resolution.width * rectTransform.anchorMin.x - resultRect.width * rectTransform.pivot.x;


                if (heightRelative)
                    resultRect.y = resolution.height * rectTransform.anchorMin.y;
                else
                    resultRect.y = resolution.height * rectTransform.anchorMin.y - resultRect.height * rectTransform.pivot.y;
            }
            else
            {
                Vector2 position = new Vector2(rectTransform.position.x - rectTransform.pivot.x * resultRect.width,
                                                rectTransform.position.y - rectTransform.pivot.y * resultRect.height);
                resultRect.position = position;
            }

            resultRect = ErrorCheckRectForResolution(resultRect, resolution, false);

            return resultRect;
        }

        public static Rect RectForCurrentResolution(this RectTransform transform)
        {
#if UNITY_2017_3_OR_NEWER
            ((RectTransform)transform).ForceUpdateRectTransforms();
#endif
            Rect resultRect = transform.ToScreenSpace();
            Resolution resolution = ScreenExtensions.CurrentResolution();
            resultRect = ErrorCheckRectForResolution(resultRect, resolution);

            return resultRect;
        }

        public static Rect ToScreenSpace(this RectTransform transform)
        {
            Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            Vector2 position = new Vector2(transform.position.x - transform.pivot.x * size.x,
                                           transform.position.y - transform.pivot.y * size.y);
            return new Rect(position, size);
        }

        public static Rect ErrorCheckRectForResolution(Rect resultRect, Resolution resolution, bool logError = false)
        {
            string warningMessage = "";
            string errorMessage = "";
            if (resultRect.x < 0 || resultRect.y < 0)
                errorMessage = "Cutout Error: Top left corner of cutout is off screen for resolution: " + resolution;
            else if (resultRect.x > resolution.width || resultRect.y > resolution.height)
                errorMessage = "Cutout Error: Bottom right corner of cutout is off screen for resolution: " + resolution;
            else if (resultRect.x + resultRect.width > resolution.width || resultRect.y + resultRect.height > resolution.height)
            {
                warningMessage = "Cutout Error: Cutout is too large to fit on screen for resolution: " + resolution;
                if (resultRect.x + resultRect.width > resolution.width)
                    resultRect.width = resolution.width - resultRect.x;
                if (resultRect.y + resultRect.height > resolution.height)
                    resultRect.height = resolution.height - resultRect.y;
            }

            if (logError)
            {
                if (warningMessage.Length > 0)
                    Debug.LogError(errorMessage);
                if (errorMessage.Length > 0)
                    Debug.LogWarning(warningMessage);
            }

            return resultRect;
        }
    }
}
