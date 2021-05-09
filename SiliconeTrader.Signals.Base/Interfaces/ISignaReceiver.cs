using SiliconeTrader.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Signals.Base
{
    public interface ISignalReceiver
    {
        string SignalName { get; }
        void Start();
        void Stop();
        int GetPeriod();
        IEnumerable<ISignal> GetSignals();
        double? GetAverageRating();
    }
}
