
namespace SimpleRDBMS.Infrastructure.Persistence.Interfaces
{
    public interface IIndexManager
    {
        void CreateIndex(string tableName, string indexName, List<string> columns);
        void DropIndex(string indexName);
        void RebuildIndex(string indexName);
    }
}