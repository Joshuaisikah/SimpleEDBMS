using System.Collections.Concurrent;
using SimpleRDBMS.Infrastructure.Indexing;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;
using Index = SimpleRDBMS.Domain.Entities.Index;  

namespace SimpleRDBMS.Infrastructure.Persistence.InMemory
{
    public class InMemoryIndexManager : IIndexManager
    {
        private readonly ConcurrentDictionary<string, Index> _indexes = new();
        private readonly IndexFactory _factory = new();

        public void CreateIndex(string tableName, string indexName, List<string> columns)
        {
            var index = _factory.CreateIndex("Hash", indexName, tableName, columns);
            _indexes[indexName] = index;
        }

        public void DropIndex(string indexName)
        {
            _indexes.TryRemove(indexName, out _);
        }

        public void RebuildIndex(string indexName)
        {
            // For in-memory, no rebuild needed
        }

        public Index GetIndex(string indexName)
        {
            _indexes.TryGetValue(indexName, out var index);
            return index;
        }
    }
}