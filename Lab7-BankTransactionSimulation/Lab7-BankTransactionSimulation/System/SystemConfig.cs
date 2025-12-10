using System;
using System.Collections.Generic;
using System.Text;

namespace Lab7_BankTransactionSimulation.System
{
    public class SystemConfig
    {
        // Эмиттеры
        public double ClientTransactionsPerSecond { get; set; } = 5; // 5 RPS
        public int CorporateBatchMinIntervalSeconds { get; set; } = 5;
        public int CorporateBatchMaxIntervalSeconds { get; set; } = 10;
        public int CorporateBatchMinSize { get; set; } = 10;
        public int CorporateBatchMaxSize { get; set; } = 100;

        // Очереди
        public int PriorityQueueCapacity { get; set; } = 100;
        public int NormalQueueCapacity { get; set; } = int.MaxValue; // Не ограничена

        // Обработчики
        public double AutoScreeningRPS { get; set; } = 3; // 3 транзакции в секунду
        public double ManualScreeningRPS { get; set; } = 0.1; // 0.1 транзакции в секунду
        public double ExecutionMinRPS { get; set; } = 1;
        public double ExecutionMaxRPS { get; set; } = 2;

        // Вероятности
        public double AutoScreenPassRate { get; set; } = 0.85;
        public double AutoScreenRetryRate { get; set; } = 0.10;
        public double AutoScreenRejectRate { get; set; } = 0.05;
        public double ManualScreenPassRate { get; set; } = 0.70;
        public double ManualScreenRejectRate { get; set; } = 0.30;

        // Общие настройки
        public TimeSpan SimulationDuration { get; set; } = TimeSpan.FromMinutes(10);
        public int MaxRetryAttempts { get; set; } = 3;
    }
}
