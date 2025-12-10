using System;
using System.Collections.Generic;
using System.Text;

namespace Lab7_BankTransactionSimulation.Models
{
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
