
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 10;
    public int currentHP;

    [Header("Damage Settings")]
    public float damageCooldown = 1f;

    private float lastDamageTime = -999f;

    private void Awake()
    {
        currentHP = maxHP;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyMovement enemy =
            other.GetComponentInParent<EnemyMovement>();

        if (enemy == null)
            return;

        int damage = enemy.Data != null
            ? enemy.Data.contactDamage
            : 1;

        TakeDamage(damage);
    }

    private void TakeDamage(int damage)
    {
        if (damage <= 0 || currentHP <= 0)
            return;

        if (Time.time < lastDamageTime + damageCooldown)
            return;

        lastDamageTime = Time.time;
        currentHP = Mathf.Max(0, currentHP - damage);

        //Debug.Log("玩家受到 " + damage +" 點傷害，目前 HP：" + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //Debug.Log("玩家死亡！");
    }
}
