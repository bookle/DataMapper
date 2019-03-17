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
    
        private CommandType _commandType;
        private string _commandText;
        private int _commandTimeout = 500;
        private List<QueryParameter> _parameters = new List<QueryParameter>();
        private string _connString;
        private bool _ignoreDataColumnNotFound = true;
        private List<PropertyMapping> _propertyMappings = new List<PropertyMapping>();
        private Func<DataValueList, T> _dataRowMapper;
        private Func<IDbConnection, IDbCommand> _createCommand = null;
        private Action<IDbConnection> _openConnection = null;

        public QueryBuilder()
        {
            _createCommand = conn => conn.CreateCommand();
            _openConnection = conn => conn.Open();
        }

        /// <summary>
        /// Mocks will use this constructor for unit testing
        /// </summary>
        /// <param name="createCommand"></param>
        /// <param name="openConnection"></param>
        public QueryBuilder(Func<IDbConnection, IDbCommand> createCommand, Action<IDbConnection> openConnection)
        {
            _createCommand = createCommand;
            _openConnection = openConnection;
        }

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
            _commandType = CommandType.StoredProcedure;
            _commandText = storedProcedureName;
            return this;
        }

        /// <summary>
        /// Sql Select statement
        /// </summary>
        public QueryBuilder<TDb, T> SetSql(string sql)
        {
            _commandType = CommandType.Text;
            _commandText = sql;
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
            _commandTimeout = seconds;
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
        public QueryBuilder<TDb, T> AddParameter(string name,
                                                 object value)
        {
            var param = new QueryParameter
            {
                Name = name,
                Value = value
            };
            _parameters.Add(param);
            return this;
        }

        public QueryBuilder<TDb, T> AddParameter(Func<bool> condition, string name, object value)
        {
            if (!condition()) return this;
            return AddParameter(name, value);
        }

        public QueryBuilder<TDb, T> AddParameter(QueryParameter queryParam)
        {
            _parameters.Add(queryParam);
            return this;
        }

        public QueryBuilder<TDb, T> AddParameter(Func<bool> condition, QueryParameter queryParam)
        {
            if (!condition()) return this;
            return AddParameter(queryParam);
        }

        /// <summary>
        /// Adds a parameter for the stored procedure or the dynamic sql only if condition is true.
        /// </summary>
        /// <summary>
        /// Adds a parameter for the stored procedure or the dynamic sql with a specified direction (e.g. Input, Output)
        /// </summary>
        public QueryBuilder<TDb, T> AddParameter(string name, object value, QueryParameterDirectionEnum direction)
        {
            var param = new QueryParameter();
            param.Name = name;
            param.Value = value;
            param.Direction = direction;
            _parameters.Add(param);
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
                _openConnection(conn);
                return GetResult(conn);
            }

        }

        public virtual QueryResult<T> GetResult(IDbConnection conn)
        {
            return _getResult(conn);
        }
        /// <summary>
        /// Return the list and parameter values after executing the stored procedure or sql.
        /// </summary>

        /// <summary>
        /// Return the list and parameter values after executing the stored procedure or sql.
        /// </summary>
        public virtual QueryResult<T> GetResult(IDbTransaction trans)
        {
            return _getResult(trans.Connection, trans);
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

        private QueryResult<T> _getResult(IDbConnection conn, IDbTransaction trans = null)
        {

            if (conn?.State != ConnectionState.Open)
            {
                _openConnection(conn);
            }

            using (var command = _createCommand(conn))
            {
                command.CommandType = _commandType;
                command.CommandText = _commandText;
                command.CommandTimeout = _commandTimeout;
                if (trans != null) command.Transaction = trans;
                foreach (var queryParam in _parameters)
                {
                    var param = command.CreateParameter();
                    param.ParameterName = queryParam.Name;
                    param.Value = queryParam.Value;
                    if (queryParam.DataType.HasValue) param.DbType = DataTypeHelper.MapDataType(queryParam.DataType.Value);
                    if (queryParam.Size.HasValue) param.Size = queryParam.Size.Value;
                    if (queryParam.Scale.HasValue) param.Scale = queryParam.Scale.Value;
                    if (queryParam.Precision.HasValue) param.Precision = queryParam.Precision.Value;
                    if (queryParam.Direction.HasValue) param.Direction = DataTypeHelper.MapDirection(queryParam.Direction.Value);
                    command.Parameters.Add(param);
                }

                List<T> list;

                using (var dr = command.ExecuteReader(CommandBehavior.Default))
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
                    Parameters = QueryMapperUtility.GetParameters(command)
                };

                return result;

            }
        }

        protected (
                    CommandType CommandType, 
                    string CommandText, 
                    int CommandTimeout, 
                    string ConnString, 
                    bool IgnoreColumnNotFound,
                    List<QueryParameter> Parameters,
                    List<PropertyMapping> propertyMappings,
                    Func<DataValueList, T> DataRowMapper,
                    Func<IDbConnection, IDbCommand> CreateCommand,
                    Action<IDbConnection> OpenConnection
                  ) GetInternalProps()
        {
            return (
                     _commandType,
                     _commandText,
                     _commandTimeout,
                     _connString,
                     _ignoreDataColumnNotFound,
                     _parameters,
                     _propertyMappings,
                     _dataRowMapper,
                     _createCommand,
                     _openConnection
                   );
        }

    }


}
