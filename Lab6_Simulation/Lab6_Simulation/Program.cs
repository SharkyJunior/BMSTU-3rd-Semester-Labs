using Demographic.Engine;
using Demographic.FileOperations;
using DemographicFileOperations;

class Program
{
    static void Main(string[] args)
    {
        string initialPath = args.Length > 0 ? args[0] : "C:/Users/devli/RiderProjects/DemographicSolution/Demographic.Exec/InitialAge.csv";
        string deathPath = args.Length > 1 ? args[1] : "C:/Users/devli/RiderProjects/DemographicSolution/Demographic.Exec/DeathRules.csv";
        int startYear = args.Length > 2 ? int.Parse(args[2]) : 1970;
        int endYear = args.Length > 3 ? int.Parse(args[3]) : 2021;
        double totalPopulation = args.Length > 4 ? double.Parse(args[4]) : 130_000_000;

        var deathRules = new CsvDeathRulesReader();
        deathRules.Load(deathPath);

        var reader = new CsvDataReader();
        var population = reader.Load(initialPath, totalPopulation, startYear);

        var engine = new Engine(deathRules);
        engine.Initialize(population);
        var result = engine.Run(startYear, endYear);

        var writer = new CsvResultWriter();
        writer.Save("results.csv", result);

        Console.WriteLine("Моделирование завершено. Результаты сохранены в results.csv");
    }
} 