
using System.ComponentModel.DataAnnotations;

namespace SimpleRDBMS.Parser.AST
{
    public class ColumnDefinitionAst
    {
        public string Name { get; set; }
        public SimpleRDBMS.Domain.ValueObjects.DataType DataType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
        public bool IsNotNull { get; set; }
    }
}