
using SimpleRDBMS.Domain.ValueObjects;

namespace SimpleRDBMS.Application.Commands
{
    public class CreateTableCommand
    {
        public string TableName { get; }
        public IEnumerable<ColumnDefinition> Columns { get; }

        public CreateTableCommand(string tableName, IEnumerable<ColumnDefinition> columns)
        {
            TableName = tableName;
            Columns = columns;
        }
    }
}