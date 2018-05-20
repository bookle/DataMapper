using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace BookLe.DataMapper.Query
{
    /// <summary>
    /// Builder to specify sql and mapping defintions. Call GetResult to execute the query and produce a list.
    /// </summary>
    /// <typeparam name="T">
    /// The type of object that will bind to the sql rows.
    /// </typeparam>
    /// <typeparam name="TDb">
    /// The type of database connection (e.g. SqlConnection)
    /// </typeparam>
    public class QueryBuilder<TDb, T>
        where T : class, new()
        where TDb : class, IDbConnection, new()
    {

        protected IDbCommand _command;
        protected string _connString;
        protected bool _ignoreDataColumnNotFound = true;
        protected IDbDataAdapter _dataAdapter;
        protected List<PropertyMapping> _propertyMappings = new List<PropertyMapping>();

        /// <summary>
        /// Connection string to the database.
        /// </summary>
        public QueryBuilder<TDb, T> SetConnectionString(string connString)
        {
            _connString = connString;
            return this;
        }

        /// <summary>
        /// Stored Procedure Name.
        /// </summary>
        public QueryBuilder<TDb, T> SetStoredProcedure(string storedProcedureName)
        {
            Command.CommandType = CommandType.StoredProcedure;
            Command.CommandText = storedProcedureName;
            return this;
        }

        /// <summary>
        /// Sql Select statement
        /// </summary>
        public QueryBuilder<TDb, T> SetSql(string sql)
        {
            Command.CommandType = CommandType.Text;
            Command.CommandText = sql;
            return this;
        }

        /// <summary>
        /// Indicate whether to ignore a DataColumn that does not exist or throw an exception.
        /// The default is ignore.
        /// </summary>
        /// <param name="ignore">Setting to false will throw exception when DataColumn does not exist.</param>
        /// <returns></returns>        
        public QueryBuilder<TDb, T> SetIgnoreDataColumnNotFound(bool ignore)
        {
            _ignoreDataColumnNotFound = ignore;
            return this;
        }

        /// <summary>
        /// Sets the command timeout
        /// </summary>
        /// <param name="seconds">number of seconds the command is allowed to execute</param>
        public QueryBuilder<TDb, T> SetCommandTimeout(int seconds)
        {
            Command.CommandTimeout = seconds;
            return this;
        }

        /// <summary>
        /// Map a property to a column in the result set. The property name will be mapped to the column name.
        /// </summary>
        public QueryBuilder<TDb, T> MapProperty<X>(Expression<Func<T, X>> property, string columnName)
        {
            var memberExp = (MemberExpression)property.Body;
            var propInfo = (PropertyInfo)memberExp.Member;
            return _mapProperty(propInfo.Name, columnName);
        }

        /// <summary>
        /// Map a property to a value computed using the row of the result set.
        /// </summary>
        public QueryBuilder<TDb, T> MapProperty<X>(Expression<Func<T, X>> property, Func<DataValueList, X> mapperFunction)
        {
            var memberExp = (MemberExpression)property.Body;
            var propInfo = (PropertyInfo)memberExp.Member;
            Func<DataValueList, object> mapperFunctionToObject = (row) => mapperFunction(row);
            return _mapProperty(propInfo.Name, mapperFunctionToObject);
        }

        private QueryBuilder<TDb, T> _mapProperty(string propertyName, Func<DataValueList, object> mapperFunction)
        {
            _propertyMappings.Add(new PropertyMapping
            {
                PropertyName = propertyName,
                PropertyMapperDataRow = mapperFunction
            });
            return this;
        }

        private QueryBuilder<TDb, T> _mapProperty(string propertyName, string columnName)
        {
            _propertyMappings.Add(new PropertyMapping { PropertyName = propertyName, ColumnName = columnName });
            return this;
        }

        protected Func<DataValueList, T> _dataRowMapper;

        /// <summary>
        /// Maps a database row to an object. This can boost performance since the
        /// mapperFunction implementation can be hand coded to populate the object.
        /// </summary>
        public QueryBuilder<TDb, T> MapObject(Func<DataValueList, T> mapperFunction)
        {
            _dataRowMapper = mapperFunction;
            return this;
        }

        /// <summary>
        /// Adds a parameter for the stored procedure or the dynamic sql.
        /// </summary>
        public QueryBuilder<TDb, T> AddParameter(string name, object value)
        {
            var param = Command.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            Command.Parameters.Add(param);
            return this;
        }

        /// <summary>
        /// Adds a parameter for the stored procedure or the dynamic sql only if condition is true.
        /// </summary>
        public QueryBuilder<TDb, T> AddParameter(Func<bool> condition, string name, object value)
        {
            if (!condition()) return this;
            var param = Command.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            Command.Parameters.Add(param);
            return this;
        }

        /// <summary>
        /// Adds a parameter for the stored procedure or the dynamic sql with a specified direction (e.g. Input, Output)
        /// </summary>
        public QueryBuilder<TDb, T> AddParameter(string name, object value, QueryParameterDirectionEnum direction)
        {
            var param = Command.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            param.Direction = ParameterDirection.Input;
            if (direction == QueryParameterDirectionEnum.Output)
                param.Direction = ParameterDirection.Output;
            Command.Parameters.Add(param);
            return this;
        }

        /// <summary>
        /// Return the list and parameter values after executing the stored procedure or sql.
        /// </summary>
        public virtual QueryResult<T> GetResult()
        {

            using (var conn = new TDb())
            {
                conn.ConnectionString = _connString;
                Command.Connection = conn;
                conn.Open();

                return GetResult(conn);
            }

        }

        /// <summary>
        /// Return the list and parameter values after executing the stored procedure or sql.
        /// </summary>
        public virtual QueryResult<T> GetResult(IDbConnection conn)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            Command.Connection = conn;

            List<T> list;
            
            using (var dr = Command.ExecuteReader(CommandBehavior.Default))
            {
                if (_dataRowMapper != null)
                {
                    list = QueryMapperUtility.GetList<T>(dr, _dataRowMapper).ToList();
                }
                else
                {
                    list = QueryMapperUtility.GetList<T>(dr, _propertyMappings, _ignoreDataColumnNotFound).ToList();
                }
                dr.Close();
            }

            var result = new QueryResult<T>
            {
                List = list, 
                Parameters = QueryMapperUtility.GetParameters(Command)
            };

            return result;


        }

        /// <summary>
        /// Return the list and parameter values after executing the stored procedure or sql.
        /// </summary>
        public virtual QueryResult<T> GetResult(IDbTransaction trans)
        {
            Command.Transaction = trans;
            return GetResult(Command.Transaction.Connection);
        }

        /// <summary>
        /// Returns a new SqlConnection object. Does not open the connection.
        /// </summary>
        public IDbConnection GetUnOpenedConnection()
        {
            var conn = new TDb();
            conn.ConnectionString = _connString;
            return conn;
        }

        /// <summary>
        /// The ADO.NET Command object
        /// </summary>
        protected IDbCommand Command
        {
            get
            {
                var conn = GetUnOpenedConnection();
                if (_command == null)
                {
                    _command = conn.CreateCommand();
                    _command.CommandTimeout = 500;
                }
                return _command;
            }
        }

    }

}
