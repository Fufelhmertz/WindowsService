﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace TestService.Workers.DataBase
{
    public interface ISqlProvider
    {
        Task InsertIntoDB(Dictionary<string, int> webText, CancellationToken token, SqlConnection sqlconnection);
    }
}
