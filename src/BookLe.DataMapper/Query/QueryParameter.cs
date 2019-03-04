namespace BookLe.DataMapper.Query
{
    public class QueryParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public DataTypeEnum? DataType { get; set; }
        public int? Size { get; set; }
        public byte? Precision { get; set; }
        public byte? Scale { get; set; }
        public QueryParameterDirectionEnum? Direction { get; set; }
    }

    public enum QueryParameterDirectionEnum
    {
        Input,
        Output,
        InputOutput,
        ReturnValue
    }
}
