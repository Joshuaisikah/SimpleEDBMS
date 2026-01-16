namespace SimpleRDBMS.Application.Commands
{
    public class CreateIndexCommand
    {
        public string IndexName { get; }
        public string TableName { get; }
        public List<string> ColumnNames { get; }

        public CreateIndexCommand(string indexName, string tableName, List<string> columnNames)
        {
            IndexName = indexName;
            TableName = tableName;
            ColumnNames = columnNames;
        }
    }
}