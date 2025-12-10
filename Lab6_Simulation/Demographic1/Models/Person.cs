using Demographic.Engine;
using Demographic.Interfaces;

namespace Demographic.Models;

public class Person : IPerson
{
    public int BirthYear { get; }
    public bool IsFemale { get; }
    public bool IsAlive { get; private set; } = true;
    public int? DeathYear { get; private set; }

    public event EventHandler? ChildBirth;

    private double _birthProbability = 0.12;

    public Person(int birthYear, bool isFemale)
    {
        BirthYear = birthYear;
        IsFemale = isFemale;
    }

    public void OnYearPassed(int currentYear, double deathProbability)
    {
        if (!IsAlive) return;

        // Проверяем смерть
        if (ProbabilityCalculator.IsEventHappened(deathProbability))
        {
            IsAlive = false;
            DeathYear = currentYear;
            return;
        }

        // Проверяем рождение ребёнка
        int age = currentYear - BirthYear;
        if (IsFemale && age >= 18 && age <= 45)
        {
            if (ProbabilityCalculator.IsEventHappened(_birthProbability))
                ChildBirth?.Invoke(this, EventArgs.Empty);
        }
    }
}