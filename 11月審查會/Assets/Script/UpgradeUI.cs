using System;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Text titleText;

    [Header("Buttons")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button critDamageButton;
    [SerializeField] private Button critRateButton;

    [Header("Button Labels")]
    [SerializeField] private Text attackLabel;
    [SerializeField] private Text critDamageLabel;
    [SerializeField] private Text critRateLabel;

    private int pointsRemaining;
    private Action onFinished;

    private void Awake()
    {
        Instance = this;

        attackButton.onClick.AddListener(OnAttackClicked);
        critDamageButton.onClick.AddListener(OnCritDamageClicked);
        critRateButton.onClick.AddListener(OnCritRateClicked);

        panel.SetActive(false);
    }

    public void Show(int upgradePoints, Action finished = null)
    {
        if (upgradePoints <= 0 || PlayerStats.Instance == null)
        {
            finished?.Invoke();
            return;
        }

        pointsRemaining = upgradePoints;
        onFinished = finished;

        Time.timeScale = 0f;   // 暫停遊戲
        panel.SetActive(true);
        RefreshUI();
    }

    private void OnAttackClicked()
    {
        PlayerStats.Instance.UpgradeAttack();
        UsePoint();
    }

    private void OnCritDamageClicked()
    {
        PlayerStats.Instance.UpgradeCritDamage();
        UsePoint();
    }

    private void OnCritRateClicked()
    {
        PlayerStats.Instance.UpgradeCritRate();
        UsePoint();
    }

    private void UsePoint()
    {
        pointsRemaining--;

        if (pointsRemaining <= 0)
        {
            FinishBasicUpgrade();
            return;
        }

        RefreshUI();
    }

    // 基礎升級完成後的下一步
    private void FinishBasicUpgrade()
    {
        // TODO: 商店完成後，在這裡改成開啟商店畫面，
        //       商店關閉時再呼叫 Close()
        Close();
    }

    private void Close()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;

        Action callback = onFinished;
        onFinished = null;
        callback?.Invoke();
    }

    private void RefreshUI()
    {
        PlayerStats stats = PlayerStats.Instance;

        if (titleText != null)
            titleText.text = "LEVEL UP!  剩餘點數：" + pointsRemaining;

        if (attackLabel != null)
            attackLabel.text =
                "攻擊力 +" + stats.AttackPerUpgrade +
                "\n目前：+" + stats.AttackBonus;

        if (critDamageLabel != null)
            critDamageLabel.text =
                "爆擊傷害 +" + Mathf.RoundToInt(stats.CritDamagePerUpgrade * 100f) + "%" +
                "\n目前：" + Mathf.RoundToInt(stats.CritDamage * 100f) + "%";

        if (critRateLabel != null)
            critRateLabel.text =
                "爆擊率 +" + Mathf.RoundToInt(stats.CritRatePerUpgrade * 100f) + "%" +
                "\n目前：" + Mathf.RoundToInt(stats.CritRate * 100f) + "%";
    }
}