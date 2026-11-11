using UnityEngine;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
    [Header("波數設定")]
    [SerializeField, Label("總波數")] private int totalRounds = 10;
    [SerializeField, Label("每波秒數")] private float roundDuration = 60f;

    [Header("UI")]
    [SerializeField, Label("波數文字")] private Text roundText;
    [SerializeField, Label("計時文字")] private Text timerText;

    private int currentRound = 1;
    private float remainingTime;

    public int CurrentRound => currentRound;
    public bool IsGameFinished { get; private set; }

    private void Start()
    {
        remainingTime = roundDuration;
        UpdateUI();
    }

    private void Update()
    {
        if (IsGameFinished) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            EndRound();
        }

        UpdateUI();
    }

    private void EndRound()
    {
        if (currentRound >= totalRounds)
        {
            FinishGame();
            return;
        }

        currentRound++;
        remainingTime = roundDuration;

        StartBetweenWaves();
    }

    // 波與波之間：有升級點先顯示升級畫面，之後一定進商店
    private void StartBetweenWaves()
    {
        int points = 0;

        if (PlayerLevel.Instance != null)
            points = PlayerLevel.Instance.ConsumeAllUpgrades();

        if (points > 0 && UpgradeUI.Instance != null)
        {
            UpgradeUI.Instance.Show(points, OpenShop);
            return;
        }

        OpenShop();
    }

    private void OpenShop()
    {
        if (ShopUI.Instance != null)
            ShopUI.Instance.Show();
    }

    private void FinishGame()
    {
        IsGameFinished = true;
        remainingTime = 0f;

        if (roundText != null)
            roundText.text = "ALL ROUNDS COMPLETED";

        if (timerText != null)
            timerText.text = "00:00";
    }

    private void UpdateUI()
    {
        if (IsGameFinished) return;

        if (roundText != null)
            roundText.text = "WAVE " + currentRound;

        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(remainingTime);
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;

            timerText.text =
                minutes.ToString("00") + ":" +
                remainingSeconds.ToString("00");
        }
    }
}