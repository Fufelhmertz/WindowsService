using System;
using System.Data.SqlClient;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestService.Helper;
using TestService.Model;
using TestService.Workers.DataBase;
using TestService.Workers.WebParser;

namespace TestService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private WebClient client;
        private SqlConnection connection;
        private WebTextParser webTextParser;
        private SqlProvider sqlprovider;
        private ConnectionString connectionString;
        private readonly Settings settings;       
        private readonly IServiceProvider service;        
       

        public Worker(ILogger<Worker> logger, IServiceProvider service)
        {
            this.logger = logger;
            this.service = service;
            settings = service.GetRequiredService<Settings>();  
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            client = new WebClient();
            webTextParser = new WebTextParser(service);
            connectionString = new ConnectionString(service);
            connection = new SqlConnection(connectionString.GetConnectionString());
            sqlprovider = new SqlProvider(service);

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var result = await webTextParser.CountingWordsOnPage(stoppingToken, client);

                await sqlprovider.InsertIntoDB(result, stoppingToken, connection);

                await Task.Delay(settings.Timeout, stoppingToken);

                logger.LogInformation("Worker finished at: {time}", DateTimeOffset.Now);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            client.Dispose();
            connection.Dispose();

            logger.LogInformation("Worker stoped at: {time}", DateTimeOffset.Now);
            return base.StopAsync(cancellationToken);
        }
      
    }
}
