using Demographic.Interfaces;
using Demographic.Models;
using Demographic.Results;

namespace Demographic.Engine;

public class Engine : IEngine
{
    public event EventHandler<int>? YearTick;
    private readonly IDeathRulesProvider _deathRules;
    private readonly List<Person> _population = new();

    public Engine(IDeathRulesProvider deathRules)
    {
        _deathRules = deathRules;
    }

    public void Initialize(IEnumerable<Person> initialPopulation)
    {
        _population.Clear();
        _population.AddRange(initialPopulation);
    }

    public SimulationResult Run(int startYear, int endYear)
    {
        var result = new SimulationResult();

        for (int year = startYear; year <= endYear; year++)
        {
            YearTick?.Invoke(this, year);

            // Новый список для рождающихся
            var newPeople = new List<Person>();

            foreach (var person in _population.Where(p => p.IsAlive))
            {
                double pDeath = _deathRules.GetProbability(person.IsFemale, year - person.BirthYear);
                person.OnYearPassed(year, pDeath);

                // Подписка на рождение
                person.ChildBirth += (s, e) =>
                {
                    bool female = ProbabilityCalculator.IsEventHappened(0.55);
                    newPeople.Add(new Person(year, female));
                };
            }

            _population.AddRange(newPeople);

            // Подсчёт статистики
            var alive = _population.Where(p => p.IsAlive).ToList();
            double total = alive.Count;
            double male = alive.Count(p => !p.IsFemale);
            double female = alive.Count(p => p.IsFemale);

            result.YearlyData.Add((year, total, male, female));
        }

        // Итоговая возрастная структура
        var aliveNow = _population.Where(p => p.IsAlive).ToList();
        result.FinalAgeGroups["0-18"] = (
            aliveNow.Count(p => !p.IsFemale && (endYear - p.BirthYear <= 18)),
            aliveNow.Count(p => p.IsFemale && (endYear - p.BirthYear <= 18))
        );
        result.FinalAgeGroups["19-45"] = (
            aliveNow.Count(p => !p.IsFemale && (endYear - p.BirthYear >= 19 && endYear - p.BirthYear <= 45)),
            aliveNow.Count(p => p.IsFemale && (endYear - p.BirthYear >= 19 && endYear - p.BirthYear <= 45))
        );
        result.FinalAgeGroups["46-65"] = (
            aliveNow.Count(p => !p.IsFemale && (endYear - p.BirthYear >= 46 && endYear - p.BirthYear <= 65)),
            aliveNow.Count(p => p.IsFemale && (endYear - p.BirthYear >= 46 && endYear - p.BirthYear <= 65))
        );
        result.FinalAgeGroups["66-100"] = (
            aliveNow.Count(p => !p.IsFemale && (endYear - p.BirthYear >= 66)),
            aliveNow.Count(p => p.IsFemale && (endYear - p.BirthYear >= 66))
        );

        return result;
    }
}