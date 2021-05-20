using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SiliconeTrader.Machine.Client.Models
{
    public class SaveSettingsRequest : BotRequest
    {
        public bool BuyDCAEnabled { get; set; }

        public bool BuyEnabled { get; set; }

        public Dictionary<string, string> Configs { get; set; }

        public bool HealthCheckEnabled { get; set; }

        public bool SellEnabled { get; set; }

        public bool TradingSuspended { get; set; }
    }

    public class SettingsViewModel : BotResponse
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