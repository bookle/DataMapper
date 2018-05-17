using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookLe.DataMapper.Query;

namespace BookLe.DataMapper.Tests.QueryBuilders
{
    class SQLiteQueryBuilder<T> : QueryBuilder<SQLiteConnection, T> where T : class, new()
    {
        public SQLiteQueryBuilder()
        {
            this.SetConnectionString(@"Data Source=Data\chinook.db;Version=3;");
        }
    }
}
