using Lab7_BankTransactionSimulation.System;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Title = "Transaction Processing System with Logging";

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    TRANSACTION PROCESSING SYSTEM WITH LOGGING          ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
        Console.ResetColor();

        var config = new SystemConfig
        {
            ClientTransactionsPerSecond = 5,
            CorporateBatchMinIntervalSeconds = 5,
            CorporateBatchMaxIntervalSeconds = 10,
            CorporateBatchMinSize = 20,
            CorporateBatchMaxSize = 50,
            PriorityQueueCapacity = 50,
            AutoScreeningRPS = 20,
            ManualScreeningRPS = 0.25,
            ExecutionMinRPS = 2,
            ExecutionMaxRPS = 5,
            AutoScreenPassRate = 0.85,
            AutoScreenRetryRate = 0.10,
            AutoScreenRejectRate = 0.05,
            ManualScreenPassRate = 0.70,
            ManualScreenRejectRate = 0.30,
            SimulationDuration = TimeSpan.FromMinutes(2),
            MaxRetryAttempts = 3
        };

        Console.WriteLine("Configuration loaded. Log file will be created automatically.\n");
        Console.WriteLine("Press ENTER to start simulation...");
        Console.ReadLine();

        try
        {
            Console.WriteLine("\n" + new string('═', 80));
            Console.WriteLine("SIMULATION STARTED - Logging to file...");
            Console.WriteLine(new string('═', 80) + "\n");

            await using var system = new TransactionQueueSystem(config);
            await system.StartSimulationAsync();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✓ Simulation completed successfully!");
            Console.ResetColor();

            // Показываем путь к лог-файлу
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nLog file saved to: {SystemLogger.GetLogFilePath()}");
            Console.ResetColor();

            // Показываем последние 10 строк лога
            Console.WriteLine("\nLast 10 log entries:");
            Console.WriteLine(new string('-', 80));

            try
            {
                var logLines = File.ReadAllLines(SystemLogger.GetLogFilePath());
                var lastEntries = logLines.Skip(Math.Max(0, logLines.Length - 10));
                foreach (var line in lastEntries)
                {
                    Console.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not read log file: {ex.Message}");
            }
        }
        catch (TaskCanceledException)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n(!) Simulation was stopped by timeout.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n(X) Error during simulation: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine("\n" + new string('═', 80));
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}