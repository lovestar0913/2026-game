
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

    [Header("Prefab")]
    public GameObject prefab;
}
