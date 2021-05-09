using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Core
{
    internal class NotificationConfig : INotificationConfig
    {
        public bool Enabled { get; set; } = false;
        public bool TelegramEnabled { get; set; }
        public string TelegramBotToken { get; set; }
        public long TelegramChatId { get; set; }
        public bool TelegramAlertsEnabled { get; set; }
    }
}
