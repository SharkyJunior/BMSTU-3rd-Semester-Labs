using BankingSimulationEngine.Models;
using Lab7_BankTransactionSimulation.System;
using System.Threading.Channels;

public class TransactionQueueSystem : IAsyncDisposable
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
    private DateTime _simulationStart;
    private long _totalTransactionsGenerated = 0;
    private long _totalTransactionsCompleted = 0;
    private long _totalTransactionsRejected = 0;

    // Управление потоками
    private readonly CancellationTokenSource _cts = new();
    private readonly List<Task> _runningTasks = new();

    public TransactionQueueSystem(SystemConfig config)
    {
        _config = config;

        SystemLogger.Initialize($"transaction_system_{DateTime.Now:yyyyMMdd_HHmmss}.log");

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

        SystemLogger.LogInfo($"TransactionQueueSystem initialized with config:");
        SystemLogger.LogInfo($"- Client TPS: {config.ClientTransactionsPerSecond}");
        SystemLogger.LogInfo($"- Auto Screening RPS: {config.AutoScreeningRPS}");
        SystemLogger.LogInfo($"- Priority Queue Capacity: {config.PriorityQueueCapacity}");
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
        SystemLogger.LogInfo($"Starting simulation for {_config.SimulationDuration:hh\\:mm\\:ss}...");

        _simulationStart = DateTime.UtcNow;

        // Запускаем все компоненты
        _runningTasks.Add(Task.Run(() => GenerateClientTransactionsAsync(_cts.Token)));
        _runningTasks.Add(Task.Run(() => GenerateCorporateTransactionsAsync(_cts.Token)));
        _runningTasks.Add(Task.Run(() => AutoScreeningWorkerAsync(_cts.Token)));
        _runningTasks.Add(Task.Run(() => ManualScreeningWorkerAsync(_cts.Token)));
        _runningTasks.Add(Task.Run(() => ExecutionWorkerAsync(_cts.Token)));
        _runningTasks.Add(Task.Run(() => MonitorMetricsAsync(_cts.Token)));

        // Ждем завершения симуляции по времени
        await Task.Delay(_config.SimulationDuration, _cts.Token);
        await StopSimulationAsync();
    }

    private async Task GenerateClientTransactionsAsync(CancellationToken ct)
    {
        SystemLogger.LogEmitter("CLIENT", "Client transaction emitter started");

        int eventCounter = 0;
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var interval = TimeSpan.FromMilliseconds(1000.0 / _config.ClientTransactionsPerSecond);
                await Task.Delay(interval, ct);

                var transaction = new Transaction
                {
                    Type = TransactionType.ClientTransfer,
                    Amount = (decimal)(_random.NextDouble() * 10000),
                    IsCorporate = false,
                    CreatedTime = DateTime.UtcNow
                };

                eventCounter++;
                SystemLogger.LogEmitter("CLIENT",
                    $"Generated transaction #{eventCounter} (Amount: {transaction.Amount:C})",
                    transaction.Id);

                await EnqueueForCheckAsync(transaction);
                Interlocked.Increment(ref _totalTransactionsGenerated);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                SystemLogger.LogError($"Error in client emitter: {ex.Message}", ex);
            }
        }

        SystemLogger.LogEmitter("CLIENT", "Client transaction emitter stopped");
    }


    private async Task GenerateCorporateTransactionsAsync(CancellationToken ct)
    {
        SystemLogger.LogEmitter("CORPORATE", "Corporate transaction emitter started");

        int batchCounter = 0;
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var interval = TimeSpan.FromSeconds(
                    _random.Next(_config.CorporateBatchMinIntervalSeconds,
                                _config.CorporateBatchMaxIntervalSeconds + 1));
                await Task.Delay(interval, ct);

                var batchSize = _random.Next(_config.CorporateBatchMinSize,
                                           _config.CorporateBatchMaxSize + 1);

                batchCounter++;
                SystemLogger.LogEmitter("CORPORATE",
                    $"Starting batch #{batchCounter} with {batchSize} transactions");

                for (int i = 0; i < batchSize && !ct.IsCancellationRequested; i++)
                {
                    var transaction = new Transaction
                    {
                        Type = TransactionType.CorporatePayment,
                        Amount = (decimal)(_random.NextDouble() * 100000),
                        IsCorporate = true,
                        CreatedTime = DateTime.UtcNow
                    };

                    SystemLogger.LogEmitter("CORPORATE",
                        $"Generated corporate transaction (Amount: {transaction.Amount:C}, Batch: {batchCounter})",
                        transaction.Id);

                    await EnqueueForCheckAsync(transaction);
                    Interlocked.Increment(ref _totalTransactionsGenerated);

                    await Task.Delay(10, ct);
                }

                SystemLogger.LogEmitter("CORPORATE",
                    $"Completed batch #{batchCounter} with {batchSize} transactions");
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                SystemLogger.LogError($"Error in corporate emitter: {ex.Message}", ex);
            }
        }

        SystemLogger.LogEmitter("CORPORATE", "Corporate transaction emitter stopped");
    }

    private async Task EnqueueForCheckAsync(Transaction transaction)
    {
        try
        {
            if (transaction.IsCorporate)
            {
                if (_priorityCheckQueue.Writer.TryWrite(transaction))
                {
                    UpdateQueueMetrics("PriorityCheck", 1);
                    SystemLogger.LogQueue("PriorityCheck",
                        $"Corporate transaction enqueued. Queue length: {_queueMetrics["PriorityCheck"].CurrentLength}/{_config.PriorityQueueCapacity}",
                        transaction.Id);
                }
                else
                {
                    await _normalCheckQueue.Writer.WriteAsync(transaction);
                    UpdateQueueMetrics("NormalCheck", 1);
                    SystemLogger.LogQueue("NormalCheck",
                        $"Corporate transaction enqueued (priority queue full). Queue length: {_queueMetrics["NormalCheck"].CurrentLength}",
                        transaction.Id);
                }
            }
            else
            {
                await _normalCheckQueue.Writer.WriteAsync(transaction);
                UpdateQueueMetrics("NormalCheck", 1);
                SystemLogger.LogQueue("NormalCheck",
                    $"Client transaction enqueued. Queue length: {_queueMetrics["NormalCheck"].CurrentLength}",
                    transaction.Id);
            }
        }
        catch (Exception ex)
        {
            SystemLogger.LogError($"Error enqueueing transaction: {ex.Message}", ex);
        }
    }

    private async Task AutoScreeningWorkerAsync(CancellationToken ct)
    {
        SystemLogger.LogProcessor("AUTO_SCREEN", "Auto-screening worker started");

        var processingTime = TimeSpan.FromMilliseconds(1000.0 / _config.AutoScreeningRPS);

        while (!ct.IsCancellationRequested)
        {
            Transaction? transaction = null;

            try
            {
                if (_priorityCheckQueue.Reader.TryRead(out transaction))
                {
                    UpdateQueueMetrics("PriorityCheck", -1);
                    SystemLogger.LogQueue("PriorityCheck",
                        $"Transaction dequeued for screening. Queue length: {_queueMetrics["PriorityCheck"].CurrentLength}",
                        transaction.Id);
                }
                else if (_normalCheckQueue.Reader.TryRead(out transaction))
                {
                    UpdateQueueMetrics("NormalCheck", -1);
                    SystemLogger.LogQueue("NormalCheck",
                        $"Transaction dequeued for screening. Queue length: {_queueMetrics["NormalCheck"].CurrentLength}",
                        transaction.Id);
                }
                else
                {
                    await Task.Delay(100, ct);
                    continue;
                }

                transaction.ScreeningStartTime = DateTime.UtcNow;
                transaction.Status = TransactionStatus.Screening;

                SystemLogger.LogProcessor("AUTO_SCREEN",
                    $"Started screening (Processing time: {processingTime.TotalMilliseconds}ms)",
                    transaction.Id);

                await Task.Delay(processingTime, ct);

                var randomValue = _random.NextDouble();

                if (randomValue < _config.AutoScreenPassRate)
                {
                    // 85% - пропускаем
                    SystemLogger.LogProcessor("AUTO_SCREEN",
                        $"Screening PASSED → forwarding to execution",
                        transaction.Id);
                    await EnqueueForExecutionAsync(transaction);
                }
                else if (randomValue < _config.AutoScreenPassRate + _config.AutoScreenRetryRate)
                {
                    // 10% - помечаем для ручной проверки
                    transaction.NeedsManualCheck = true;
                    transaction.IsSuspicious = true;
                    transaction.ProcessingAttempts++;

                    SystemLogger.LogProcessor("AUTO_SCREEN",
                        $"Screening FLAGGED → requires manual check (Attempt: {transaction.ProcessingAttempts}/{_config.MaxRetryAttempts})",
                        transaction.Id);

                    if (transaction.ProcessingAttempts <= _config.MaxRetryAttempts)
                    {
                        await EnqueueForManualCheckAsync(transaction);
                    }
                    else
                    {
                        transaction.Status = TransactionStatus.Rejected;
                        Interlocked.Increment(ref _totalTransactionsRejected);
                        SystemLogger.LogProcessor("AUTO_SCREEN",
                            $"Transaction REJECTED (max attempts reached)",
                            transaction.Id);
                    }
                }
                else
                {
                    // 5% - отклоняем
                    transaction.Status = TransactionStatus.Rejected;
                    Interlocked.Increment(ref _totalTransactionsRejected);
                    SystemLogger.LogProcessor("AUTO_SCREEN",
                        $"Screening FAILED → transaction rejected",
                        transaction.Id);
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                SystemLogger.LogError($"Error in auto-screening: {ex.Message}", ex);
                if (transaction != null)
                {
                    await EnqueueForCheckAsync(transaction);
                }
            }
        }

        SystemLogger.LogProcessor("AUTO_SCREEN", "Auto-screening worker stopped");
    }

    private async Task ManualScreeningWorkerAsync(CancellationToken ct)
    {
        SystemLogger.LogProcessor("MANUAL_SCREEN", "Manual screening worker started");

        var processingTime = TimeSpan.FromMilliseconds(1000.0 / _config.ManualScreeningRPS);

        while (!ct.IsCancellationRequested)
        {
            Transaction? transaction = null;

            try
            {
                if (_manualCheckQueue.Reader.TryRead(out transaction))
                {
                    UpdateQueueMetrics("ManualCheck", -1);
                    SystemLogger.LogQueue("ManualCheck",
                        $"Suspicious transaction dequeued for manual check",
                        transaction.Id);
                }
                else if (_normalCheckQueue.Reader.TryRead(out transaction))
                {
                    UpdateQueueMetrics("NormalCheck", -1);
                    SystemLogger.LogProcessor("MANUAL_SCREEN",
                        $"Random transaction selected for manual check (sampling)",
                        transaction.Id);
                }
                else
                {
                    await Task.Delay(1000, ct);
                    continue;
                }

                transaction.Status = TransactionStatus.ManualCheck;

                SystemLogger.LogProcessor("MANUAL_SCREEN",
                    $"Started manual screening (Processing time: {processingTime.TotalSeconds:F1}s)",
                    transaction.Id);

                await Task.Delay(processingTime, ct);

                var randomValue = _random.NextDouble();

                if (randomValue < _config.ManualScreenPassRate)
                {
                    // 70% - пропускаем
                    SystemLogger.LogProcessor("MANUAL_SCREEN",
                        $"Manual screening PASSED → forwarding to execution",
                        transaction.Id);
                    await EnqueueForExecutionAsync(transaction);
                }
                else
                {
                    // 30% - отклоняем
                    transaction.Status = TransactionStatus.Rejected;
                    Interlocked.Increment(ref _totalTransactionsRejected);
                    SystemLogger.LogProcessor("MANUAL_SCREEN",
                        $"Manual screening FAILED → transaction rejected",
                        transaction.Id);
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                SystemLogger.LogError($"Error in manual screening: {ex.Message}", ex);
            }
        }

        SystemLogger.LogProcessor("MANUAL_SCREEN", "Manual screening worker stopped");
    }


    private async Task ExecutionWorkerAsync(CancellationToken ct)
    {
        SystemLogger.LogProcessor("EXECUTION", "Execution worker started");

        while (!ct.IsCancellationRequested)
        {
            Transaction? transaction = null;

            try
            {
                if (_priorityExecuteQueue.Reader.TryRead(out transaction))
                {
                    UpdateQueueMetrics("PriorityExecute", -1);
                    SystemLogger.LogQueue("PriorityExecute",
                        $"Corporate transaction dequeued for execution",
                        transaction.Id);
                }
                else if (_normalExecuteQueue.Reader.TryRead(out transaction))
                {
                    UpdateQueueMetrics("NormalExecute", -1);
                    SystemLogger.LogQueue("NormalExecute",
                        $"Transaction dequeued for execution",
                        transaction.Id);
                }
                else
                {
                    await Task.Delay(100, ct);
                    continue;
                }

                transaction.ExecutionStartTime = DateTime.UtcNow;
                transaction.Status = TransactionStatus.Executing;

                var minTime = 1000.0 / _config.ExecutionMaxRPS;
                var maxTime = 1000.0 / _config.ExecutionMinRPS;
                var executionTime = TimeSpan.FromMilliseconds(
                    minTime + (_random.NextDouble() * (maxTime - minTime)));

                SystemLogger.LogProcessor("EXECUTION",
                    $"Started execution (Time: {executionTime.TotalMilliseconds}ms, Amount: {transaction.Amount:C})",
                    transaction.Id);

                await Task.Delay(executionTime, ct);

                transaction.CompletionTime = DateTime.UtcNow;
                transaction.Status = TransactionStatus.Completed;

                var totalTime = (transaction.CompletionTime.Value - transaction.CreatedTime).TotalSeconds;

                Interlocked.Increment(ref _totalTransactionsCompleted);
                SystemLogger.LogProcessor("EXECUTION",
                    $"Transaction COMPLETED successfully (Total time: {totalTime:F2}s)",
                    transaction.Id);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                SystemLogger.LogError($"Error in execution: {ex.Message}", ex);
            }
        }

        SystemLogger.LogProcessor("EXECUTION", "Execution worker stopped");
    }

    private async Task EnqueueForExecutionAsync(Transaction transaction)
    {
        try
        {
            if (transaction.IsCorporate && !transaction.NeedsManualCheck)
            {
                if (_priorityExecuteQueue.Writer.TryWrite(transaction))
                {
                    UpdateQueueMetrics("PriorityExecute", 1);
                    SystemLogger.LogQueue("PriorityExecute",
                        $"Transaction enqueued for priority execution",
                        transaction.Id);
                }
                else
                {
                    await _normalExecuteQueue.Writer.WriteAsync(transaction);
                    UpdateQueueMetrics("NormalExecute", 1);
                    SystemLogger.LogQueue("NormalExecute",
                        $"Corporate transaction enqueued for normal execution (priority queue full)",
                        transaction.Id);
                }
            }
            else
            {
                await _normalExecuteQueue.Writer.WriteAsync(transaction);
                UpdateQueueMetrics("NormalExecute", 1);
                SystemLogger.LogQueue("NormalExecute",
                    $"Transaction enqueued for execution",
                    transaction.Id);
            }
        }
        catch (Exception ex)
        {
            SystemLogger.LogError($"Error enqueueing for execution: {ex.Message}", ex);
        }
    }

    private async Task EnqueueForManualCheckAsync(Transaction transaction)
    {
        try
        {
            await _manualCheckQueue.Writer.WriteAsync(transaction);
            UpdateQueueMetrics("ManualCheck", 1);
            SystemLogger.LogQueue("ManualCheck",
                $"Transaction enqueued for manual check. Queue length: {_queueMetrics["ManualCheck"].CurrentLength}",
                transaction.Id);
        }
        catch (Exception ex)
        {
            SystemLogger.LogError($"Error enqueueing for manual check: {ex.Message}", ex);
        }
    }

    private async Task MonitorMetricsAsync(CancellationToken ct)
    {
        SystemLogger.LogInfo("Metrics monitor started");

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

        SystemLogger.LogInfo("Metrics monitor stopped");
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
        SystemLogger.LogSeparator();
        SystemLogger.LogMetrics($"System Metrics at {DateTime.Now:HH:mm:ss}");
        SystemLogger.LogMetrics($"Total Generated: {_totalTransactionsGenerated}");
        SystemLogger.LogMetrics($"Total Completed: {_totalTransactionsCompleted}");
        SystemLogger.LogMetrics($"Total Rejected:  {_totalTransactionsRejected}");

        if (_totalTransactionsGenerated > 0)
        {
            var successRate = (_totalTransactionsCompleted * 100.0) / _totalTransactionsGenerated;
            SystemLogger.LogMetrics($"Success Rate:    {successRate:F1}%");
        }

        SystemLogger.LogMetrics("Queue Status:");

        lock (_metricsLock)
        {
            foreach (var kvp in _queueMetrics)
            {
                SystemLogger.LogMetrics($"  {kvp.Key,-20}: Current={kvp.Value.CurrentLength,3} | Max={kvp.Value.MaxLength,3}");
            }
        }

        var totalQueueLength = _queueMetrics.Values.Sum(m => m.CurrentLength);
        SystemLogger.LogMetrics($"Total in queues: {totalQueueLength}");
        SystemLogger.LogSeparator();
    }

    private async Task StopSimulationAsync()
    {
        SystemLogger.LogInfo("Stopping simulation...");

        _cts.Cancel();

        await Task.Delay(TimeSpan.FromSeconds(2));

        // Завершаем все каналы
        _priorityCheckQueue.Writer.Complete();
        _normalCheckQueue.Writer.Complete();
        _priorityExecuteQueue.Writer.Complete();
        _normalExecuteQueue.Writer.Complete();
        _manualCheckQueue.Writer.Complete();

        await Task.WhenAll(_runningTasks);

        // Финальный отчет
        SystemLogger.LogSeparator();
        SystemLogger.LogInfo("FINAL SIMULATION REPORT");
        SystemLogger.LogSeparator();

        var totalProcessed = _totalTransactionsCompleted + _totalTransactionsRejected;
        var percentageProcessed = _totalTransactionsCompleted / _totalTransactionsGenerated * 100;
        if (totalProcessed > 0)
        {
            var rejectionRate = (_totalTransactionsRejected * 100.0) / totalProcessed;
            SystemLogger.LogInfo($"Total Transactions Generated: {_totalTransactionsGenerated}");
            SystemLogger.LogInfo($"Total Transactions Processed: {totalProcessed} ({percentageProcessed:F1}%)");
            SystemLogger.LogInfo($"Total Transactions Rejected: {_totalTransactionsRejected}");
            SystemLogger.LogInfo($"Overall Rejection Rate: {rejectionRate:F1}%");
            SystemLogger.LogInfo($"Throughput: {_totalTransactionsCompleted / _config.SimulationDuration.TotalHours:F1} transactions/hour");
        }

        // Информация о лог-файле
        SystemLogger.LogInfo($"Log file saved to: {SystemLogger.GetLogFilePath()}");
        SystemLogger.LogInfo($"Total log entries: {File.ReadAllLines(SystemLogger.GetLogFilePath()).Length}");
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