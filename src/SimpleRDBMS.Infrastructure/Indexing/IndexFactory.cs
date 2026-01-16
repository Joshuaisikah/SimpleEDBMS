namespace SimpleRDBMS.Infrastructure.Indexing
{
    public class IndexFactory
    {
        public Domain.Entities.Index CreateIndex(string type, string name, string tableName, List<string> columnNames)
        {
            return type switch
            {
                "Hash" => new HashIndex(name, tableName, columnNames),
                "BTree" => new BTreeIndex(name, tableName, columnNames),
                _ => throw new ArgumentException("Unknown index type")
            };
        }
    }
}