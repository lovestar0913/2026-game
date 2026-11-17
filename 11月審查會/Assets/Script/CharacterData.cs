using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCharacterData",
    menuName = "Roguelike/Character Data"
)]
public class CharacterData : ScriptableObject
{
    [Header("基本資料")]
    [Label("角色名稱")] public string characterName = "角色";
    [Tooltip("角色 Prefab。沒有指定頭像或全身圖時，會把整個 Prefab 拍成圖片顯示")]
    public GameObject prefab;
    [Tooltip("選擇畫面欄位用的頭像（可不指定）")]
    public Sprite portrait;
    [Tooltip("選中後顯示在畫面左側的全身圖（可不指定）")]
    public Sprite fullBody;
    [Tooltip("角色介紹")]
    [TextArea(2, 4)] public string description;

    [Header("屬性（顯示在屬性欄）")]
    [Label("血量", 1)] public int maxHP = 3;
    [Label("移動速度", 0f)] public float moveSpeed = 5f;
    [Label("攻擊力", 0)] public int attackDamage = 1;
    [Label("攻擊間隔（秒）", 0f)] public float attackInterval = 1f;
    [Label("攻擊範圍", 0f)] public float attackRange = 8f;
    [Label("爆擊率", 0f, 1f)] public float critRate = 0.05f;
    [Label("爆擊傷害倍率", 1f)] public float critDamage = 1.5f;

    [Header("被動技能")]
    [Label("命中回復機率（0.005 = 0.5%）", 0f, 1f)] public float healOnHitChance = 0.005f;
    [Label("命中回復量", 0)] public int healOnHitAmount = 1;

    [System.NonSerialized] private Sprite prefabSnapshot;

    // 被動說明文字（沒有被動時回傳空字串）
    public string GetPassiveText()
    {
        if (healOnHitChance <= 0f || healOnHitAmount <= 0)
            return "";

        return "ダメージを与えた際、" +
               (healOnHitChance * 100f).ToString("0.##") +
               "%の確率でHPが" + healOnHitAmount + "回復する";
    }

    // 頭像：頭像 → 全身圖 → Prefab 照片
    public Sprite GetPortrait()
    {
        if (portrait != null) return portrait;
        if (fullBody != null) return fullBody;
        return GetPrefabSprite();
    }

    // 全身圖：全身圖 → Prefab 照片
    public Sprite GetFullBody()
    {
        if (fullBody != null) return fullBody;
        return GetPrefabSprite();
    }

    private Sprite GetPrefabSprite()
    {
        if (prefab == null)
            return null;

        // 只拍一次，之後重複使用
        if (prefabSnapshot == null)
            prefabSnapshot = PrefabSnapshot.Render(prefab);

        return prefabSnapshot;
    }
}