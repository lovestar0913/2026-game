
using UnityEngine;

public class PlayerAutoAttack : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private int bulletDamage = 1;

    private float attackTimer;

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        EnemyMovement target = FindNearestEnemy();

        if (target == null)
            return;

        Shoot(target);
        attackTimer = attackInterval;
    }

    // 取得敵人的身體中心
    private Vector2 GetEnemyCenter(EnemyMovement enemy)
    {
        // 搜尋敵人底下所有子物件
        Transform[] children =
            enemy.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == "AimPoint")
            {
                return child.position;
            }
        }

        return enemy.transform.position;
    }

    // 尋找最近的敵人
    private EnemyMovement FindNearestEnemy()
    {
        EnemyMovement[] enemies =
            FindObjectsByType<EnemyMovement>(
                FindObjectsSortMode.None
            );

        EnemyMovement nearestEnemy = null;
        float nearestDistanceSqr = attackRange * attackRange;

        foreach (EnemyMovement enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            Vector2 enemyCenter = GetEnemyCenter(enemy);

            float distanceSqr =
                (enemyCenter - (Vector2)transform.position).sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    // 發射子彈
    private void Shoot(EnemyMovement target)
    {
        if (bulletPrefab == null)
            return;

        Vector2 spawnPosition = firePoint != null
            ? (Vector2)firePoint.position
            : (Vector2)transform.position;

        // 瞄準敵人身體中心
        Vector2 targetPosition = GetEnemyCenter(target);

        Vector2 direction = targetPosition - spawnPosition;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        // 計算子彈旋轉角度
        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        Bullet bullet = Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.Euler(0f, 0f, angle)
        );

        bullet.Initialize(direction, bulletDamage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
