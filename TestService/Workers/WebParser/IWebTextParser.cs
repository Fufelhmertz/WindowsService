using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TestService.Workers.WebParser
{
    public interface IWebTextParser
    {
        Dictionary<string, string> GetTextFromWebPages();
        string LoadWebPage(string page);
        Task<Dictionary<string, int>> CountingWordsOnPage(CancellationToken token);
    }
}

