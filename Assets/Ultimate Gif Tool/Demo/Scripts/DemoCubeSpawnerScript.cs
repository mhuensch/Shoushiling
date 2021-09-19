using UnityEngine;
using System.Collections;

namespace TRS.CaptureTool
{
    public class DemoCubeSpawnerScript : MonoBehaviour
    {
        public GameObject cubePrefab;
        public float Interval = 0.5f;

        void OnEnable()
        {
            if (cubePrefab == null)
                return;

            StartCoroutine(SpawnCube());
        }

        IEnumerator SpawnCube()
        {
            float timer = 0f;

            while (true)
            {
                timer += Time.deltaTime;

                if (timer > Interval)
                {
                    timer = 0f;
                    Instantiate(cubePrefab, transform.position, Random.rotationUniform, transform);
                }

                yield return null;
            }
        }
    }
}