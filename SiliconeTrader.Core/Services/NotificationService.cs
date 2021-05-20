using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SiliconeTrader.Core
{
    internal class NotificationService : ConfigrableServiceBase<NotificationConfig>, INotificationService
    {
        public override string ServiceName => Constants.ServiceNames.NotificationService;

        INotificationConfig INotificationService.Config => this.Config;

        private readonly ILoggingService loggingService;

        // Telegram
        private TelegramBotClient telegramBotClient;
        private ChatId telegramChatId;

        public NotificationService(ILoggingService loggingService)
        {
            this.loggingService = loggingService;
        }

        public void Start()
        {
            try
            {
                loggingService.Info("Start Notification service...");
                if (this.Config.TelegramEnabled)
                {
                    telegramBotClient = new TelegramBotClient(this.Config.TelegramBotToken);
                    User me = telegramBotClient.GetMeAsync().Result;
                    telegramChatId = new ChatId(this.Config.TelegramChatId);
                }
                loggingService.Info("Notification service started");
            }
            catch (Exception ex)
            {
                loggingService.Error("Unable to start Notification service", ex);
                this.Config.Enabled = false;
            }
        }

        public void Stop()
        {
            loggingService.Info("Stop Notification service...");
            if (this.Config.TelegramEnabled)
            {
                telegramBotClient = null;
            }
            loggingService.Info("Notification service stopped");
        }

        public void Notify(string message)
        {
            if (this.Config.Enabled)
            {
                if (this.Config.TelegramEnabled)
                {
                    try
                    {
                        string instanceName = Application.Resolve<ICoreService>().Config.InstanceName;
                        telegramBotClient.SendTextMessageAsync(telegramChatId, $"({instanceName}) {message}", ParseMode.Default, false, !this.Config.TelegramAlertsEnabled);
                    }
                    catch (Exception ex)
                    {
                        loggingService.Error("Unable to send Telegram message", ex);
                    }
                }
            }
        }

        protected override void OnConfigReloaded()
        {
            this.Stop();
            this.Start();
        }
    }
}
