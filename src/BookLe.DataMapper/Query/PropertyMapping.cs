using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace BookLe.DataMapper.Query
{
    /// <summary>
    /// Mapping definition between a database column and a class property
    /// </summary>
    public class PropertyMapping
    {
        /// <summary>
        /// Class Property Name
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// Database Column Name
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// Function that performs the mapping. This is optional. If null, will do it's best
        /// to perform the mapping.
        /// </summary>
        public Func<DataValueList, object> PropertyMapperDataRow { get; set; }
        /// <summary>
        /// The index of the column.
        /// </summary>
        internal int OrdinalIndex { get; set; }
        internal PropertyInfo Property { get; set; }
        public override int GetHashCode()
        {
            return PropertyName.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var other = obj as PropertyMapping;
            if (other == null) return false;
            return (other.PropertyName == this.PropertyName);
        }
    }
}
