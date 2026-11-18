using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("地圖限制")]
    [SerializeField, Label("與地板邊緣的距離（約等於角色半徑）", 0f)]
    private float edgePadding = 0.5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 讀取玩家輸入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 避免斜向移動速度較快
        moveInput = new Vector2(horizontal, vertical).normalized;
    }

    private void FixedUpdate()
    {
        Vector2 velocity = moveInput * moveSpeed;

        if (ArenaFloor.Instance != null)
        {
            Rect bounds = GetBounds();

            // 如果這一步會走出範圍，就取消那個方向的移動（沿著邊緣滑動）
            Vector2 next = rb.position + velocity * Time.fixedDeltaTime;

            if ((next.x < bounds.xMin && velocity.x < 0f) ||
                (next.x > bounds.xMax && velocity.x > 0f))
                velocity.x = 0f;

            if ((next.y < bounds.yMin && velocity.y < 0f) ||
                (next.y > bounds.yMax && velocity.y > 0f))
                velocity.y = 0f;

            // 保險：萬一已經在範圍外（例如被推出去），直接拉回來
            Vector2 clamped = new Vector2(
                Mathf.Clamp(rb.position.x, bounds.xMin, bounds.xMax),
                Mathf.Clamp(rb.position.y, bounds.yMin, bounds.yMax));

            if (clamped != rb.position)
                rb.position = clamped;
        }

        // 使用物理更新處理移動
        rb.linearVelocity = velocity;
    }

    // 玩家中心點可以到達的範圍
    private Rect GetBounds()
    {
        Rect safe = ArenaFloor.Instance.SafeRect;

        float minX = safe.xMin + edgePadding;
        float maxX = safe.xMax - edgePadding;
        float minY = safe.yMin + edgePadding;
        float maxY = safe.yMax - edgePadding;

        // 地板太小的話固定在中央，避免範圍變成負的
        if (minX > maxX)
            minX = maxX = safe.center.x;

        if (minY > maxY)
            minY = maxY = safe.center.y;

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }
}