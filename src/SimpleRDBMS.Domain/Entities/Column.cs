
using SimpleRDBMS.Domain.ValueObjects;

namespace SimpleRDBMS.Domain.Entities
{
    public class Column
    {
        public string Name { get; private set; }
        public DataType DataType { get; private set; }
        public bool IsNullable { get; private set; }
        public object DefaultValue { get; private set; }

        public Column(ColumnDefinition definition)
        {
            Name = definition.Name;
            DataType = definition.DataType;
            IsNullable = definition.IsNullable;
            DefaultValue = definition.DefaultValue;
        }
    }
}