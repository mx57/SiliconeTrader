using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SiliconeTrader.Machine.Client.Models
{
    public class SettingsViewModel : DefaultViewModel
    {
        [Display(Name = "Buy DCA Enabled")]
        public bool BuyDCAEnabled { get; set; }

        [Display(Name = "Buy Enabled")]
        public bool BuyEnabled { get; set; }

        public Dictionary<string, string> Configs { get; set; }

        [Display(Name = "Health Check Enabled")]
        public bool HealthCheckEnabled { get; set; }

        [Display(Name = "Sell Enabled")]
        public bool SellEnabled { get; set; }

        [Display(Name = "Trading Suspended")]
        public bool TradingSuspended { get; set; }
    }
}