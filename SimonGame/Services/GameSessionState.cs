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

public sealed class GameSessionState
{
    public static readonly int[] SupportedGridSizes = [4, 5, 6];
    public static readonly GameColorMode[] SupportedColorModes = [GameColorMode.Random, GameColorMode.Mono];
    public static readonly MonoColorOption[] SupportedMonoColors =
    [
        new("Blue", 212),
        new("Green", 142),
        new("Purple", 276),
        new("Orange", 28),
        new("Teal", 186)
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
}
