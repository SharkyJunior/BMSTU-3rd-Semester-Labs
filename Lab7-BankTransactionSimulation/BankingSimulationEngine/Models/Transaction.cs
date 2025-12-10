using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace BankingSimulationEngine.Models
{
    public class Transaction
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public bool IsCorporate { get; set; }
        public bool NeedsManualCheck { get; set; }
        public bool IsSuspicious { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public int ProcessingAttempts { get; set; }
        public DateTime? ScreeningStartTime { get; set; }
        public DateTime? ExecutionStartTime { get; set; }
        public DateTime? CompletionTime { get; set; }

    }

    public enum TransactionType
    {
        ClientTransfer,     // Клиентский перевод
        CorporatePayment    // Корпоративный платеж
    }

    public enum TransactionStatus
    {
        Pending,           // Ожидает обработки
        Screening,         // На скрининге
        ManualCheck,       // На ручной проверке
        Executing,         // На исполнении
        Completed,         // Успешно завершена
        Rejected,          // Отклонена
        QueuedForRetry     // В очереди на повторную проверку
    }

}
