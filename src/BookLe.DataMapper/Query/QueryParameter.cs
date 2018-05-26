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
