using Demographic.Results;

namespace DemographicFileOperations;

public class CsvResultWriter
{
    public void Save(string resultsPath, SimulationResult result)
    {
        using var writer = new StreamWriter(resultsPath);
        writer.WriteLine("Year,Total,Male,Female");
        foreach (var (year, total, male, female) in result.YearlyData)
            writer.WriteLine($"{year},{total},{male},{female}");

        using var ageWriter = new StreamWriter("AgeStructure_2021.csv");
        ageWriter.WriteLine("Group,Male,Female");
        foreach (var kv in result.FinalAgeGroups)
            ageWriter.WriteLine($"{kv.Key},{kv.Value.Male},{kv.Value.Female}");
    }
}