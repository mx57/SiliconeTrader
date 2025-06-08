using System.Collections.Generic;
using SiliconeTrader.Core.Models;

namespace SiliconeTrader.Machine.Client.Models
{
    public class RulesViewModel : BotResponse, IInstanceVersion
    {
        public Dictionary<string, SignalRuleStats> SignalRuleStats { get; set; }
    }

    public class SignalRuleStats
    {
        public List<double> Age { get; set; } = new List<double>();

        public List<int> DCA { get; set; } = new List<int>();

        public List<decimal> Margin { get; set; } = new List<decimal>();

        public List<decimal> MarginDCA { get; set; } = new List<decimal>();

        public decimal TotalCost { get; set; }

        public decimal TotalFees { get; set; }

        public int TotalOrders { get; set; }

        public decimal TotalProfit { get; set; }

        public int TotalSwaps { get; set; }

        public int TotalTrades { get; set; }
    }
}