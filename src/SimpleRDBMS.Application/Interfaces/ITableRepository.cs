using SimpleRDBMS.Domain.Entities;

namespace SimpleRDBMS.Infrastructure.Persistence.Interfaces
{
    public interface ITableRepository
    {
        Table GetTable(string name);
        void SaveTable(Table table);
        void DeleteTable(string name);
        IEnumerable<string> ListTables();
    }
}