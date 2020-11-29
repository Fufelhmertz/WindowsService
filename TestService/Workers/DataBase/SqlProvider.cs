using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
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
        private SqlConnection connection;

        public SqlProvider(IServiceProvider service)
        {
            this.service = service;
            this.settings = service.GetRequiredService<Settings>();
            this.logger = service.GetRequiredService<ILogger<SqlProvider>>();            
        }

        /// <summary>
        /// Метод добавления записи в БД
        /// </summary>        
        public async Task InsertIntoDB(Dictionary<string, int> webText, CancellationToken token, SqlConnection connection)
        {
            this.connection = connection;

            if (!webText.Any())
            {
                logger.LogWarning($"Dictionary is empty");
            }

            var webPageName = webText.Select(x => x.Key).First() ?? null;
            var wordsCount = webText.Select(x => x.Value).ToString() ?? null;            

            var connectParams = settings.DataBase;

            string query = $"INSERT INTO {connectParams["Table"]} ({connectParams["Colum1"]},{connectParams["Colum2"]}) VALUES {webPageName},{wordsCount}";
            
            try
            {
                await Task.Run(() =>
                {  
                   connection.Open();

                   logger.LogInformation($"Connection is open. {DateTimeOffset.Now}");

                   var command = SetupCommand(query);

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


        private DbCommand SetupCommand(string query, int? timeout = null)
        {
            var command = connection.CreateCommand();
            command.CommandTimeout = Math.Min(300, Math.Max(0, timeout ?? 0));
            command.CommandType = CommandType.Text;
            command.CommandText = query;            

            return command;
        }
    }
}
