using UnityEngine;
using UnityEngine.UI;

namespace TRS.CaptureTool.Extras
{
    public class UIMouseFollowScript : MouseFollowScript
    {
        RawImage mouseImage;

        void Awake()
        {
            mouseImage = GetComponent<RawImage>();
        }

        void Update()
        {
            mouseImage.rectTransform.anchoredPosition = FlexibleInput.MousePosition();
        }
    }
}