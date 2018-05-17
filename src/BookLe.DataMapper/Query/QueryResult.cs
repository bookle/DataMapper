using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookLe.DataMapper.Query
{
    public class QueryResult<T> where T : class, new()
    {
        public List<T> List { get; set; }
        public List<QueryParameter> Parameters { get; set; } 
    }

}
