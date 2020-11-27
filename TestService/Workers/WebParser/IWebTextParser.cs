using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TestService.Workers.WebParser
{
    public interface IWebTextParser
    {
        ConcurrentDictionary<string, string> GetTextFromWebPages(WebClient client);
        string LoadWebPage(string page, WebClient client);
        Task<ConcurrentDictionary<string, int>> CountWordsFromHtmlText(CancellationToken token, WebClient client);
    }
}
