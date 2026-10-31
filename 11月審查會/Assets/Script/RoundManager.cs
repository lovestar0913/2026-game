using UnityEngine;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
    [Header("Round Settings")]
    [SerializeField] private int totalRounds = 10;
    [SerializeField] private float roundDuration = 60f;

    [Header("UI")]
    [SerializeField] private Text roundText;
    [SerializeField] private Text timerText;

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
            roundText.text = "WAVE " + currentRound + " / " + totalRounds;

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