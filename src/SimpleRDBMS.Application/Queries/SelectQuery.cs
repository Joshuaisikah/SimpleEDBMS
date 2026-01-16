namespace SimpleRDBMS.Application.Queries
{
    public class SelectQuery
    {
        public List<string> Columns { get; }
        public string TableName { get; }
        public string WhereClause { get; } // Simplified

        public SelectQuery(List<string> columns, string tableName, string whereClause)
        {
            Columns = columns;
            TableName = tableName;
            WhereClause = whereClause;
        }
    }
}