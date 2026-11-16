using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Serializable]
    public class CharacterSlot
    {
        public CharacterData data;
        public bool unlocked;
        public Button button;
        public Text label;

        [NonSerialized] public Sprite defaultSprite;
    }

    [Header("場景")]
    [SerializeField, Label("遊戲場景名稱")] private string gameSceneName = "GameScene";

    [Header("標題畫面")]
    [SerializeField, Label("標題面板")] private GameObject titlePanel;
    [SerializeField, Label("全螢幕點擊按鈕")] private Button anyClickButton;

    [Header("選擇畫面")]
    [SerializeField, Label("選擇面板")] private GameObject selectPanel;

    [Header("角色欄位（頭像）")]
    [SerializeField] private CharacterSlot[] characterSlots;

    [Header("角色預覽（左側全身圖）")]
    [SerializeField, Label("全身圖")] private Image previewImage;
    [SerializeField, Label("未解鎖提示文字")] private Text lockedText;

    [Header("屬性欄（右側）")]
    [SerializeField, Label("角色名稱文字")] private Text nameText;
    [SerializeField, Label("屬性文字")] private Text statsText;
    [SerializeField, Label("角色介紹文字")] private Text descriptionText;
    [SerializeField, Label("未解鎖時顯示的文字")] private string lockedMessage = "尚未解鎖角色";

    [Header("難度與開始")]
    [SerializeField, Label("Easy 按鈕")] private Button easyButton;
    [SerializeField, Label("Hard 按鈕")] private Button hardButton;
    [SerializeField, Label("開始遊戲按鈕")] private Button startButton;

    [Header("選中外框")]
    [SerializeField, Label("外框顏色")] private Color selectedColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField, Label("外框粗細", 1f)] private float borderThickness = 6f;

    private int cursor;      // 目前選中的角色欄位
    private bool isReady;

    private void Awake()
    {
        // 避免從暫停中的遊戲回到主畫面時時間仍是停止的
        Time.timeScale = 1f;

        // 先設定面板顯示狀態：只顯示標題，隱藏選擇畫面
        if (titlePanel != null)
            titlePanel.SetActive(true);

        if (selectPanel != null)
            selectPanel.SetActive(false);

        if (titlePanel == null || selectPanel == null ||
            easyButton == null || hardButton == null || startButton == null ||
            characterSlots == null)
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

            if (slot.button == null)
                continue;

            // 記住原本的底圖，未解鎖時要還原
            if (slot.button.image != null)
                slot.defaultSprite = slot.button.image.sprite;

            slot.button.onClick.AddListener(() => SelectCharacter(index));
        }

        // 每次進主畫面都重設為預設選擇
        GameSettings.CurrentDifficulty = Difficulty.Easy;
        GameSettings.SelectedCharacter = 0;
        GameSettings.SelectedCharacterData = null;
        cursor = 0;

        isReady = true;
        RefreshSelectionUI();
    }

    private void Update()
    {
        if (!isReady || !titlePanel.activeSelf)
            return;

        // 剛載入的 0.3 秒內不接受輸入，避免被按 Play 的點擊誤觸
        if (Time.timeSinceLevelLoad < 0.3f)
            return;

        // 標題畫面：按下任意鍵（含滑鼠按鍵）進入選擇畫面
        if (Input.anyKeyDown)
            ShowSelectPanel();
    }

    // ───────── 面板切換 ─────────

    private void ShowSelectPanel()
    {
        titlePanel.SetActive(false);
        selectPanel.SetActive(true);
        RefreshSelectionUI();
    }

    // ───────── 選擇 ─────────

    private void SelectCharacter(int index)
    {
        if (index < 0 || index >= characterSlots.Length)
            return;

        cursor = index;
        RefreshSelectionUI();
    }

    private void SelectDifficulty(Difficulty difficulty)
    {
        GameSettings.CurrentDifficulty = difficulty;
        RefreshSelectionUI();
    }

    private void StartGame()
    {
        // 選中未解鎖（？）的角色時不能開始
        if (!IsAvailable(GetCursorSlot()))
            return;

        SceneManager.LoadScene(gameSceneName);
    }

    // ───────── 畫面更新 ─────────

    private bool IsAvailable(CharacterSlot slot)
    {
        return slot != null && slot.unlocked && slot.data != null;
    }

    private CharacterSlot GetCursorSlot()
    {
        if (cursor < 0 || cursor >= characterSlots.Length)
            return null;

        return characterSlots[cursor];
    }

    private void RefreshSelectionUI()
    {
        // 難度：只顯示外框
        SetBorder(easyButton, GameSettings.CurrentDifficulty == Difficulty.Easy);
        SetBorder(hardButton, GameSettings.CurrentDifficulty == Difficulty.Hard);

        // 角色欄位：頭像 + 外框
        for (int i = 0; i < characterSlots.Length; i++)
        {
            CharacterSlot slot = characterSlots[i];
            bool available = IsAvailable(slot);
            Sprite portrait = available ? slot.data.GetPortrait() : null;
            bool hasPortrait = portrait != null;

            if (slot.button != null && slot.button.image != null)
            {
                Image image = slot.button.image;
                image.sprite = hasPortrait ? portrait : slot.defaultSprite;
                image.preserveAspect = hasPortrait;
            }

            SetBorder(slot.button, i == cursor);

            if (slot.label != null)
            {
                if (!available)
                    slot.label.text = "?";
                else
                    slot.label.text = hasPortrait ? "" : slot.data.characterName;
            }
        }

        RefreshPreview();
    }

    // 更新左側全身圖與右側屬性欄
    private void RefreshPreview()
    {
        CharacterSlot slot = GetCursorSlot();
        bool available = IsAvailable(slot);

        if (available)
        {
            GameSettings.SelectedCharacter = cursor;
            GameSettings.SelectedCharacterData = slot.data;
        }

        // 左側：全身圖（未解鎖時顯示大問號）
        if (previewImage != null)
        {
            Sprite fullBody = available ? slot.data.GetFullBody() : null;
            bool showImage = fullBody != null;

            previewImage.enabled = showImage;

            if (showImage)
            {
                previewImage.sprite = fullBody;
                previewImage.preserveAspect = true;
            }
        }

        if (lockedText != null)
            lockedText.gameObject.SetActive(!available);

        // 右側：屬性欄（未解鎖時顯示提示文字）
        if (nameText != null)
            nameText.text = available ? slot.data.characterName : "？？？";

        if (statsText != null)
            statsText.text = available ? BuildStatsText(slot.data) : lockedMessage;

        if (descriptionText != null)
            descriptionText.text = available ? slot.data.description : "";

        // 未解鎖的角色不能開始遊戲
        startButton.interactable = available;
    }

    private string BuildStatsText(CharacterData data)
    {
        return
            "血量　　　" + data.maxHP + "\n" +
            "移動速度　" + data.moveSpeed.ToString("0.#") + "\n" +
            "攻擊力　　" + data.attackDamage + "\n" +
            "攻擊間隔　" + data.attackInterval.ToString("0.##") + " 秒\n" +
            "攻擊範圍　" + data.attackRange.ToString("0.#") + "\n" +
            "爆擊率　　" + Mathf.RoundToInt(data.critRate * 100f) + "%\n" +
            "爆擊傷害　" + Mathf.RoundToInt(data.critDamage * 100f) + "%";
    }

    // 只顯示外框，不改變按鈕本身的顏色
    private void SetBorder(Button button, bool selected)
    {
        if (button == null || button.image == null)
            return;

        Outline outline = button.GetComponent<Outline>();

        if (outline == null)
            outline = button.gameObject.AddComponent<Outline>();

        outline.effectColor = selectedColor;
        outline.effectDistance = new Vector2(borderThickness, borderThickness);
        outline.enabled = selected;
    }
}