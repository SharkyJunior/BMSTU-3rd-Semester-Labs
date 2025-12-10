using Demographic.Interfaces;
using System.Globalization;

namespace DemographicFileOperations;

public class CsvDeathRulesReader : IDeathRulesProvider
{
    private List<(int from, int to, double male, double female)> _rules = new();

    public void Load(string path)
    {
        _rules.Clear();
        foreach (var line in File.ReadAllLines(path).Skip(1))
        {
            var parts = line.Split(',');
            _rules.Add((
                int.Parse(parts[0].Trim()),
                int.Parse(parts[1].Trim()),
                double.Parse(parts[2].Trim(), CultureInfo.InvariantCulture),
                double.Parse(parts[3].Trim(), CultureInfo.InvariantCulture)
            ));
        }
    }

    public double GetProbability(bool isFemale, int age)
    {
        var rule = _rules.FirstOrDefault(r => age >= r.from && age <= r.to);
        return isFemale ? rule.female : rule.male;
    }
}