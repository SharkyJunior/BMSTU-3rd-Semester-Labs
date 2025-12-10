using Demographic.Models;

using System.Globalization;

namespace Demographic.FileOperations;

public class CsvDataReader
{
    public IEnumerable<Person> Load(string path, double totalPopulation, int startYear)
    {
        var lines = File.ReadAllLines(path).Skip(1);
        var people = new List<Person>();
        var data = new Dictionary<int, double>();

        foreach (var line in lines)
        {
            var parts = line.Split(',');
            int age = int.Parse(parts[0].Trim());
            double count = double.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
            data[age] = count;
        }

        double sum = data.Values.Sum();
        double multiplier = totalPopulation / 1000.0 / sum;

        foreach (var kv in data)
        {
            int numPeople = (int)(kv.Value * multiplier);
            for (int i = 0; i < numPeople; i++)
            {
                bool female = i % 2 == 0;
                people.Add(new Person(startYear - kv.Key, female));
            }
        }

        return people;
    }
}
