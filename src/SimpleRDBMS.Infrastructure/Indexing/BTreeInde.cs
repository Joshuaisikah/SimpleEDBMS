
using SimpleRDBMS.Domain.Entities;

namespace SimpleRDBMS.Infrastructure.Indexing
{
    public class BTreeIndex : SimpleRDBMS.Domain.Entities.Index
    {
        private readonly SortedDictionary<object, List<Row>> _index = new SortedDictionary<object, List<Row>>();

        public BTreeIndex(string name, string tableName, List<string> columnNames) : base(name, tableName, columnNames) { }

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
            var result = new List<Row>();
            foreach (var kvp in _index)
            {
                if (((IComparable)kvp.Key).CompareTo(start) >= 0 && ((IComparable)kvp.Key).CompareTo(end) <= 0)
                {
                    result.AddRange(kvp.Value);
                }
            }
            return result;
        }
    }
}