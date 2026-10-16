
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public EnemyData enemyData;

        [Min(0f)]
        public float weight = 1f;
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private SpawnEntry[] enemies;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnDistance = 8f;
    [SerializeField] private int maxEnemies = 30;

    private float spawnTimer;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        if (player == null || enemies == null || enemies.Length == 0)
            return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            TrySpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }

    private void TrySpawnEnemy()
    {
        EnemyMovement[] aliveEnemies =
            FindObjectsByType<EnemyMovement>(
                FindObjectsSortMode.None
            );

        if (aliveEnemies.Length >= maxEnemies)
            return;

        EnemyData data = ChooseEnemy();

        if (data == null || data.prefab == null)
            return;

        Vector2 direction = Random.insideUnitCircle.normalized;

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.up;

        Vector2 position =
            (Vector2)player.position + direction * spawnDistance;

        GameObject obj = Instantiate(
            data.prefab,
            new Vector3(position.x, position.y, 0f),
            Quaternion.identity
        );

        EnemyMovement movement = obj.GetComponent<EnemyMovement>();
        EnemyHealth health = obj.GetComponent<EnemyHealth>();

        if (movement != null)
            movement.Initialize(data, player);

        if (health != null)
            health.Initialize(data);
    }

    private EnemyData ChooseEnemy()
    {
        float totalWeight = 0f;

        foreach (SpawnEntry entry in enemies)
        {
            if (entry.enemyData != null &&
                entry.enemyData.prefab != null &&
                entry.weight > 0f)
            {
                totalWeight += entry.weight;
            }
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue = Random.value * totalWeight;

        foreach (SpawnEntry entry in enemies)
        {
            if (entry.enemyData == null ||
                entry.enemyData.prefab == null ||
                entry.weight <= 0f)
                continue;

            randomValue -= entry.weight;

            if (randomValue <= 0f)
                return entry.enemyData;
        }

        return null;
    }
}
