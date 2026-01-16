
namespace SimpleRDBMS.Domain.Exceptions
{
    public class TableNotFoundException : Exception
    {
        public TableNotFoundException(string tableName) 
            : base($"Table '{tableName}' not found.") { }
    }
}