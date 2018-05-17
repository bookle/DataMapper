using System;

namespace BookLe.DataMapper.Query
{
    public class DataColumnNotFoundException : ApplicationException 
    {
        public DataColumnNotFoundException(string message) : base(message) { }
    }
}
