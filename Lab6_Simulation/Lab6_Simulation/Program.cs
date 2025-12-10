using Demographic.Engine;
using Demographic.FileOperations;
using DemographicFileOperations;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 5)
        {
            Console.WriteLine("Incorrect number of arguments!");
            return;
        }

        string initialPath = args[0];
        string deathPath = args[1];
        int startYear = int.Parse(args[2]);
        int endYear = int.Parse(args[3]);
        double totalPopulation = double.Parse(args[4]);

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