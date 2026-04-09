using UnityEngine;

public class SpawnOcean : MonoBehaviour
{
    public Bounds bounds = new Bounds(Vector3.zero, new Vector3(14, 5, 0.1f));
    public float particleSpacing = 0.5f;
    public GameObject particlePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (float x = bounds.min.x; x < bounds.max.x; x += particleSpacing)
        {
            for (float y = bounds.min.y; y < bounds.max.y; y += particleSpacing)
            {
                Vector3 position = new Vector3(x, y, 0);
                Instantiate(
                    particlePrefab,
                    position,
                    Quaternion.identity,
                    transform
                );
            }
        }
    }
}
