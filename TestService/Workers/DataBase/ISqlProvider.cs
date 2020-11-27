using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TestService.Workers.DataBase
{
    public interface ISqlProvider
    {
        Task InsertIntoDB(ConcurrentDictionary<string, int> webText, CancellationToken token);
    }
}
