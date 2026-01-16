namespace SimpleRDBMS.Application.Commands
{
    public class UpdateCommand
    {
        public string TableName { get; }
        public Dictionary<string, object> SetValues { get; }
        public string WhereClause { get; } // Simplified as string for now

        public UpdateCommand(string tableName, Dictionary<string, object> setValues, string whereClause)
        {
            TableName = tableName;
            SetValues = setValues;
            WhereClause = whereClause;
        }
    }
}