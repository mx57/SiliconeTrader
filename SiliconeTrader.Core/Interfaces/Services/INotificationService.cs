using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Core
{
    public interface INotificationService
    {
        INotificationConfig Config { get; }
        void Start();
        void Stop();
        void Notify(string message);
    }
}
