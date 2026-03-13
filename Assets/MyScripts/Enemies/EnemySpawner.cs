using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    // public GameObject enemyPrefab;
    public GameObject[] enemyPrefabs;
    public Transform player;
    public int maxEnemies = 10;
    private Transform[] spawnPoints;
    public float spawnInterval = 4f;
    private float spawnTimer;
    public float minSpawnInterval = 1f;
    public float spawnAcceleration = 0.05f;
    public float minSpawnDistance = 15f;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
        // Initialize the array with the exact number of children (enemy spawn points)
        spawnPoints = new Transform[transform.childCount];

        // Loop through and assign each child to an index in the array
        for (int i = 0; i < transform.childCount; i++)
        {
            spawnPoints[i] = transform.GetChild(i);
        }

        Debug.Log("Array initialized with " + spawnPoints.Length + " spawn points.");
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            TrySpawnEnemy();
            spawnTimer = 0f;

            // gradually decrease the spawn interval to make game harder
            spawnInterval -= spawnAcceleration;

            if (spawnInterval < minSpawnInterval)
                spawnInterval = minSpawnInterval;
        }
    }
    
    void TrySpawnEnemy()
    {
        int currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (currentEnemies >= maxEnemies)
            return;

        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0)
            return;

        Transform spawnPoint = null;

        int attempts = 10;
        
        // Try to find a spawnPoint which is not near the player
        while (attempts > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);

            spawnPoint = spawnPoints[randomIndex];

            float distance = Vector3.Distance(player.position, spawnPoint.position);

            if (distance >= minSpawnDistance)
                break;

            attempts--;
        }

        if (spawnPoint != null)
        {   
            int enemyIndex = Random.Range(0, enemyPrefabs.Length);

            GameObject enemyPrefab = enemyPrefabs[enemyIndex];
            
            Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        // Debug.Log($"Number of enemies: {GameObject.FindGameObjectsWithTag("Enemy").Length}");
    }
}