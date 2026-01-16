using System.Collections.Concurrent;
using SimpleRDBMS.Domain.Entities;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;

namespace SimpleRDBMS.Infrastructure.Persistence.InMemory
{
    public class InMemoryTableRepository : ITableRepository
    {
        private readonly ConcurrentDictionary<string, Table> _tables = new ConcurrentDictionary<string, Table>();

        public Table GetTable(string name)
        {
            _tables.TryGetValue(name, out var table);
            return table;
        }

        public void SaveTable(Table table)
        {
            _tables[table.Name] = table;
        }

        public void DeleteTable(string name)
        {
            _tables.TryRemove(name, out _);
        }

        public IEnumerable<string> ListTables()
        {
            return _tables.Keys;
        }
    }
}