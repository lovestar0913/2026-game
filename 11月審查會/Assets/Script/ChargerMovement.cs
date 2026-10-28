
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

    [Header("Charger Settings")]
    [SerializeField] private float prepareDistance = 4f;
    [SerializeField] private float prepareTime = 1f;
    [SerializeField] private float dashSpeed = 6f;
    [SerializeField] private float recoveryTime = 1f;
    [SerializeField] private float overshootDistance = 0.5f;

    private ChargerState currentState = ChargerState.Chasing;

    private float stateTimer;

    // 衝刺方向
    private Vector2 dashDirection;

    // 衝刺開始時鎖定的玩家位置
    private Vector2 dashTargetPosition;

    // 玩家位置再延伸 overshootDistance 的終點
    private Vector2 dashEndPosition;

    protected override void FixedUpdate()
    {
        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
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

    // 停止移動，等待衝刺
    private void PrepareDash()
    {
        rb.linearVelocity = Vector2.zero;

        stateTimer -= Time.fixedDeltaTime;

        if (stateTimer <= 0f)
        {
            // 鎖定衝刺開始瞬間玩家的位置
            dashTargetPosition = player.position;

            // 計算敵人到玩家的方向
            Vector2 direction =
                dashTargetPosition - rb.position;

            if (direction.sqrMagnitude < 0.0001f)
            {
                currentState = ChargerState.Recovering;
                stateTimer = recoveryTime;
                return;
            }

            dashDirection = direction.normalized;

            // 設定衝刺終點：
            // 玩家位置 + 衝刺方向 * 0.5
            dashEndPosition =
                dashTargetPosition +
                dashDirection * overshootDistance;

            currentState = ChargerState.Dashing;

        }
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
}
