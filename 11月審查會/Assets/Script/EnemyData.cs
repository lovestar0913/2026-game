using UnityEngine;

[CreateAssetMenu(
    fileName = "NewEnemyData",
    menuName = "Roguelike/Enemy Data"
)]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";
    public Sprite sprite;

    [Header("Stats")]
    [Min(1)] public int maxHP = 3;
    [Min(0f)] public float moveSpeed = 2f;
    [Min(0)] public int contactDamage = 1;

    [Header("Drops")]
    public GameObject coinPrefab;
    [Range(0f, 1f)] public float coinDropChance = 0.5f;
    [Min(0)] public int minCoins = 1;
    [Min(0)] public int maxCoins = 2;

    [Header("Prefab")]
    public GameObject prefab;
}