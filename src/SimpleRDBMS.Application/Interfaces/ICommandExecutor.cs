using SimpleRDBMS.Application.Commands;
using SimpleRDBMS.Application.DTOs;

namespace SimpleRDBMS.Application.Interfaces
{
    public interface ICommandExecutor
    {
        ExecutionResultDto Execute(CreateTableCommand command);
        ExecutionResultDto Execute(InsertCommand command);
        ExecutionResultDto Execute(UpdateCommand command);
        ExecutionResultDto Execute(DeleteCommand command);
        ExecutionResultDto Execute(CreateIndexCommand command);
    }
}