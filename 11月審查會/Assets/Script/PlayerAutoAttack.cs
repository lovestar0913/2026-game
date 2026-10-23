
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

            float distanceSqr =
                ((Vector2)enemy.transform.position -
                 (Vector2)transform.position).sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }


    private void Shoot(EnemyMovement target)
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("尚未設定 Bullet Prefab！", this);
            return;
        }

        Vector2 spawnPosition = firePoint != null
            ? (Vector2)firePoint.position
            : (Vector2)transform.position;

        Vector2 direction =
            (Vector2)target.transform.position - spawnPosition;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        // 計算子彈飛行方向的角度
        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        // 生成子彈時，同時旋轉到目標方向
        Bullet bullet = Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.Euler(0f, 0f, angle)
        );

        bullet.Initialize(direction, bulletDamage);

        Debug.Log("玩家向敵人發射子彈：" + target.name);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
