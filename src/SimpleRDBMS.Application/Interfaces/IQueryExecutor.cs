using SimpleRDBMS.Application.Queries;

namespace SimpleRDBMS.Application.Interfaces
{
    public interface IQueryExecutor
    {
        QueryResult Execute(SelectQuery query);
        QueryResult Execute(JoinQuery query);
    }
}