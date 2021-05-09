using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Core
{
    public interface INamedService
    {
        string ServiceName { get; }
    }
}
