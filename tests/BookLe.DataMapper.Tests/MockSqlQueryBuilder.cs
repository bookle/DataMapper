using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using BookLe.DataMapper.Query;
using BookLe.DataMapper.Tests.QueryBuilders;

namespace BookLe.DataMapper.Tests
{
    class MockSqlQueryBuilder<T> : SqlQueryBuilder<T> where T : class, new()
    {
        public QueryBuilderInternalProps<T> InternalProps { get; private set; }

        /// <summary>
        /// Overrides GetResult so no real database statements are executed.
        /// </summary>
        /// <returns></returns>
        public override QueryResult<T> GetResult()
        {
            InternalProps = new QueryBuilderInternalProps<T>
            {
                Command = _command,
                ConnString = _connString,
                IgnoreDataColumnNotFound = _ignoreDataColumnNotFound,
                DataAdapter = _dataAdapter,
                PropertyMappings = _propertyMappings,
                DataRowMapper = _dataRowMapper
            };

            return new QueryResult<T>
            {
                List = new System.Collections.Generic.List<T>()
            };
        }
    }

    class QueryBuilderInternalProps<T> where T : class, new()
    {
        public IDbCommand Command;
        public string ConnString;
        public bool IgnoreDataColumnNotFound;
        public IDbDataAdapter DataAdapter;
        public List<PropertyMapping> PropertyMappings = new List<PropertyMapping>();
        public Func<DataValueList, T> DataRowMapper;
    }
}
