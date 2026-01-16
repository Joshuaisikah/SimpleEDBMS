
using SimpleRDBMS.Domain.Interfaces;

namespace SimpleRDBMS.Domain.Entities
{
    public abstract class Index : IIndex
    {
        public string Name { get; protected set; }
        public string TableName { get; protected set; }
        public List<string> ColumnNames { get; protected set; }

        protected Index(string name, string tableName, List<string> columnNames)
        {
            Name = name;
            TableName = tableName;
            ColumnNames = columnNames;
        }

        public abstract void Insert(object key, Row row);
        public abstract void Delete(object key, Row row);
        public abstract IEnumerable<Row> Search(object key);
        public abstract IEnumerable<Row> RangeScan(object start, object end);
    }
}