namespace SimpleRDBMS.Application.Interfaces
{
    public interface ITransactionManager
    {
        void BeginTransaction();
        void Commit();
        void Rollback();
    }
}