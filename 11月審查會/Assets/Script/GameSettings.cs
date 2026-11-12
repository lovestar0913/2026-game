public enum Difficulty
{
    Easy,
    Hard
}

public static class GameSettings
{
    public static Difficulty CurrentDifficulty = Difficulty.Easy;
    public static int SelectedCharacter = 0;

    // 難度倍率（暫定值，可自行調整）
    public static float EnemyHpMultiplier =>
        CurrentDifficulty == Difficulty.Hard ? 1.5f : 1f;
}