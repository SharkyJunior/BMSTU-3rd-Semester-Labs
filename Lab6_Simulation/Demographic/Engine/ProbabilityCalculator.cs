namespace Demographic.Engine;

public static class ProbabilityCalculator
{
    private static readonly Random _random = new();

    public static bool IsEventHappened(double probability)
        => _random.NextDouble() <= probability;
}