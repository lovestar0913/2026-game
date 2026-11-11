using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [Header("子彈設定")]
    [Label("飛行速度")] public float speed = 10f;
    [Label("傷害")] public int damage = 1;
    [Label("存活時間（秒）")] public float lifetime = 3f;

    private Rigidbody2D rb;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 direction, int bulletDamage)
    {
        damage = bulletDamage;
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
            return;

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

        if (enemy == null)
            return;

        hasHit = true;

        // 套用攻擊力加成與爆擊
        int finalDamage = damage;

        if (PlayerStats.Instance != null)
            finalDamage = PlayerStats.Instance.CalculateDamage(damage, out bool isCrit);

        enemy.TakeDamage(finalDamage);

        Destroy(gameObject);
    }
}