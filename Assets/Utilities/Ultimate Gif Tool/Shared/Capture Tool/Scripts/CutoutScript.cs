using UnityEngine;
using UnityEngine.UI;

using TRS.CaptureTool.Extras;
namespace TRS.CaptureTool
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [RequireComponent(typeof(RectTransform))]
    public class CutoutScript : MonoBehaviour
    {
        [SerializeField]
        bool _preview;
        public bool preview
        {
            get { return _preview; }
            set { _preview = value; UpdateCutoutGraphic(); }
        }
        bool temporarilyHidden;

        public bool positionRelative = true;

        public Rect rect
        {
            get
            {
                transform.rotation = Quaternion.identity;
                return ((RectTransform)transform).RectForCurrentResolution();
            }
        }

        public bool clickToSelectPivot;
        Graphic cutoutGraphic;

        void Awake()
        {
            cutoutGraphic = GetComponent<Graphic>();
        }

        public void Show()
        {
            temporarilyHidden = false;
            UpdateCutoutGraphic();
        }

        public void Hide()
        {
            temporarilyHidden = true;
            UpdateCutoutGraphic();
        }

        void UpdateCutoutGraphic()
        {
            if (cutoutGraphic != null)
                cutoutGraphic.enabled = _preview && !temporarilyHidden;
        }

        public Rect RectForResolution(Resolution resolution)
        {
            return ((RectTransform)transform).RectForResolution(resolution, positionRelative);
        }

        void Update()
        {
            if (FlexibleInput.LeftMouseButton() && preview && clickToSelectPivot)
            {
                Resolution resolution = ScreenExtensions.CurrentResolution();
                RectTransform rectTransform = cutoutGraphic.rectTransform;
                if (positionRelative)
                {
                    float centerX = FlexibleInput.MousePosition().x / resolution.width;
                    float centerY = FlexibleInput.MousePosition().y / resolution.height;
                    float halfWidth = (rectTransform.anchorMax.x - rectTransform.anchorMin.x) / 2f;
                    float halfHeight = (rectTransform.anchorMax.y - rectTransform.anchorMin.y) / 2f;
                    rectTransform.anchoredPosition = new Vector2(0f, 0f);

                    if (rectTransform.anchorMin.x != rectTransform.anchorMax.x)
                    {
                        rectTransform.offsetMin = new Vector2(0f, rectTransform.offsetMin.y);
                        rectTransform.offsetMax = new Vector2(0f, rectTransform.offsetMax.y);
                    }
                    if (rectTransform.anchorMin.y != rectTransform.anchorMax.y)
                    {
                        rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0f);
                        rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0f);
                    }

                    rectTransform.anchorMin = new Vector2(centerX - halfWidth, centerY - halfHeight);
                    rectTransform.anchorMax = new Vector2(centerX + halfWidth, centerY + halfHeight);
                }
                else
                {
                    if (rectTransform.anchorMin.x != rectTransform.anchorMax.x)
                        rectTransform.sizeDelta = new Vector2((rectTransform.anchorMax.x - rectTransform.anchorMin.x) * resolution.width, rectTransform.sizeDelta.y);
                    if (rectTransform.anchorMin.y != rectTransform.anchorMax.y)
                        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, (rectTransform.anchorMax.y - rectTransform.anchorMin.y) * resolution.height);

                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0f, 0f);
                    rectTransform.anchoredPosition = FlexibleInput.MousePosition();
                }
            }
        }
    }
}