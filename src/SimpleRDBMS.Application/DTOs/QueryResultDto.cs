namespace SimpleRDBMS.Application.DTOs
{
    public class QueryResultDto
    {
        public List<string> ColumnNames { get; set; }
        public List<Dictionary<string, object>> Rows { get; set; }
    }
}