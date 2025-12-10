using System;
using System.Collections.Generic;
using System.Text;

namespace Lab7_BankTransactionSimulation.System
{
    public static class SystemLogger
    {
        private static readonly object _fileLock = new();
        private static string _logFilePath = $"transaction_log_{DateTime.Now:yyyyMMdd_HHmmss}.log";

        public static void Initialize(string? customPath = null)
        {
            if (!string.IsNullOrEmpty(customPath))
            {
                _logFilePath = customPath;
            }

            // Очищаем или создаем файл
            lock (_fileLock)
            {
                File.WriteAllText(_logFilePath, $"=== Transaction Processing System Log ===\n");
                File.AppendAllText(_logFilePath, $"Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
                File.AppendAllText(_logFilePath, new string('=', 80) + "\n\n");
            }

            LogInfo($"Log file initialized: {_logFilePath}");
        }

        public static void LogInfo(string message)
        {
            var logEntry = $"{DateTime.Now:HH:mm:ss} - INFO: {message}";
            WriteToFile(logEntry);
            Console.WriteLine(logEntry);
        }

        public static void LogTransaction(string message, Guid transactionId)
        {
            var logEntry = $"{DateTime.Now:HH:mm:ss} - TXN [{transactionId.ToString().Substring(0, 8)}]: {message}";
            WriteToFile(logEntry);
            Console.WriteLine(logEntry);
        }

        public static void LogEmitter(string emitterName, string message, Guid? transactionId = null)
        {
            var idPart = transactionId.HasValue ? $" [{transactionId.Value.ToString().Substring(0, 8)}]" : "";
            var logEntry = $"{DateTime.Now:HH:mm:ss} - EMITTER [{emitterName}]{idPart}: {message}";
            WriteToFile(logEntry);
            Console.WriteLine(logEntry);
        }

        public static void LogProcessor(string processorName, string message, Guid? transactionId = null)
        {
            var idPart = transactionId.HasValue ? $" [{transactionId.Value.ToString().Substring(0, 8)}]" : "";
            var logEntry = $"{DateTime.Now:HH:mm:ss} - PROCESSOR [{processorName}]{idPart}: {message}";
            WriteToFile(logEntry);
            Console.WriteLine(logEntry);
        }

        public static void LogQueue(string queueName, string message, Guid? transactionId = null)
        {
            var idPart = transactionId.HasValue ? $" [{transactionId.Value.ToString().Substring(0, 8)}]" : "";
            var logEntry = $"{DateTime.Now:HH:mm:ss} - QUEUE [{queueName}]{idPart}: {message}";
            WriteToFile(logEntry);
            Console.WriteLine(logEntry);
        }

        public static void LogError(string message, Exception? ex = null)
        {
            var logEntry = $"{DateTime.Now:HH:mm:ss} - ERROR: {message}";
            if (ex != null)
            {
                logEntry += $"\n           Exception: {ex.Message}";
            }
            WriteToFile(logEntry);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(logEntry);
            Console.ResetColor();
        }

        public static void LogMetrics(string message)
        {
            var logEntry = $"{DateTime.Now:HH:mm:ss} - METRICS: {message}";
            WriteToFile(logEntry);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(logEntry);
            Console.ResetColor();
        }

        public static void LogSeparator()
        {
            var separator = new string('-', 80);
            WriteToFile(separator);
            Console.WriteLine(separator);
        }

        private static void WriteToFile(string message)
        {
            lock (_fileLock)
            {
                try
                {
                    File.AppendAllText(_logFilePath, message + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }
        }

        public static string GetLogFilePath() => _logFilePath;
    }
}
