using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSimulationEngine.Models
{
    public class QueueMetrics
    {
        public int CurrentLength { get; set; }
        public int MaxLength { get; set; }
        public double AvgWaitTimeSeconds { get; set; }
        public long TotalProcessed { get; set; }
        public long TotalRejected { get; set; }
    }
}
