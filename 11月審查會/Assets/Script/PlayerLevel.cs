using UnityEngine;
using UnityEngine.UI;

public class PlayerLevel : MonoBehaviour
{
    public static PlayerLevel Instance { get; private set; }

    [Header("等級設定")]
    [SerializeField, Label("1 升 2 級所需經驗")] private int baseXpToLevel = 5;
    [SerializeField, Label("每級額外所需經驗")] private int xpGrowthPerLevel = 3;

    [Header("UI（可不指定）")]
    [SerializeField, Label("等級文字")] private Text levelText;
    [SerializeField, Label("經驗條")] private Slider xpSlider;

    public int Level { get; private set; } = 1;
    public int CurrentXp { get; private set; }
    public int PendingUpgrades { get; private set; }

    public int XpToNext => baseXpToLevel + (Level - 1) * xpGrowthPerLevel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddXp(int amount)
    {
        if (amount <= 0) return;

        CurrentXp += amount;

        while (CurrentXp >= XpToNext)
        {
            CurrentXp -= XpToNext;
            Level++;
            PendingUpgrades++;
        }

        UpdateUI();
    }

    // 取出所有累積的升級點，並歸零
    public int ConsumeAllUpgrades()
    {
        int points = PendingUpgrades;
        PendingUpgrades = 0;
        return points;
    }

    private void UpdateUI()
    {
        if (levelText != null)
            levelText.text = "LV " + Level;

        if (xpSlider != null)
        {
            xpSlider.minValue = 0;
            xpSlider.maxValue = XpToNext;
            xpSlider.value = CurrentXp;
        }
    }
}