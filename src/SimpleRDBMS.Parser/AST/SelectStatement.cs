using SimpleRDBMS.Parser.Visitors;

namespace SimpleRDBMS.Parser.AST
{
    public class SelectStatement : Statement
    {
        public List<string> Columns { get; set; } = new();
        public string TableName { get; set; }
        public Expression WhereClause { get; set; }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}