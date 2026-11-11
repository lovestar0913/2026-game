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

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        DropCoins();
        GiveXp();
        Destroy(gameObject);
    }

    private void GiveXp()
    {
        if (enemyData == null || PlayerLevel.Instance == null)
            return;

        PlayerLevel.Instance.AddXp(enemyData.xpReward);
    }

    private void DropCoins()
    {
        if (enemyData == null || enemyData.coinPrefab == null)
            return;

        if (Random.value > enemyData.coinDropChance)
            return;

        int count = Random.Range(enemyData.minCoins, enemyData.maxCoins + 1);

        for (int i = 0; i < count; i++)
        {
            Instantiate(enemyData.coinPrefab, transform.position, Quaternion.identity);
        }
    }
}