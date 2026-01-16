using SimpleRDBMS.Parser.Visitors;

namespace SimpleRDBMS.Parser.AST
{
    public class CreateTableStatement : Statement
    {
        public string TableName { get; set; }
        public List<ColumnDefinitionAst> Columns { get; set; } = new();

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}