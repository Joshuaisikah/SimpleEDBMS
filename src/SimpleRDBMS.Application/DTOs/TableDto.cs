
using SimpleRDBMS.Domain.ValueObjects;

namespace SimpleRDBMS.Application.DTOs
{
    public class TableDto
    {
        public string Name { get; set; }
        public List<ColumnDefinition> Columns { get; set; }
        public int RowCount { get; set; }
    }
}