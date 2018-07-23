using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BookLe.DataMapper.Tests.QueryBuilders
{
    public class DataTableBuilder<T> where T : class, new()
    {
        private List<T> list;
        private Dictionary<string, string> mappings = new Dictionary<string, string>();

        public DataTableBuilder(List<T> list)
        {
            this.list = list;
        }

        public DataTableBuilder<T> ColumnFor<X>(Expression<Func<T, X>> property, string columnName)
        {
            var memberExp = (MemberExpression)property.Body;
            var propInfo = (PropertyInfo)memberExp.Member;
            mappings.Add(propInfo.Name, columnName);
            return this;
        }

        public DataTable Build()
        {
            var propInfos = (typeof(T)).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var dict = new Dictionary<string, PropertyInfo>();

            var dt = new DataTable();

            foreach (var prop in propInfos)
            {
                var columnName = prop.Name;
                if (mappings.TryGetValue(prop.Name, out var name))
                {
                    columnName = name;
                }

                var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
                if (underlyingType != null)
                {
                    // this is a nullable type, make it a nullable column
                    var column = new DataColumn(prop.Name, underlyingType);
                    column.AllowDBNull = true;
                    dt.Columns.Add(column);
                }
                else
                {
                    // non-nullable type
                    dt.Columns.Add(columnName, prop.PropertyType);
                }
                dict.Add(columnName, prop);
            }

            foreach (var item in list)
            {
                var row = dt.NewRow();
                foreach (var kv in dict)
                {
                    var value = kv.Value.GetValue(item);
                    if (value == null)
                    {
                        row[kv.Key] = DBNull.Value;
                    }
                    else
                    {
                        row[kv.Key] = value;
                    }
                }
                dt.Rows.Add(row);
            }

            return dt;
        }


    }
}
