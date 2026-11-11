using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [Header("面板")]
    [SerializeField, Label("商店面板")] private GameObject panel;
    [SerializeField, Label("標題文字")] private Text titleText;
    [SerializeField, Label("金幣文字")] private Text coinText;
    [SerializeField, Label("下一波按鈕")] private Button nextWaveButton;

    [Header("商品按鈕")]
    [SerializeField, Label("攻擊力 按鈕")] private Button attackButton;
    [SerializeField, Label("爆擊傷害 按鈕")] private Button critDamageButton;
    [SerializeField, Label("爆擊率 按鈕")] private Button critRateButton;

    [Header("商品文字")]
    [SerializeField, Label("攻擊力 文字")] private Text attackLabel;
    [SerializeField, Label("爆擊傷害 文字")] private Text critDamageLabel;
    [SerializeField, Label("爆擊率 文字")] private Text critRateLabel;

    [Header("價格")]
    [SerializeField, Label("攻擊力 基礎價格", 0)] private int attackBasePrice = 10;
    [SerializeField, Label("爆擊傷害 基礎價格", 0)] private int critDamageBasePrice = 10;
    [SerializeField, Label("爆擊率 基礎價格", 0)] private int critRateBasePrice = 10;
    [SerializeField, Label("每次購買漲價", 0)] private int priceGrowth = 5;

    private int attackBought;
    private int critDamageBought;
    private int critRateBought;

    private void Awake()
    {
        Instance = this;

        if (panel == null || nextWaveButton == null ||
            attackButton == null || critDamageButton == null || critRateButton == null)
        {
            Debug.LogError("ShopUI：面板或按鈕欄位沒有指定，請在 Inspector 拖入。", this);
            return;
        }

        attackButton.onClick.AddListener(BuyAttack);
        critDamageButton.onClick.AddListener(BuyCritDamage);
        critRateButton.onClick.AddListener(BuyCritRate);
        nextWaveButton.onClick.AddListener(Close);

        panel.SetActive(false);
    }

    public void Show()
    {
        if (panel == null)
            return;

        panel.SetActive(true);
        RefreshUI();

        // 確認面板成功顯示後才暫停遊戲
        Time.timeScale = 0f;
    }

    private void Close()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    // ───────── 價格 ─────────

    private int AttackPrice => attackBasePrice + attackBought * priceGrowth;
    private int CritDamagePrice => critDamageBasePrice + critDamageBought * priceGrowth;
    private int CritRatePrice => critRateBasePrice + critRateBought * priceGrowth;

    // ───────── 購買 ─────────

    private void BuyAttack()
    {
        if (!TrySpend(AttackPrice)) return;

        PlayerStats.Instance.UpgradeAttack();
        attackBought++;
        RefreshUI();
    }

    private void BuyCritDamage()
    {
        if (!TrySpend(CritDamagePrice)) return;

        PlayerStats.Instance.UpgradeCritDamage();
        critDamageBought++;
        RefreshUI();
    }

    private void BuyCritRate()
    {
        if (!TrySpend(CritRatePrice)) return;

        PlayerStats.Instance.UpgradeCritRate();
        critRateBought++;
        RefreshUI();
    }

    private bool TrySpend(int price)
    {
        if (PlayerWallet.Instance == null || PlayerStats.Instance == null)
            return false;

        return PlayerWallet.Instance.SpendCoins(price);
    }

    // ───────── 更新畫面 ─────────

    private void RefreshUI()
    {
        PlayerStats stats = PlayerStats.Instance;
        int coins = PlayerWallet.Instance != null ? PlayerWallet.Instance.Coins : 0;

        if (titleText != null)
            titleText.text = "商店";

        if (coinText != null)
            coinText.text = "金幣：" + coins;

        if (stats == null)
            return;

        if (attackLabel != null)
            attackLabel.text =
                "攻擊力 +" + stats.AttackPerUpgrade +
                "\n價格：" + AttackPrice;

        if (critDamageLabel != null)
            critDamageLabel.text =
                "爆擊傷害 +" + Mathf.RoundToInt(stats.CritDamagePerUpgrade * 100f) + "%" +
                "\n價格：" + CritDamagePrice;

        if (critRateLabel != null)
            critRateLabel.text =
                "爆擊率 +" + Mathf.RoundToInt(stats.CritRatePerUpgrade * 100f) + "%" +
                "\n價格：" + CritRatePrice;

        // 金幣不夠就不能按
        attackButton.interactable = coins >= AttackPrice;
        critDamageButton.interactable = coins >= CritDamagePrice;
        critRateButton.interactable = coins >= CritRatePrice;
    }
}