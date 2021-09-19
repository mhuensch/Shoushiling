using UnityEngine;
using UnityEngine.UI;

namespace TRS.CaptureTool
{
    public class DemoCanSaveButtonScript : MonoBehaviour
    {
        public GifScript gifScript;
        Button button;

        void Awake()
        {
            button = GetComponent<Button>();
        }

        void Update()
        {
            button.interactable = gifScript.frames.Count > 0;
        }
    }
}