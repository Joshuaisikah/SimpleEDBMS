namespace SimpleRDBMS.Application.Commands
{
    public class DeleteCommand
    {
        public string TableName { get; }
        public string WhereClause { get; } // Simplified

        public DeleteCommand(string tableName, string whereClause)
        {
            TableName = tableName;
            WhereClause = whereClause;
        }
    }
}