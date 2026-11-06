using UnityEngine;
using UnityEngine.UI;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [SerializeField] private Text coinText;

    public int Coins { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        UpdateUI();
    }

    public bool SpendCoins(int amount)
    {
        if (Coins < amount) return false;

        Coins -= amount;
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = "COINS: " + Coins;
    }
}