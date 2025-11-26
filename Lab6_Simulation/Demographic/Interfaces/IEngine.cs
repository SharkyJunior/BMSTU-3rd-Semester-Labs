using Demographic.Results;
using Demographic.Models;

namespace Demographic.Interfaces;

public interface IEngine
{
    event EventHandler<int> YearTick;
    void Initialize(IEnumerable<Person> initialPopulation);
    SimulationResult Run(int startYear, int endYear);
}