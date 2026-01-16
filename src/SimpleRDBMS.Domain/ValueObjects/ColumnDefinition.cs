// src/SimpleRDBMS.Domain/ValueObjects/ColumnDefinition.cs
namespace SimpleRDBMS.Domain.ValueObjects
{
    public class ColumnDefinition
    {
        public string Name { get; set; }
        public DataType DataType { get; set; }
        public bool IsNullable { get; set; } = true;
        public object DefaultValue { get; set; } = null;
    }
}