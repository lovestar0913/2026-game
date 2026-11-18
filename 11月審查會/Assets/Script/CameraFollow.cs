using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("追蹤目標")]
    [SerializeField, Label("目標（空白則自動找 Player）")] private Transform target;

    [Header("跟隨")]
    [SerializeField, Label("延遲時間（秒，越大越慢）", 0.01f)] private float smoothTime = 0.3f;
    [SerializeField, Label("不移動範圍半徑（0 = 關閉）", 0f)] private float deadZone = 0f;

    [Header("範圍限制")]
    [SerializeField, Label("限制在地板範圍內")] private bool clampToArena = true;
    [SerializeField, Label("可超出地板的距離（世界單位）", 0f)] private float outsideMargin = 2f;

    [Header("背景")]
    [SerializeField, Label("由腳本設定背景顏色")] private bool applyBackground = true;
    [SerializeField, Label("背景顏色")] private Color backgroundColor = Color.black;

    private Camera cam;
    private Vector2 velocity;
    private float fixedZ;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        fixedZ = transform.position.z;

        if (!cam.orthographic)
            Debug.LogWarning("CameraFollow：鏡頭不是 Orthographic，範圍限制會不準確。", this);

        // 地板以外的地方顯示純色背景
        if (applyBackground)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = backgroundColor;
        }
    }

    private void Start()
    {
        FindTarget();

        // 一開始直接對準玩家，不要從原點慢慢飄過去
        if (target != null)
        {
            Vector2 start = Clamp(target.position);
            transform.position = new Vector3(start.x, start.y, fixedZ);
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();

            if (target == null)
                return;
        }

        Vector2 current = transform.position;
        Vector2 desired = GetDesiredPosition(current);

        Vector2 next = Vector2.SmoothDamp(current, desired, ref velocity, smoothTime);
        next = Clamp(next);

        transform.position = new Vector3(next.x, next.y, fixedZ);
    }

    private Vector2 GetDesiredPosition(Vector2 current)
    {
        Vector2 playerPosition = target.position;
        Vector2 desired = playerPosition;

        if (deadZone > 0f)
        {
            Vector2 offset = playerPosition - current;

            // 玩家還在範圍內就不動，超出範圍才跟上去
            desired = offset.magnitude <= deadZone
                ? current
                : playerPosition - offset.normalized * deadZone;
        }

        return Clamp(desired);
    }

    // 限制鏡頭位置：可視範圍最多只能超出地板 outsideMargin 的距離
    private Vector2 Clamp(Vector2 position)
    {
        if (!clampToArena || ArenaFloor.Instance == null)
            return position;

        Rect rect = ArenaFloor.Instance.Rect;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = rect.xMin + halfWidth - outsideMargin;
        float maxX = rect.xMax - halfWidth + outsideMargin;
        float minY = rect.yMin + halfHeight - outsideMargin;
        float maxY = rect.yMax - halfHeight + outsideMargin;

        // 範圍比畫面小的話，就固定在中央
        position.x = minX > maxX ? rect.center.x : Mathf.Clamp(position.x, minX, maxX);
        position.y = minY > maxY ? rect.center.y : Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    private void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            target = player.transform;
    }
}