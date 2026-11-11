using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("目前數值")]
    [SerializeField, Label("攻擊力加成")] private int attackBonus = 0;
    [SerializeField, Label("爆擊率（0~1）", 0f, 1f)] private float critRate = 0.05f;
    [SerializeField, Label("爆擊傷害倍率", 1f)] private float critDamage = 1.5f;

    [Header("每次升級增加量")]
    [SerializeField, Label("攻擊力 / 次")] private int attackPerUpgrade = 1;
    [SerializeField, Label("爆擊率 / 次（0.02 = 2%）")] private float critRatePerUpgrade = 0.02f;
    [SerializeField, Label("爆擊傷害 / 次（0.1 = 10%）")] private float critDamagePerUpgrade = 0.1f;
    [SerializeField, Label("爆擊率上限", 0f, 1f)] private float maxCritRate = 1f;

    public int AttackBonus => attackBonus;
    public float CritRate => critRate;
    public float CritDamage => critDamage;

    public int AttackPerUpgrade => attackPerUpgrade;
    public float CritRatePerUpgrade => critRatePerUpgrade;
    public float CritDamagePerUpgrade => critDamagePerUpgrade;

    private void Awake()
    {
        Instance = this;
    }

    // 傳入基礎傷害，回傳最終傷害（含攻擊力加成與爆擊）
    public int CalculateDamage(int baseDamage, out bool isCrit)
    {
        float damage = baseDamage + attackBonus;

        isCrit = Random.value < critRate;
        if (isCrit)
            damage *= critDamage;

        // 四捨五入（0.5 進位）
        return Mathf.Max(1, Mathf.FloorToInt(damage + 0.5f));
    }

    public void UpgradeAttack()
    {
        attackBonus += attackPerUpgrade;
    }

    public void UpgradeCritRate()
    {
        critRate = Mathf.Min(maxCritRate, critRate + critRatePerUpgrade);
    }

    public void UpgradeCritDamage()
    {
        critDamage += critDamagePerUpgrade;
    }
}