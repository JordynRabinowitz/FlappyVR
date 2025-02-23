using UnityEngine;

public class SpawnPipes: MonoBehaviour
{
    public GameObject PipeGoodPrefab; // Assign your prefab in the Inspector
    public GameObject PipeBadPrefab;
    public int spawnCount = 30;
    public float spawnHeightLimit = 50f;
    
    private Bounds groundBounds;

    void Start()
    {
        // Get the bounds of the ground
        Collider groundCollider = GameObject.FindWithTag("Ground").GetComponent<Collider>();
        if (groundCollider != null)
        {
            groundBounds = groundCollider.bounds;
            SpawnGoodPipes();
            SpawnBadPipes();
        }
        else
        {
            Debug.LogError("No object with tag 'Ground' found or missing Collider!");
        }
    }

    void SpawnGoodPipes()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            // Generate random position within ground area
            float x = Random.Range(groundBounds.min.x, groundBounds.max.x);
            float z = Random.Range(groundBounds.min.z, groundBounds.max.z);
            float y = Random.Range(groundBounds.min.y, spawnHeightLimit);

            Vector3 spawnPosition = new Vector3(x, y, z);
            Instantiate(PipeGoodPrefab, spawnPosition, Quaternion.identity);
        }
    }

    void SpawnBadPipes()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            // Generate random position within ground area
            float x = Random.Range(groundBounds.min.x, groundBounds.max.x);
            float z = Random.Range(groundBounds.min.z, groundBounds.max.z);
            float y = Random.Range(groundBounds.min.y, spawnHeightLimit);

            Vector3 spawnPosition = new Vector3(x, y, z);
            Instantiate(PipeBadPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
