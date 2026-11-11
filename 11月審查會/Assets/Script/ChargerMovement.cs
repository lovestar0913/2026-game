using UnityEngine;

public class ChargerMovement : EnemyMovement
{
    private enum ChargerState
    {
        Chasing,
        Preparing,
        Dashing,
        Recovering
    }

    [Header("衝刺設定")]
    [SerializeField, Label("開始蓄力距離")] private float prepareDistance = 4f;
    [SerializeField, Label("蓄力時間")] private float prepareTime = 1f;
    [SerializeField, Label("衝刺速度")] private float dashSpeed = 6f;
    [SerializeField, Label("衝刺後休息時間")] private float recoveryTime = 1f;
    [SerializeField, Label("超出玩家的距離")] private float overshootDistance = 0.5f;

    [Header("警告線")]
    [SerializeField, Label("警告線材質")] private Material warningMaterial;
    [SerializeField, Label("警告線寬度")] private float warningWidth = 0.4f;
    [SerializeField, Label("警告線顏色")] private Color warningColor = new Color(1f, 0f, 0f, 1f);
    [SerializeField, Label("起始透明度")] private float warningMinAlpha = 0.25f;
    [SerializeField, Label("最終透明度")] private float warningMaxAlpha = 0.8f;
    [SerializeField, Label("排序層級")] private int warningSortingOrder = 5;

    // 所有衝鋒怪共用同一個預設材質，避免每隻敵人都建立新材質
    private static Material defaultWarningMaterial;

    private ChargerState currentState = ChargerState.Chasing;

    private float stateTimer;

    // 衝刺方向
    private Vector2 dashDirection;

    // 衝刺開始時鎖定的玩家位置
    private Vector2 dashTargetPosition;

    // 玩家位置再延伸 overshootDistance 的終點
    private Vector2 dashEndPosition;

    // 攻擊路徑警告線
    private LineRenderer warningLine;

    protected override void FixedUpdate()
    {
        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            HideWarning();
            return;
        }

        switch (currentState)
        {
            case ChargerState.Chasing:
                ChasePlayer();
                break;

            case ChargerState.Preparing:
                PrepareDash();
                break;

            case ChargerState.Dashing:
                Dash();
                break;

            case ChargerState.Recovering:
                Recover();
                break;
        }
    }

    // 正常追蹤玩家
    private void ChasePlayer()
    {
        Vector2 direction =
            (Vector2)player.position - rb.position;

        float distance = direction.magnitude;

        if (distance <= prepareDistance)
        {
            rb.linearVelocity = Vector2.zero;

            currentState = ChargerState.Preparing;
            stateTimer = prepareTime;

            return;
        }

        rb.linearVelocity = direction.normalized * Data.moveSpeed;
    }

    // 停止移動，顯示警告線，等待衝刺
    private void PrepareDash()
    {
        rb.linearVelocity = Vector2.zero;

        stateTimer -= Time.fixedDeltaTime;

        if (stateTimer > 0f)
        {
            // 蓄力中：警告線持續跟著玩家位置更新
            float progress = 1f - (stateTimer / prepareTime);
            UpdateWarning(progress);
            return;
        }

        // 蓄力結束，鎖定衝刺開始瞬間玩家的位置
        dashTargetPosition = player.position;

        Vector2 direction =
            dashTargetPosition - rb.position;

        if (direction.sqrMagnitude < 0.0001f)
        {
            HideWarning();
            currentState = ChargerState.Recovering;
            stateTimer = recoveryTime;
            return;
        }

        dashDirection = direction.normalized;

        // 衝刺終點：玩家位置 + 衝刺方向 * overshootDistance
        dashEndPosition =
            dashTargetPosition +
            dashDirection * overshootDistance;

        HideWarning();
        currentState = ChargerState.Dashing;
    }

    // 固定方向高速衝刺，不再追蹤玩家
    private void Dash()
    {
        // 計算距離終點還有多遠
        float remainingDistance = Vector2.Dot(
            dashEndPosition - rb.position,
            dashDirection
        );

        // 已經抵達終點
        if (remainingDistance <= 0f)
        {
            EndDash();
            return;
        }

        // 計算這一幀可以移動的距離
        float moveDistance = dashSpeed * Time.fixedDeltaTime;

        // 如果下一次移動會超過終點，就直接停在終點
        if (moveDistance >= remainingDistance)
        {
            rb.MovePosition(dashEndPosition);
            EndDash();
            return;
        }

        // 持續朝鎖定方向衝刺
        rb.linearVelocity = dashDirection * dashSpeed;
    }

    // 結束衝刺
    private void EndDash()
    {
        rb.linearVelocity = Vector2.zero;

        currentState = ChargerState.Recovering;
        stateTimer = recoveryTime;
    }

    // 衝刺結束後休息
    private void Recover()
    {
        rb.linearVelocity = Vector2.zero;

        stateTimer -= Time.fixedDeltaTime;

        if (stateTimer <= 0f)
        {
            currentState = ChargerState.Chasing;
        }
    }

    // ───────── 警告線 ─────────

    private void CreateWarningLine()
    {
        GameObject lineObject = new GameObject("ChargeWarningLine");
        lineObject.transform.SetParent(transform, false);

        warningLine = lineObject.AddComponent<LineRenderer>();
        warningLine.useWorldSpace = true;
        warningLine.positionCount = 2;
        warningLine.startWidth = warningWidth;
        warningLine.endWidth = warningWidth;
        warningLine.numCapVertices = 0;
        warningLine.sortingOrder = warningSortingOrder;
        warningLine.material = GetWarningMaterial();
        warningLine.enabled = false;
    }

    private Material GetWarningMaterial()
    {
        if (warningMaterial != null)
            return warningMaterial;

        if (defaultWarningMaterial == null)
            defaultWarningMaterial = new Material(Shader.Find("Sprites/Default"));

        return defaultWarningMaterial;
    }

    private void UpdateWarning(float progress)
    {
        if (warningLine == null)
            CreateWarningLine();

        Vector2 start = rb.position;
        Vector2 toPlayer = (Vector2)player.position - start;

        if (toPlayer.sqrMagnitude < 0.0001f)
        {
            warningLine.enabled = false;
            return;
        }

        // 路徑與實際衝刺相同：敵人 → 玩家位置 + 方向 * overshootDistance
        Vector2 direction = toPlayer.normalized;
        Vector2 end = (Vector2)player.position + direction * overshootDistance;

        warningLine.SetPosition(0, start);
        warningLine.SetPosition(1, end);

        // 蓄力越久越不透明
        Color color = warningColor;
        color.a = Mathf.Lerp(warningMinAlpha, warningMaxAlpha, progress);
        warningLine.startColor = color;
        warningLine.endColor = color;

        warningLine.enabled = true;
    }

    private void HideWarning()
    {
        if (warningLine != null)
            warningLine.enabled = false;
    }
}