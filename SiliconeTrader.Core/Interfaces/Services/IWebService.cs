using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Core
{
    public interface IWebService
    {
        IWebConfig Config { get; }
        void Start();
        void Stop();
    }
}
