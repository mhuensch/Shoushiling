using UnityEngine;
using System.Collections;

// Tangled Reality Studios - 9/1/18 - Added [AddComponentMenu("")]
public class CubeSpawner : MonoBehaviour
{
    public Transform CubePrefab;
    public float Interval = 0.5f;

    void Start()
    {
        if (CubePrefab == null)
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
                // Tangled Reality Studios - 7/28/18 - Added transform parameter to clean up hierarchy
                Instantiate(CubePrefab, transform.position, Random.rotationUniform, transform);
            }

            yield return null;
        }
    }
}
