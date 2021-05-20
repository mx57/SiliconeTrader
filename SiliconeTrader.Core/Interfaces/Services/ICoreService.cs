using System;
using System.Collections.Concurrent;

namespace SiliconeTrader.Core
{
    public interface ICoreService : IConfigurableService
    {
        event Action Started;
        ICoreConfig Config { get; }
        string Version { get; }
        void Start();
        void Stop();
        void Restart();
    }
}
