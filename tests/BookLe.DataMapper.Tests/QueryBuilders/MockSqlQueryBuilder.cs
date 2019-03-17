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
        public QueryBuilderInternalProps<T> InternalProps
        {
            get
            {
                var internalProps = GetInternalProps();
                return new QueryBuilderInternalProps<T>
                {
                    CommandType = internalProps.CommandType,
                    CommandText = internalProps.CommandText,
                    CommandTimeout = internalProps.CommandTimeout,
                    ConnString = internalProps.ConnString,
                    IgnoreDataColumnNotFound = internalProps.IgnoreColumnNotFound,
                    Parameters = internalProps.Parameters,
                    PropertyMappings = internalProps.propertyMappings,
                    DataRowMapper = internalProps.DataRowMapper
                };
            }
        }

        /// <summary>
        /// Overrides GetResult so no real database statements are executed.
        /// </summary>
        /// <returns></returns>
        public override QueryResult<T> GetResult()
        {
            return new QueryResult<T>
            {
                List = new System.Collections.Generic.List<T>()
            };
        }
    }

    class QueryBuilderInternalProps<T> where T : class, new()
    {
        public CommandType CommandType;
        public string CommandText;
        public int CommandTimeout = 500;
        public string ConnString;
        public bool IgnoreDataColumnNotFound = true;
        public List<QueryParameter> Parameters;
        public List<PropertyMapping> PropertyMappings = new List<PropertyMapping>();
        public Func<DataValueList, T> DataRowMapper;
    }
}
