using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using mshtml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TestService.Model;

namespace TestService.Workers.WebParser
{
    public class WebTextParser : IWebTextParser
    {
        private readonly Settings settings;
        private readonly ILogger logger;
        private readonly IServiceProvider service;
        private WebClient client;

        public WebTextParser(IServiceProvider service)
        {           
            this.service = service;
            this.settings = service.GetRequiredService<Settings>();
            this.logger = service.GetRequiredService<ILogger<WebTextParser>>();

        }

        /// <summary>
        /// Метод для преобразования HTML страницы в string
        /// </summary>  
        public Dictionary<string, string> GetTextFromWebPages()
        {
            var webpage = settings.Websites;
            var webPageDictionary = new Dictionary<string, string>();
            var text = new Dictionary<string, string>();
            var htmldoc = (IHTMLDocument2)new HTMLDocument();

            foreach (var page in webpage)
            {
                var result = LoadWebPage(page.Value);

                if (result.Any())
                {
                    webPageDictionary.TryAdd(page.Key, result);
                }
                else
                {
                    logger.LogWarning($"Loaded page is empty!");
                }
            }

            try
            {
                foreach (var doc in webPageDictionary)
                {
                    htmldoc.write(doc.Value.ToString());
                    text.TryAdd(doc.Key, htmldoc.body.outerText);
                }

                return text;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Convert current web page fails {ex}");
                throw new ArgumentException($"Convert current web page fails{ex}");
            }
        }

        /// <summary>
        /// Метод для загрузки страницы из конфига
        /// </summary>  
        public string LoadWebPage(string url)
        {
            string result = string.Empty;

            if (!url.Any())
            {
                logger.LogError($"Url has incorrect value:{url}");
                throw new ArgumentException($"Url has incorrect value:{url}");
            }

            try
            {   
                if(client==null)
                {
                    client = new WebClient();
                }

                result = client.DownloadString(url);               

                return result;
            }
            catch (WebException ex)
            {
                logger.LogError($"Download website{url} failed: {ex}");
                throw new Exception($"Download website{url} failed: {ex}");
            }

        }

        /// <summary>
        /// Метод для получения текста с веб стравницы
        /// </summary>      
        public Task<Dictionary<string, int>> CountingWordsOnPage(CancellationToken token, WebClient client)
        {
            this.client = client;
            string pattern = "[a-zA-Zа-яА-ЯёЁ]+";

            Dictionary<string, string> text = GetTextFromWebPages();

            return Task.Run(() =>
            {

                Dictionary<string, int> dicWithCountingWords = new Dictionary<string, int>();

                foreach (var doc in text)
                {
                    logger.LogInformation($"Site {doc.Key} has {Regex.Matches(doc.Value, pattern).Count} words");
                    dicWithCountingWords.TryAdd(doc.Key, Regex.Matches(doc.Value, pattern).Count);
                }

                return dicWithCountingWords;

            }, token);

        }
    }
}
