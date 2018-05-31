using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Text;
using BookLe.DataMapper.Query;

namespace BookLe.DataMapper.Tests.QueryBuilders
{
    public class SqlQueryBuilder<T> : QueryBuilder<SqlConnection, T> where T : class, new()
    {
        public SqlQueryBuilder()
        {
            this.SetConnectionString(@"Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;");
        }
    }
}
