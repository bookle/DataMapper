using System.Collections.Generic;

namespace BookLe.DataMapper.Query
{
    public class QueryResult<T> where T : class, new()
    {
        public List<T> List { get; set; }
        public List<QueryParameter> Parameters { get; set; } 
    }

}
