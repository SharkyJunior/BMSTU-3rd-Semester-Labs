namespace Demographic.Interfaces;

public interface IPerson
{
    int BirthYear { get; }
    bool IsAlive { get; }
    bool IsFemale { get; }
    int? DeathYear { get; }

    event EventHandler ChildBirth;
    void OnYearPassed(int currentYear, double deathProbability);
}