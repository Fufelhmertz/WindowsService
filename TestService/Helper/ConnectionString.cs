using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using TestService.Model;

namespace TestService.Helper
{
    public class ConnectionString
    {
        private readonly Settings settings;
        private readonly IServiceProvider service;
        public ConnectionString(IServiceProvider service)
        {
            this.service = service;
            settings = service.GetRequiredService<Settings>();
        }

        /// <summary>
        /// Метод для формирования ConnectionString
        /// </summary>       
        public string GetConnectionString()
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
