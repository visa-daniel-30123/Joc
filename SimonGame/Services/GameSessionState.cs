namespace SimonGame.Services;

public sealed class GameSessionState
{
    public static readonly int[] SupportedGridSizes = [4, 5, 6];

    public int GridSize { get; private set; } = 5;
    public int HighScore => GetHighScoreForMode(GridSize);

    private readonly Dictionary<int, int> highScoresByMode = SupportedGridSizes.ToDictionary(size => size, _ => 0);

    public event Action? Changed;

    public void RegisterScore(int score)
    {
        var currentModeHighScore = GetHighScoreForMode(GridSize);
        if (score <= currentModeHighScore)
        {
            return;
        }

        highScoresByMode[GridSize] = score;
        Changed?.Invoke();
    }

    public void SetGridSize(int gridSize)
    {
        if (!SupportedGridSizes.Contains(gridSize) || gridSize == GridSize)
        {
            return;
        }

        GridSize = gridSize;
        Changed?.Invoke();
    }

    public int GetHighScoreForMode(int gridSize)
    {
        return highScoresByMode.TryGetValue(gridSize, out var score) ? score : 0;
    }
}
