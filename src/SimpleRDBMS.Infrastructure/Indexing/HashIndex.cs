using SimpleRDBMS.Domain.Entities;

namespace SimpleRDBMS.Infrastructure.Indexing
{
    public class HashIndex : Domain.Entities.Index
    {
        private readonly Dictionary<object, List<Row>> _index = new Dictionary<object, List<Row>>();

        public HashIndex(string name, string tableName, List<string> columnNames) : base(name, tableName, columnNames) { }

        public override void Insert(object key, Row row)
        {
            if (!_index.TryGetValue(key, out var list))
            {
                list = new List<Row>();
                _index[key] = list;
            }
            list.Add(row);
        }

        public override void Delete(object key, Row row)
        {
            if (_index.TryGetValue(key, out var list))
            {
                list.Remove(row);
            }
        }

        public override IEnumerable<Row> Search(object key)
        {
            _index.TryGetValue(key, out var list);
            return list ?? new List<Row>();
        }

        public override IEnumerable<Row> RangeScan(object start, object end)
        {
            throw new NotSupportedException("HashIndex does not support range scans.");
        }
    }
}