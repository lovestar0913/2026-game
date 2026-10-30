
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    [SerializeField] private BoxCollider2D spawnArea;
    [SerializeField] private GameObject warningPrefab;

    [Header("Enemy Settings")]
    [SerializeField] private SpawnEntry[] enemies;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float warningDuration = 2.5f;
    [SerializeField] private float blockRadius = 0.5f;

    [Header("Difficulty Settings")]
    [SerializeField] private int initialSpawnCount = 2;
    [SerializeField] private float increaseInterval = 30f;
    [SerializeField] private int increaseAmount = 1;

    private float gameStartTime;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (spawnArea == null || warningPrefab == null)
        {
            return;
        }

        gameStartTime = Time.time;

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            int spawnCount = GetCurrentSpawnCount();

            yield return StartCoroutine(SpawnWave(spawnCount));
        }
    }

    // 根據遊戲時間增加生成數量
    private int GetCurrentSpawnCount()
    {
        float elapsedTime = Time.time - gameStartTime;

        int increaseCount = 0;

        if (increaseInterval > 0f)
        {
            increaseCount =
                Mathf.FloorToInt(elapsedTime / increaseInterval);
        }

        return initialSpawnCount +
               increaseCount * increaseAmount;
    }

    // 一次安排多個生成位置
    private IEnumerator SpawnWave(int count)
    {
        List<GameObject> warnings = new List<GameObject>();
        List<Vector2> positions = new List<Vector2>();
        List<EnemyData> selectedEnemies = new List<EnemyData>();

        // 同時建立所有紅色 X
        for (int i = 0; i < count; i++)
        {
            EnemyData data = ChooseEnemy();

            if (data == null)
                continue;

            Vector2 position = GetRandomSpawnPosition();

            GameObject warning = Instantiate(
                warningPrefab,
                position,
                Quaternion.identity
            );

            SpawnWarningEffect effect =
                warning.GetComponent<SpawnWarningEffect>();

            if (effect != null)
                effect.Play(warningDuration);

            warnings.Add(warning);
            positions.Add(position);
            selectedEnemies.Add(data);
        }

        // 等待預警結束
        yield return new WaitForSeconds(warningDuration);

        // 分別判斷每個位置是否被玩家阻止
        for (int i = 0; i < warnings.Count; i++)
        {
            Vector2 position = positions[i];

            bool blocked = false;

            if (player != null)
            {
                float distance = Vector2.Distance(
                    player.position,
                    position
                );

                blocked = distance <= blockRadius;
            }

            Destroy(warnings[i]);

            if (blocked)
            {
                //Debug.Log("玩家成功阻止一隻怪物生成！");
            }
            else
            {
                SpawnEnemy(selectedEnemies[i], position);
            }
        }

    }

    // 在地圖範圍內隨機選擇位置
    private Vector2 GetRandomSpawnPosition()
    {
        Bounds bounds = spawnArea.bounds;

        float randomX = Random.Range(
            bounds.min.x,
            bounds.max.x
        );

        float randomY = Random.Range(
            bounds.min.y,
            bounds.max.y
        );

        return new Vector2(randomX, randomY);
    }

    // 加權隨機選擇敵人
    private EnemyData ChooseEnemy()
    {
        if (enemies == null || enemies.Length == 0)
            return null;

        float totalWeight = 0f;

        foreach (SpawnEntry entry in enemies)
        {
            if (entry == null ||
                entry.enemyData == null ||
                entry.enemyData.prefab == null ||
                entry.weight <= 0f)
                continue;

            totalWeight += entry.weight;
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue = Random.Range(0f, totalWeight);

        foreach (SpawnEntry entry in enemies)
        {
            if (entry == null ||
                entry.enemyData == null ||
                entry.enemyData.prefab == null ||
                entry.weight <= 0f)
                continue;

            randomValue -= entry.weight;

            if (randomValue <= 0f)
                return entry.enemyData;
        }

        return null;
    }

    // 生成敵人
    private void SpawnEnemy(EnemyData data, Vector2 position)
    {
        GameObject enemyObject = Instantiate(
            data.prefab,
            position,
            Quaternion.identity
        );

        EnemyMovement movement =
            enemyObject.GetComponent<EnemyMovement>();

        EnemyHealth health =
            enemyObject.GetComponent<EnemyHealth>();

        if (movement != null)
            movement.Initialize(data, player);

        if (health != null)
            health.Initialize(data);
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnArea == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            spawnArea.bounds.center,
            spawnArea.bounds.size
        );
    }
}
