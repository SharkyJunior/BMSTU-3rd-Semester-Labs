using BankingSimulationEngine;
using BankingSimulationEngine.Models;
using System.Threading.Channels;

public class TransactionQueueSystem
{
    // Очереди на проверку
    private readonly Channel<Transaction> _priorityCheckQueue;  // Для корпоративных
    private readonly Channel<Transaction> _normalCheckQueue;    // Для клиентских

    // Очереди на исполнение
    private readonly Channel<Transaction> _priorityExecuteQueue; // Для корпоративных
    private readonly Channel<Transaction> _normalExecuteQueue;   // Для клиентских

    // Очередь для ручной проверки (подозрительные транзакции)
    private readonly Channel<Transaction> _manualCheckQueue;

    private readonly SystemConfig _config;
    private readonly Random _random = new();
    private readonly object _metricsLock = new();

    // Метрики
    private readonly Dictionary<string, QueueMetrics> _queueMetrics = new();
    private long _totalTransactionsGenerated = 0;
    private long _totalTransactionsCompleted = 0;
    private long _totalTransactionsRejected = 0;

    // Управление потоками
    private readonly CancellationTokenSource _cts = new();
    private readonly List<Task> _runningTasks = new();

    public TransactionQueueSystem(SystemConfig config)
    {
        _config = config;

        // Инициализация очередей с ограничениями
        _priorityCheckQueue = Channel.CreateBounded<Transaction>(
            new BoundedChannelOptions(_config.PriorityQueueCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = false
            });

        _normalCheckQueue = Channel.CreateBounded<Transaction>(
            new BoundedChannelOptions(_config.NormalQueueCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = false
            });

        _priorityExecuteQueue = Channel.CreateBounded<Transaction>(
            new BoundedChannelOptions(_config.PriorityQueueCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = false
            });

        _normalExecuteQueue = Channel.CreateBounded<Transaction>(
            new BoundedChannelOptions(_config.NormalQueueCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = false
            });

        _manualCheckQueue = Channel.CreateUnbounded<Transaction>(
            new UnboundedChannelOptions { SingleWriter = false, SingleReader = false });

        // Инициализация метрик
        InitializeMetrics();
    }

    private void InitializeMetrics()
    {
        _queueMetrics["PriorityCheck"] = new QueueMetrics();
        _queueMetrics["NormalCheck"] = new QueueMetrics();
        _queueMetrics["PriorityExecute"] = new QueueMetrics();
        _queueMetrics["NormalExecute"] = new QueueMetrics();
        _queueMetrics["ManualCheck"] = new QueueMetrics();
    }

    public async Task StartSimulationAsync()
    {
        Console.WriteLine("Starting transaction processing system...");
        Console.WriteLine($"Simulation duration: {_config.SimulationDuration:hh\\:mm\\:ss}");

        // Запускаем эмиттеры
        _runningTasks.Add(Task.Run(() => GenerateClientTransactionsAsync(_cts.Token)));
        _runningTasks.Add(Task.Run(() => GenerateCorporateTransactionsAsync(_cts.Token)));

        // Запускаем обработчики
        _runningTasks.Add(Task.Run(() => AutoScreeningWorkerAsync(_cts.Token)));
        _runningTasks.Add(Task.Run(() => ManualScreeningWorkerAsync(_cts.Token)));
        _runningTasks.Add(Task.Run(() => ExecutionWorkerAsync(_cts.Token)));

        // Запускаем мониторинг
        _runningTasks.Add(Task.Run(() => MonitorMetricsAsync(_cts.Token)));

        // Ждем завершения симуляции
        await Task.Delay(_config.SimulationDuration, _cts.Token);
        await StopSimulationAsync();
    }

