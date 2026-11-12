using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Serializable]
    public class CharacterSlot
    {
        public string characterName = "?";
        public bool unlocked;
        public Button button;
        public Text label;
    }

    [Header("場景")]
    [SerializeField, Label("遊戲場景名稱")] private string gameSceneName = "GameScene";

    [Header("標題畫面")]
    [SerializeField, Label("標題面板")] private GameObject titlePanel;
    [SerializeField, Label("全螢幕點擊按鈕")] private Button anyClickButton;

    [Header("選擇畫面")]
    [SerializeField, Label("選擇面板")] private GameObject selectPanel;
    [SerializeField, Label("角色欄位")] private CharacterSlot[] characterSlots;
    [SerializeField, Label("Easy 按鈕")] private Button easyButton;
    [SerializeField, Label("Hard 按鈕")] private Button hardButton;
    [SerializeField, Label("開始遊戲按鈕")] private Button startButton;

    [Header("選中顏色")]
    [SerializeField, Label("選中時顏色")] private Color selectedColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField, Label("未選中顏色")] private Color normalColor = Color.white;


    private void Awake()
    {
        // 避免從暫停中的遊戲回到主畫面時時間仍是停止的
        Time.timeScale = 1f;

        if (titlePanel == null || selectPanel == null ||
            easyButton == null || hardButton == null || startButton == null)
        {
            Debug.LogError("MainMenuUI：面板或按鈕欄位沒有指定，請在 Inspector 拖入。", this);
            return;
        }

        if (anyClickButton != null)
            anyClickButton.onClick.AddListener(ShowSelectPanel);

        easyButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Easy));
        hardButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Hard));
        startButton.onClick.AddListener(StartGame);

        for (int i = 0; i < characterSlots.Length; i++)
        {
            int index = i; // 避免閉包抓到迴圈變數
            CharacterSlot slot = characterSlots[i];

            if (slot.button != null)
                slot.button.onClick.AddListener(() => SelectCharacter(index));
        }

        // 每次進主畫面都重設為預設選擇
        GameSettings.CurrentDifficulty = Difficulty.Easy;
        GameSettings.SelectedCharacter = 0;

        ShowTitlePanel();
        RefreshSelectionUI();
    }

    private void Update()
    {
        if (titlePanel == null || !titlePanel.activeSelf)
            return;

        // 剛載入的 0.3 秒內不接受輸入，避免被按 Play 的點擊誤觸
        if (Time.timeSinceLevelLoad < 3f)
            return;

        // 按下任意鍵（含滑鼠按鍵）進入選擇畫面
        if (Input.anyKeyDown)
            ShowSelectPanel();
    }

    private void ShowTitlePanel()
    {
        titlePanel.SetActive(true);
        selectPanel.SetActive(false);
    }

    private void ShowSelectPanel()
    {
        titlePanel.SetActive(false);
        selectPanel.SetActive(true);
        RefreshSelectionUI();
    }

    private void SelectCharacter(int index)
    {
        if (index < 0 || index >= characterSlots.Length)
            return;

        if (!characterSlots[index].unlocked)
            return;

        GameSettings.SelectedCharacter = index;
        RefreshSelectionUI();
    }

    private void SelectDifficulty(Difficulty difficulty)
    {
        GameSettings.CurrentDifficulty = difficulty;
        RefreshSelectionUI();
    }

    private void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void RefreshSelectionUI()
    {
        // 難度按鈕
        SetButtonColor(easyButton, GameSettings.CurrentDifficulty == Difficulty.Easy);
        SetButtonColor(hardButton, GameSettings.CurrentDifficulty == Difficulty.Hard);

        // 角色欄位：未解鎖的顯示「?」且不能點
        if (characterSlots == null)
            return;

        for (int i = 0; i < characterSlots.Length; i++)
        {
            CharacterSlot slot = characterSlots[i];

            if (slot.label != null)
                slot.label.text = slot.unlocked ? slot.characterName : "?";

            if (slot.button != null)
            {
                slot.button.interactable = slot.unlocked;
                SetButtonColor(slot.button,
                    slot.unlocked && GameSettings.SelectedCharacter == i);
            }
        }
    }

    private void SetButtonColor(Button button, bool selected)
    {
        if (button != null && button.image != null)
            button.image.color = selected ? selectedColor : normalColor;
    }
}