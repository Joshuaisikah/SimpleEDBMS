namespace SimpleRDBMS.Infrastructure.Persistence.Interfaces
{
    public interface ITransactionLog
    {
        void BeginTransaction();
        void LogOperation(string operation);
        void Commit();
        void Rollback();
    }
}