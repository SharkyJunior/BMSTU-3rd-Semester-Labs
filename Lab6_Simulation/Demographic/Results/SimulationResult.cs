using System;
using System.Collections.Generic;
using System.Text;

namespace Demographic.Results
{
    public class SimulationResult
    {
        public List<(int Year, double Total, double Male, double Female)> YearlyData { get; } = new();
        public Dictionary<string, (double Male, double Female)> FinalAgeGroups { get; } = new();
    }
}
