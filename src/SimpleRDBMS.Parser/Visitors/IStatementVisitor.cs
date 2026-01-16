using SimpleRDBMS.Parser.AST;

namespace SimpleRDBMS.Parser.Visitors
{
    public interface IStatementVisitor
    {
        void Visit(CreateTableStatement stmt);
        void Visit(InsertStatement stmt);
        void Visit(SelectStatement stmt);
        void Visit(UpdateStatement stmt);
        void Visit(DeleteStatement stmt);
    }
}