using SimpleRDBMS.Domain.Entities;

namespace SimpleRDBMS.Application.Queries
{
    public class QueryResult
    {
        public List<Column> Columns { get; }
        public List<Row> Rows { get; }

        public QueryResult(List<Column> columns, List<Row> rows)
        {
            Columns = columns;
            Rows = rows;
        }
    }
}