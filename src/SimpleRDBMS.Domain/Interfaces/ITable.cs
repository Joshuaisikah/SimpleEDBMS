using System.Collections.Generic;
using SimpleRDBMS.Domain.Entities;

namespace SimpleRDBMS.Domain.Interfaces
{
    public interface ITable
    {
        string Name { get; }
        IReadOnlyList<Column> Columns { get; }
        void AddRow(Row row);
        void RemoveRow(Row row);
        IEnumerable<Row> GetRows();
    }
}