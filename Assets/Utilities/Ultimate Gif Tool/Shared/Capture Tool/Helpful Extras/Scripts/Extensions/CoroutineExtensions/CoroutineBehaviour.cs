using System.Collections;
using UnityEngine;

namespace TRS.CaptureTool.Extras
{
    public class CoroutineBehaviour : MonoBehaviour
    {
        static MonoBehaviour Instance;

        public static void StaticStartCoroutine(IEnumerator iEnumerator)
        {
            InitializeInstance();
            Instance.StartCoroutine(iEnumerator);
        }

        public static void StaticStartCoroutineAfterYield<T>(System.Action action) where T : YieldInstruction, new()
        {
            InitializeInstance();
            Instance.StartCoroutineAfterYield<T>(action);
        }

        static void InitializeInstance()
        {
            if (Instance == null)
                Instance = new GameObject { hideFlags = HideFlags.HideAndDontSave }
                    .AddComponent<CoroutineBehaviour>();
        }
    }
}