    private async Task GenerateClientTransactionsAsync(CancellationToken ct)
    {
        Console.WriteLine("Client transaction emitter started");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Генерируем клиентские переводы с частотой 5 RPS
                var interval = TimeSpan.FromMilliseconds(1000.0 / _config.ClientTransactionsPerSecond);
                await Task.Delay(interval, ct);

                var transaction = new Transaction
                {
                    Type = TransactionType.ClientTransfer,
                    Amount = (decimal)(_random.NextDouble() * 10000), // Сумма до 10,000
                    IsCorporate = false,
                    CreatedTime = DateTime.UtcNow
                };

                await EnqueueForCheckAsync(transaction);
                Interlocked.Increment(ref _totalTransactionsGenerated);

                if (_totalTransactionsGenerated % 100 == 0)
                {
                    Console.WriteLine($"Generated {_totalTransactionsGenerated} transactions total");
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in client emitter: {ex.Message}");
            }
        }
    }

    private async Task GenerateCorporateTransactionsAsync(CancellationToken ct)
    {
        Console.WriteLine("Corporate transaction emitter started");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Ждем случайный интервал 5-10 секунд
                var interval = TimeSpan.FromSeconds(
                    _random.Next(_config.CorporateBatchMinIntervalSeconds,
                                _config.CorporateBatchMaxIntervalSeconds + 1));
                await Task.Delay(interval, ct);

                // Генерируем пакет из 10-100 транзакций
                var batchSize = _random.Next(_config.CorporateBatchMinSize,
                                           _config.CorporateBatchMaxSize + 1);

                Console.WriteLine($"Generating corporate batch of {batchSize} transactions");

                for (int i = 0; i < batchSize && !ct.IsCancellationRequested; i++)
                {
                    var transaction = new Transaction
                    {
                        Type = TransactionType.CorporatePayment,
                        Amount = (decimal)(_random.NextDouble() * 100000), // Сумма до 100,000
                        IsCorporate = true,
                        CreatedTime = DateTime.UtcNow
                    };

                    await EnqueueForCheckAsync(transaction);
                    Interlocked.Increment(ref _totalTransactionsGenerated);

                    // Небольшая задержка между транзакциями в пакете
                    await Task.Delay(10, ct);
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in corporate emitter: {ex.Message}");
            }
        }
    }

    private async Task EnqueueForCheckAsync(Transaction transaction)
    {
        try
        {
            if (transaction.IsCorporate)
            {
                // Пытаемся положить в приоритетную очередь
                if (_priorityCheckQueue.Writer.TryWrite(transaction))
                {
                    UpdateQueueMetrics("PriorityCheck", 1);
                }
                else
                {
                    // Если переполнена - в обычную
                    await _normalCheckQueue.Writer.WriteAsync(transaction);
                    UpdateQueueMetrics("NormalCheck", 1);
                    Console.WriteLine($"Corporate transaction #{transaction.Id} routed to normal queue (priority full)");
                }
            }
            else
            {
                // Клиентские - всегда в обычную очередь
                await _normalCheckQueue.Writer.WriteAsync(transaction);
                UpdateQueueMetrics("NormalCheck", 1);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enqueueing transaction: {ex.Message}");
        }
    }

    private async Task AutoScreeningWorkerAsync(CancellationToken ct)
    {
        Console.WriteLine("Auto-screening worker started");

        var processingTime = TimeSpan.FromMilliseconds(1000.0 / _config.AutoScreeningRPS);

        while (!ct.IsCancellationRequested)
        {
            Transaction? transaction = null;

            try
            {
                // Пытаемся взять из приоритетной очереди, затем из обычной
                if (_priorityCheckQueue.Reader.TryRead(out transaction))
                {
                    UpdateQueueMetrics("PriorityCheck", -1);
                }
                else if (_normalCheckQueue.Reader.TryRead(out transaction))
                {
                    UpdateQueueMetrics("NormalCheck", -1);
                }
                else
                {
                    // Если обе очереди пусты, ждем
                    await Task.Delay(100, ct);
                    continue;
                }

                transaction.ScreeningStartTime = DateTime.UtcNow;
                transaction.Status = TransactionStatus.Screening;

                // Имитируем время обработки
                await Task.Delay(processingTime, ct);

                // Применяем вероятностную логику
                var randomValue = _random.NextDouble();

                if (randomValue < _config.AutoScreenPassRate)
                {
                    // 85% - пропускаем на исполнение
                    await EnqueueForExecutionAsync(transaction);
                }
                else if (randomValue < _config.AutoScreenPassRate + _config.AutoScreenRetryRate)
                {
                    // 10% - помечаем для ручной проверки и возвращаем
                    transaction.NeedsManualCheck = true;
                    transaction.IsSuspicious = true;
                    transaction.ProcessingAttempts++;

                    if (transaction.ProcessingAttempts <= _config.MaxRetryAttempts)
                    {
                        // Возвращаем в очередь на проверку
                        await EnqueueForManualCheckAsync(transaction);
                    }
                    else
                    {
                        // Превышено максимальное число попыток
                        transaction.Status = TransactionStatus.Rejected;
                        Interlocked.Increment(ref _totalTransactionsRejected);
                        Console.WriteLine($"Transaction #{transaction.Id} rejected (max attempts)");
                    }
                }
                else
                {
                    // 5% - сразу отклоняем
                    transaction.Status = TransactionStatus.Rejected;
                    Interlocked.Increment(ref _totalTransactionsRejected);
                    Console.WriteLine($"Transaction #{transaction.Id} auto-rejected");
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in auto-screening: {ex.Message}");
                // Возвращаем транзакцию в очередь при ошибке
                if (transaction != null)
                {
                    await EnqueueForCheckAsync(transaction);
                }
            }
        }
    }

    private async Task ManualScreeningWorkerAsync(CancellationToken ct)
    {
        Console.WriteLine("Manual screening worker started");

        var processingTime = TimeSpan.FromMilliseconds(1000.0 / _config.ManualScreeningRPS);

        while (!ct.IsCancellationRequested)
        {
            Transaction? transaction = null;

            try
            {
                // Пытаемся взять подозрительную транзакцию из очереди ручной проверки
                if (_manualCheckQueue.Reader.TryRead(out transaction))
                {
                    UpdateQueueMetrics("ManualCheck", -1);
                }
                else
                {
                    // Если нет подозрительных, берем случайную из обычной очереди на проверку
                    if (_normalCheckQueue.Reader.TryRead(out transaction))
                    {
                        UpdateQueueMetrics("NormalCheck", -1);
                        Console.WriteLine($"Manual check: random transaction #{transaction.Id} selected");
                    }
                    else
                    {
                        // Если все очереди пусты, ждем
                        await Task.Delay(1000, ct);
                        continue;
                    }
                }

                transaction.Status = TransactionStatus.ManualCheck;

                // Имитируем время ручной проверки (дольше автоматической)
                await Task.Delay(processingTime, ct);

                // Вероятностная логика ручной проверки
                var randomValue = _random.NextDouble();

                if (randomValue < _config.ManualScreenPassRate)
                {
                    // 70% - пропускаем на исполнение
                    await EnqueueForExecutionAsync(transaction);
                }
                else
                {
                    // 30% - отклоняем
                    transaction.Status = TransactionStatus.Rejected;
                    Interlocked.Increment(ref _totalTransactionsRejected);
                    Console.WriteLine($"Transaction #{transaction.Id} manually rejected");
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in manual screening: {ex.Message}");
            }
        }
    }

    private async Task ExecutionWorkerAsync(CancellationToken ct)
    {
        Console.WriteLine("Execution worker started");

        while (!ct.IsCancellationRequested)
        {
            Transaction? transaction = null;

            try
            {
                // Пытаемся взять из приоритетной очереди исполнения, затем из обычной
                if (_priorityExecuteQueue.Reader.TryRead(out transaction))
                {
                    UpdateQueueMetrics("PriorityExecute", -1);
                }
                else if (_normalExecuteQueue.Reader.TryRead(out transaction))
                {
                    UpdateQueueMetrics("NormalExecute", -1);
                }
                else
                {
                    // Если обе очереди пусты, ждем
                    await Task.Delay(100, ct);
                    continue;
                }

                transaction.ExecutionStartTime = DateTime.UtcNow;
                transaction.Status = TransactionStatus.Executing;

                // Имитируем время исполнения (1-2 RPS = 500-1000 мс)
                var minTime = 1000.0 / _config.ExecutionMaxRPS;
                var maxTime = 1000.0 / _config.ExecutionMinRPS;
                var executionTime = TimeSpan.FromMilliseconds(
                    minTime + (_random.NextDouble() * (maxTime - minTime)));

                await Task.Delay(executionTime, ct);

                // Завершаем транзакцию
                transaction.CompletionTime = DateTime.UtcNow;
                transaction.Status = TransactionStatus.Completed;
                Interlocked.Increment(ref _totalTransactionsCompleted);

                if (_totalTransactionsCompleted % 50 == 0)
                {
                    Console.WriteLine($"Completed {_totalTransactionsCompleted} transactions");
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in execution: {ex.Message}");
            }
        }
    }

    private async Task EnqueueForExecutionAsync(Transaction transaction)
    {
        try
        {
            // Корпоративные платежи, которые были в приоритетной очереди на проверку,
            // попадают в приоритетную очередь на исполнение
            if (transaction.IsCorporate && !transaction.NeedsManualCheck)
            {
                if (_priorityExecuteQueue.Writer.TryWrite(transaction))
                {
                    UpdateQueueMetrics("PriorityExecute", 1);
                }
                else
                {
                    // Если переполнена - в обычную
                    await _normalExecuteQueue.Writer.WriteAsync(transaction);
                    UpdateQueueMetrics("NormalExecute", 1);
                }
            }
            else
            {
                // Все остальные - в обычную очередь на исполнение
                await _normalExecuteQueue.Writer.WriteAsync(transaction);
                UpdateQueueMetrics("NormalExecute", 1);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enqueueing for execution: {ex.Message}");
        }
    }

    private async Task EnqueueForManualCheckAsync(Transaction transaction)
    {
        try
        {
            await _manualCheckQueue.Writer.WriteAsync(transaction);
            UpdateQueueMetrics("ManualCheck", 1);
            Console.WriteLine($"Transaction #{transaction.Id} queued for manual check");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enqueueing for manual check: {ex.Message}");
        }
    }

    private async Task MonitorMetricsAsync(CancellationToken ct)
    {
        Console.WriteLine("Metrics monitor started");

        var sampleInterval = TimeSpan.FromSeconds(5);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(sampleInterval, ct);
                PrintCurrentMetrics();
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private void UpdateQueueMetrics(string queueName, int delta)
    {
        lock (_metricsLock)
        {
            if (!_queueMetrics.ContainsKey(queueName))
                return;

            var metrics = _queueMetrics[queueName];
            metrics.CurrentLength = Math.Max(0, metrics.CurrentLength + delta);

            if (metrics.CurrentLength > metrics.MaxLength)
            {
                metrics.MaxLength = metrics.CurrentLength;
            }

            if (delta > 0)
            {
                metrics.TotalProcessed++;
            }
            else if (delta < 0)
            {
                // Можно добавить логику для расчета времени ожидания
            }
        }
    }

    private void PrintCurrentMetrics()
    {
        Console.WriteLine("\n═══════════════════════════════════════════════");
        Console.WriteLine($"System Metrics at {DateTime.Now:HH:mm:ss}");
        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine($"Total Generated: {_totalTransactionsGenerated}");
        Console.WriteLine($"Total Completed: {_totalTransactionsCompleted}");
        Console.WriteLine($"Total Rejected:  {_totalTransactionsRejected}");
        Console.WriteLine($"Success Rate:    {(_totalTransactionsCompleted * 100.0 / Math.Max(1, _totalTransactionsGenerated)):F1}%");
        Console.WriteLine("\nQueue Status:");

        lock (_metricsLock)
        {
            foreach (var kvp in _queueMetrics)
            {
                Console.WriteLine($"  {kvp.Key,-20}: Current={kvp.Value.CurrentLength,3} | Max={kvp.Value.MaxLength,3} | Processed={kvp.Value.TotalProcessed}");
            }
        }

        // Расчет примерной загрузки системы
        var totalQueueLength = _queueMetrics.Values.Sum(m => m.CurrentLength);
        var estimatedProcessingTime = totalQueueLength / (_config.AutoScreeningRPS + _config.ManualScreeningRPS);

        Console.WriteLine($"\nEstimated processing time: {estimatedProcessingTime:F1} seconds");
        Console.WriteLine("═══════════════════════════════════════════════\n");
    }

    private async Task StopSimulationAsync()
    {
        Console.WriteLine("\nStopping simulation...");

        _cts.Cancel();

        // Даем время на завершение текущих операций
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Завершаем все каналы
        _priorityCheckQueue.Writer.Complete();
        _normalCheckQueue.Writer.Complete();
        _priorityExecuteQueue.Writer.Complete();
        _normalExecuteQueue.Writer.Complete();
        _manualCheckQueue.Writer.Complete();

        // Ждем завершения всех задач
        await Task.WhenAll(_runningTasks);

        // Выводим финальные метрики
        Console.WriteLine("\n═══════════════════════════════════════════════");
        Console.WriteLine("           FINAL SIMULATION REPORT");
        Console.WriteLine("═══════════════════════════════════════════════");
        PrintCurrentMetrics();

        // Дополнительная статистика
        Console.WriteLine("\nAdditional Statistics:");
        var totalProcessed = _totalTransactionsCompleted + _totalTransactionsRejected;
        if (totalProcessed > 0)
        {
            var rejectionRate = (_totalTransactionsRejected * 100.0) / totalProcessed;
            Console.WriteLine($"Overall Rejection Rate: {rejectionRate:F1}%");
            Console.WriteLine($"Throughput: {_totalTransactionsCompleted / _config.SimulationDuration.TotalHours:F1} transactions/hour");
        }

        // Анализ очередей
        var maxQueueLength = _queueMetrics.Values.Max(m => m.MaxLength);
        var bottleneckQueue = _queueMetrics.FirstOrDefault(kvp => kvp.Value.MaxLength == maxQueueLength).Key;
        Console.WriteLine($"\nPotential Bottleneck: {bottleneckQueue} (max length: {maxQueueLength})");

        Console.WriteLine("═══════════════════════════════════════════════\n");
    }

    public async ValueTask DisposeAsync()
    {
        if (!_cts.IsCancellationRequested)
        {
            await StopSimulationAsync();
        }
        _cts.Dispose();
    }
}