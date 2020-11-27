using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestService.Model;
using TestService.Workers.DataBase;
using TestService.Workers.WebParser;

namespace TestService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private WebClient client;
        private SqlConnection connection;
        private readonly Settings settings;        
        private readonly IServiceProvider service;        
        private readonly WebTextParser webTextParser;
        private readonly SqlProvider sqlprovider;
        private ConcurrentDictionary<string, int> webResult;


        public Worker(ILogger<Worker> logger, IServiceProvider service)
        {
            _logger = logger;
            this.service = service;
            settings = service.GetRequiredService<Settings>();            
            webTextParser = new WebTextParser(service);
            sqlprovider = new SqlProvider(service, connection);
            webResult = new ConcurrentDictionary<string, int>();
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            client = new WebClient();
            connection = new SqlConnection(GetConnectionString());

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var result = await webTextParser.CountWordsFromHtmlText(stoppingToken, client);

                await sqlprovider.InsertIntoDB(result, stoppingToken);

                await Task.Delay(settings.Timeout, stoppingToken);

                _logger.LogInformation("Worker finished at: {time}", DateTimeOffset.Now);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            client.Dispose();
            return base.StopAsync(cancellationToken);
        }

        private string GetConnectionString()
        {
            var connectParams = settings.DataBase;

            var connectionString = new SqlConnectionStringBuilder()
            {
                DataSource = connectParams["Source"],
                InitialCatalog = connectParams["Database"],
                UserID = connectParams["Login"],
                Password = connectParams["Password"]
            };


            return connectionString.ToString();
        }
    }
}
