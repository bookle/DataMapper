using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BookLe.DataMapper.Query
{
    /// <summary>
    /// List of values from a row in the result. 
    /// </summary>
    public class DataValueList : List<DataValue>
    {
        public bool? GetBoolean(string columnName)
        {
            var dataValue = this.FirstOrDefault(x => string.Compare(x.ColumnName, columnName, true) == 0);
            if (dataValue != null)
            {
                if (dataValue.Value == null) return null;
                return Convert.ToBoolean(dataValue.Value);
            }
            return null;
        }

        public int? GetInteger(string columnName)
        {
            var dataValue = this.FirstOrDefault(x => string.Compare(x.ColumnName, columnName, true) == 0);
            if (dataValue != null)
            {
                return Convert.ToInt32(dataValue.Value);
            }
            return null;
        }

        public long? GetLong(string columnName)
        {
            var dataValue = this.FirstOrDefault(x => string.Compare(x.ColumnName, columnName, true) == 0);
            if (dataValue != null)
            {
                return Convert.ToInt64(dataValue.Value);
            }
            return null;
        }

        public byte? GetByte(string columnName)
        {
            var dataValue = this.FirstOrDefault(x => string.Compare(x.ColumnName, columnName, true) == 0);
            if (dataValue != null)
            {
                if (dataValue.Value == null) return null;
                return Convert.ToByte(dataValue.Value);
            }
            return null;
        }

        public decimal? GetDecimal(string columnName)
        {
            var dataValue = this.FirstOrDefault(x => string.Compare(x.ColumnName, columnName, true) == 0);
            if (dataValue != null)
            {
                if (dataValue.Value == null) return null;
                return Convert.ToDecimal(dataValue.Value);
            }
            return null;
        }

        public DateTime? GetDateTime(string columnName)
        {
            var dataValue = this.FirstOrDefault(x => string.Compare(x.ColumnName, columnName, true) == 0);
            if (dataValue != null)
            {
                if (dataValue.Value == null) return null;
                return Convert.ToDateTime(dataValue.Value);
            }
            return null;
        }


        public string GetString(string columnName)
        {
            var dataValue = this.FirstOrDefault(x => string.Compare(x.ColumnName, columnName, true) == 0);
            if (dataValue != null)
            {
                if (dataValue.Value == null) return null;
                return Convert.ToString(dataValue.Value);
            }
            return null;
        }


    }

}
