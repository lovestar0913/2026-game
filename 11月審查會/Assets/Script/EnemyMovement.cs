
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;

    protected Rigidbody2D rb;
    protected Transform player;
    private SpriteRenderer spriteRenderer;

    public EnemyData Data => enemyData;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Initialize(EnemyData data, Transform target)
    {
        enemyData = data;
        player = target;

        ApplyEnemyData();
    }

    protected virtual void Start()
    {
        ApplyEnemyData();

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player == null)
        {
            Debug.LogError(
                name + " 確認 Player 的 Tag。",
                this
            );
        }
    }

    private void ApplyEnemyData()
    {
        if (enemyData == null)
            return;

        if (spriteRenderer != null && enemyData.sprite != null)
            spriteRenderer.sprite = enemyData.sprite;
    }

    protected virtual void FixedUpdate()
    {
        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 重新計算玩家的目前位置
        Vector2 targetPosition = player.position;

        Vector2 direction = targetPosition - rb.position;

        if (direction.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        direction.Normalize();

        float speed = enemyData != null
            ? enemyData.moveSpeed
            : 2f;

        rb.linearVelocity = direction * speed;
    }
}
