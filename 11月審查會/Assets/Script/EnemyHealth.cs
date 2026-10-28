
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;

    private int currentHP;

    public int CurrentHP => currentHP;

    public void Initialize(EnemyData data)
    {
        enemyData = data;
        currentHP = enemyData != null ? enemyData.maxHP : 3;
    }

    private void Awake()
    {
        if (enemyData != null)
            currentHP = enemyData.maxHP;
        else
            currentHP = 3;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || currentHP <= 0)
            return;

        currentHP -= damage;

        //Debug.Log("敵人受到傷害，目前 HP：" + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
