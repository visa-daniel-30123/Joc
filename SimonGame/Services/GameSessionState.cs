namespace SimonGame.Services;

public enum GameColorMode
{
    Random = 0,
    Mono = 1
}

public enum GameType
{
    Normal = 0,
    Hard = 1
}

public readonly record struct MonoColorOption(string Label, int Hue);
public readonly record struct GameStatEntry(
    DateTimeOffset CreatedAt,
    GameType GameType,
    string RunLabel,
    int Round,
    bool Correct,
    int ResponseMs,
    int ElapsedMs,
    int Score);

public sealed class GameSessionState
{
    public static readonly int[] SupportedGridSizes = [4, 5, 6];
    public static readonly GameColorMode[] SupportedColorModes = [GameColorMode.Random, GameColorMode.Mono];
    public static readonly MonoColorOption[] SupportedMonoColors =
    [
        new("Albastru", 212),
        new("Verde", 142),
        new("Mov", 276),
        new("Portocaliu", 28),
        new("Turcoaz", 186)
    ];

    public int GridSize { get; private set; } = 5;
    public GameColorMode ColorMode { get; private set; } = GameColorMode.Random;
    public int MonoHue { get; private set; } = SupportedMonoColors[0].Hue;
    public GameType ActiveGameType { get; private set; } = GameType.Normal;
    public int HighScore => GetHighScore(ActiveGameType, GridSize);

    private readonly Dictionary<(GameType Type, int GridSize), int> highScores =
        Enum.GetValues<GameType>()
            .SelectMany(type => SupportedGridSizes.Select(size => ((Type: type, GridSize: size), 0)))
            .ToDictionary(item => item.Item1, item => item.Item2);
    private readonly Dictionary<GameType, List<GameStatEntry>> statsByGameType =
        Enum.GetValues<GameType>().ToDictionary(type => type, _ => new List<GameStatEntry>());

    public event Action? Changed;

    public void RegisterScore(int score, GameType gameType)
    {
        var key = (gameType, GridSize);
        var currentModeHighScore = GetHighScore(gameType, GridSize);
        if (score <= currentModeHighScore)
        {
            return;
        }

        highScores[key] = score;
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

    public void SetColorMode(GameColorMode colorMode)
    {
        if (!SupportedColorModes.Contains(colorMode) || colorMode == ColorMode)
        {
            return;
        }

        ColorMode = colorMode;
        Changed?.Invoke();
    }

    public void SetMonoHue(int hue)
    {
        if (!SupportedMonoColors.Any(option => option.Hue == hue) || hue == MonoHue)
        {
            return;
        }

        MonoHue = hue;
        Changed?.Invoke();
    }

    public int GetHighScore(GameType gameType, int gridSize)
    {
        return highScores.TryGetValue((gameType, gridSize), out var score) ? score : 0;
    }

    public void SetActiveGameType(GameType gameType)
    {
        if (gameType == ActiveGameType)
        {
            return;
        }

        ActiveGameType = gameType;
        Changed?.Invoke();
    }

    public IReadOnlyList<GameStatEntry> GetStats(GameType gameType)
    {
        return statsByGameType[gameType];
    }

    public void AddStat(GameStatEntry entry)
    {
        var list = statsByGameType[entry.GameType];
        list.Insert(0, entry);

        // Keep the last 120 rows so UI stays snappy.
        if (list.Count > 120)
        {
            list.RemoveRange(120, list.Count - 120);
        }

        Changed?.Invoke();
    }
}
