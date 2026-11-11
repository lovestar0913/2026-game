using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("外觀")]
    [SerializeField, Label("Sprite Renderer")] private SpriteRenderer spriteRenderer;
    [Tooltip("金幣圖案，每次隨機抽一個")]
    [SerializeField] private Sprite[] coinSprites;

    [Header("價值")]
    [SerializeField, Label("金幣價值")] private int value = 1;

    [Header("拾取")]
    [SerializeField, Label("拾取範圍")] private float magnetRange = 3f;
    [SerializeField, Label("拾取速度")] private float magnetSpeed = 10f;
    [SerializeField, Label("拾取距離")] private float collectDistance = 0.3f;

    [Header("掉落物")]
    [SerializeField, Label("彈開速度")] private float popSpeed = 3f;
    [SerializeField, Label("彈開時間")] private float popDuration = 0.25f;

    [Header("存活")]
    [SerializeField, Label("存活秒數（0不消失）")] private float lifetime = 0f;

    private Transform player;
    private Vector2 popVelocity;
    private float age;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // 從圖案陣列隨機抽一個
        if (spriteRenderer != null && coinSprites != null && coinSprites.Length > 0)
        {
            spriteRenderer.sprite = coinSprites[Random.Range(0, coinSprites.Length)];
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;

        popVelocity = Random.insideUnitCircle.normalized * popSpeed;

        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        age += Time.deltaTime;

        // 剛掉落：往外彈開並逐漸減速
        if (age < popDuration)
        {
            float t = 1f - (age / popDuration);
            transform.position += (Vector3)(popVelocity * t * Time.deltaTime);
            return;
        }

        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= collectDistance)
        {
            Collect();
            return;
        }

        // 進入範圍後被吸向玩家，越近越快
        if (distance <= magnetRange)
        {
            float speed = magnetSpeed * (1f + (1f - distance / magnetRange));
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                speed * Time.deltaTime
            );
        }
    }

    private void Collect()
    {
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.AddCoins(value);

        Destroy(gameObject);
    }
}