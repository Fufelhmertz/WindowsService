using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TestService.Model;

namespace TestService.Workers.DataBase
{
    /// <summary>
    /// Класс для записи данных в БД
    /// </summary>
    public class SqlProvider : ISqlProvider
    {
        private readonly Settings settings;
        private readonly ILogger logger;
        private readonly IServiceProvider service;
        private readonly SqlConnection connection;

        public SqlProvider(IServiceProvider service, SqlConnection connection)
        {
            this.service = service;
            this.settings = service.GetRequiredService<Settings>();
            this.logger = service.GetRequiredService<ILogger<SqlProvider>>();
            this.connection = connection;
        }

        /// <summary>
        /// Метод добавления записи в БД
        /// </summary>        
        public async Task InsertIntoDB(ConcurrentDictionary<string, int> webText, CancellationToken token)
        {
            var webPageName = string.Empty;
            int wordsCount = 0;

            foreach (var res in webText)
            {
                webPageName = res.Key;
                wordsCount = res.Value;
            }

            var connectParams = settings.DataBase;

            string query = $"INSERT INTO {connectParams["Table"]} ({connectParams["Colum1"]},{connectParams["Colum2"]}) VALUES {webPageName},{wordsCount}";

            
            try
            {
                await Task.Run(() =>
                {  
                   connection.Open();

                   logger.LogInformation($"Connection is open. {DateTimeOffset.Now}");

                   var command = SetupCommand(query, connection);

                   int number = command.ExecuteNonQuery();

                   logger.LogInformation($"INSERT into DB change {number} rows");

                    connection.Close();

                    logger.LogInformation($"Connection closed. {DateTimeOffset.Now}");

                }, token);
            }
            catch (DbException ex)
            {
                logger.LogError($"Connection with parameters: {connection.ConnectionString} failed.{Environment.NewLine} {ex.Message}");
                throw new Exception($"Connection with parameters:  failed.{Environment.NewLine} {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError($"Connection string is empty: {connection.ConnectionString} {ex.Message}");
                throw new Exception($"Connection string is empty:  {ex.Message}");
            }
        }


        private DbCommand SetupCommand(string query, SqlConnection connection, int? timeout = null)
        {
            var command = connection.CreateCommand();
            command.CommandTimeout = Math.Min(300, Math.Max(0, timeout ?? 0));
            command.CommandType = CommandType.Text;
            command.CommandText = query;            

            return command;
        }
    }
}
