using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BookLe.DataMapper.Query
{
    public class QueryMapperUtility
    {
        /// <summary>
        /// Converts a DataReader to a strongly typed IEnumerable. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="mapperFunction">Function that returns an instance of T passing in the current row.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetList<T>(IDataReader dr, Func<DataValueList, T> mapperFunction) where T : class, new()
        {
            while (dr.Read())
            {
                var rowValues = new DataValueList();
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    object value = dr.IsDBNull(i) ? null : dr[i];
                    rowValues.Add(new DataValue { ColumnName = dr.GetName(i), Value = value });
                }
                var item = mapperFunction(rowValues);
                yield return item;
            }

        }

        public static async Task<List<T>> GetListAsync<T>(SqlDataReader dr, Func<DataValueList, T> mapperFunction) where T : class, new()
        {
            var list = new List<T>();
            while (await dr.ReadAsync())
            {
                var rowValues = new DataValueList();
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    object value = dr.IsDBNull(i) ? null : dr[i];
                    rowValues.Add(new DataValue { ColumnName = dr.GetName(i), Value = value });
                }
                var item = mapperFunction(rowValues);
                list.Add(item);
            }
            return list;
        }


        /// <summary>
        /// Converts a IDataReader to a strongly typed IList. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetList<T>(IDataReader dr, IList<PropertyMapping> propertyMappings, bool ignoreDataColumnNotFound) where T : class, new()
        {

            //Define a mapping for each property
            var allMappings = _getAllMappings<T>(dr, propertyMappings, ignoreDataColumnNotFound);

            while (dr.Read())
            {
                var item = _getItem<T>(dr, allMappings);
                yield return item;
            }

        }

        /// <summary>
        /// Converts a IDataReader to a strongly typed IList. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<List<T>> GetListAsync<T>(SqlDataReader dr, IList<PropertyMapping> propertyMappings, bool ignoreDataColumnNotFound) where T : class, new()
        {

            //Define a mapping for each property
            var allMappings = _getAllMappings<T>(dr, propertyMappings, ignoreDataColumnNotFound);
            var list = new List<T>();
            while (await dr.ReadAsync())
            {
                var item = _getItem<T>(dr, allMappings);
                list.Add(item);
            }
            return list;
        }

        private static List<PropertyMapping> _getAllMappings<T>(IDataReader dr, IList<PropertyMapping> propertyMappings, bool ignoreDataColumnNotFound)
        {
            var props = (typeof(T)).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            //Define a mapping for each property
            var allMappings = new List<PropertyMapping>();
            foreach (PropertyInfo prop in props)
            {
                var propertyName = prop.Name;
                var mapping = propertyMappings.FirstOrDefault(p => p.PropertyName == propertyName);
                if (mapping == null)
                {
                    //generate a new default mapping.                  
                    mapping = new PropertyMapping { ColumnName = propertyName, PropertyName = propertyName };
                }
                //set default index to -1 which indicates data column does not exist.
                mapping.OrdinalIndex = -1;
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    //Find column (case-insensitive)
                    if (string.Compare(mapping.ColumnName, dr.GetName(i), true) == 0)
                    {
                        mapping.OrdinalIndex = i;
                        break;
                    }
                }
                //Check if DataColumn was found
                if (mapping.OrdinalIndex == -1)
                {
                    if (!ignoreDataColumnNotFound && mapping.PropertyMapperDataRow == null)
                    {
                        throw new DataColumnNotFoundException(string.Format("Column '{0}' was not found.", mapping.ColumnName));
                    }
                }
                mapping.Property = prop;
                allMappings.Add(mapping);
            }
            return allMappings;
        }

        private static T _getItem<T>(IDataReader dr, List<PropertyMapping> allMappings) where T : new()
        {
            var item = new T();

            foreach (PropertyMapping mapping in allMappings)
            {
                var prop = mapping.Property;
                if (prop == null) continue;

                if (mapping.PropertyMapperDataRow != null)
                {
                    var rowValues = new DataValueList();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        object value = dr.IsDBNull(i) ? null : dr[i];
                        rowValues.Add(new DataValue { ColumnName = dr.GetName(i), Value = value });
                    }
                    var mapperResult = mapping.PropertyMapperDataRow(rowValues);
                    prop.SetValue(item, mapperResult, null);
                    continue;
                }

                //skip if there is no data column
                if (mapping.OrdinalIndex == -1) continue;

                if (dr.IsDBNull(mapping.OrdinalIndex))
                {
                    prop.SetValue(item, null, null);
                    continue;
                }

                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (type.GetTypeInfo().IsEnum)
                {
                    var value = Enum.ToObject(type, dr[mapping.OrdinalIndex]);
                    prop.SetValue(item, value, null);
                }
                else
                {
                    var value = Convert.ChangeType(dr[mapping.OrdinalIndex], type);
                    prop.SetValue(item, value, null);
                }

            }

            return item;
        }

        public static List<QueryParameter> GetParameters(IDbCommand command)
        {
            var list = new List<QueryParameter>();
            foreach (IDataParameter param in command.Parameters)
            {
                var dbParam = new QueryParameter { Name = param.ParameterName, Value = param.Value };
                switch (param.Direction)
                {
                    case ParameterDirection.Input:
                        dbParam.Direction = QueryParameterDirectionEnum.Input;
                        break;
                    case ParameterDirection.Output:
                        dbParam.Direction = QueryParameterDirectionEnum.Output;
                        break;
                    case ParameterDirection.InputOutput:
                        dbParam.Direction = QueryParameterDirectionEnum.Output;
                        break;
                    default:
                        dbParam.Direction = QueryParameterDirectionEnum.Input;
                        break;
                }
                list.Add(dbParam);
            }
            return list;
        }

    }

}
