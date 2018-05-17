using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookLe.DataMapper.Query
{
    public class QueryParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public QueryParameterDirectionEnum Direction { get; set; }
    }

    public enum QueryParameterDirectionEnum
    {
        Input,
        Output
    }
}
