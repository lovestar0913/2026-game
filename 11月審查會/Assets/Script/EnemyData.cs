using UnityEngine;

[CreateAssetMenu(
    fileName = "NewEnemyData",
    menuName = "Roguelike/Enemy Data"
)]
public class EnemyData : ScriptableObject
{
    [Header("基本資料")]
    [Label("敵人名稱")] public string enemyName = "Enemy";
    [Label("圖片")] public Sprite sprite;

    [Header("能力")]
    [Label("最大血量", 1)] public int maxHP = 3;
    [Label("移動速度", 0f)] public float moveSpeed = 2f;
    [Label("接觸傷害", 0)] public int contactDamage = 1;

    [Header("掉落")]
    [Label("金幣 Prefab")] public GameObject coinPrefab;
    [Label("金幣掉落機率", 0f, 1f)] public float coinDropChance = 0.5f;
    [Label("最少金幣數", 0)] public int minCoins = 1;
    [Label("最多金幣數", 0)] public int maxCoins = 2;

    [Header("獎勵")]
    [Label("擊殺經驗值", 0)] public int xpReward = 1;

    [Header("Prefab")]
    [Label("敵人 Prefab")] public GameObject prefab;
}