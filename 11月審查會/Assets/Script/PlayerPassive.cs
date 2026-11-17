using UnityEngine;

public class PlayerPassive : MonoBehaviour
{
    public static PlayerPassive Instance { get; private set; }

    [Header("設定")]
    [SerializeField, Label("玩家血量")] private PlayerHealth playerHealth;
    [Tooltip("直接開啟遊戲場景測試時（沒經過主畫面）使用這個角色的被動")]
    [SerializeField] private CharacterData fallbackCharacter;

    private float healChance;
    private int healAmount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        // 優先使用主畫面選的角色，沒有的話用測試角色
        CharacterData data = GameSettings.SelectedCharacterData != null
            ? GameSettings.SelectedCharacterData
            : fallbackCharacter;

        if (data == null)
            return;

        healChance = data.healOnHitChance;
        healAmount = data.healOnHitAmount;
    }

    // 玩家對敵人造成傷害時呼叫
    public void OnDamageDealt(int damage)
    {
        if (damage <= 0 || healChance <= 0f || healAmount <= 0)
            return;

        if (playerHealth == null)
            return;

        if (Random.value < healChance)
            playerHealth.Heal(healAmount);
    }
}