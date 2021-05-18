using System.Threading;
using System.Threading.Tasks;

namespace SiliconeTrader.Machine.Client.Core.Abstractions
{
    public interface IAccountManager
    {
        Task Refresh(CancellationToken cancellationToken);
    }

}