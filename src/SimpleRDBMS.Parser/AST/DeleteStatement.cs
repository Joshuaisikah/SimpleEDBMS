using SimpleRDBMS.Parser.Visitors;

namespace SimpleRDBMS.Parser.AST
{
    public class DeleteStatement : Statement
    {
        public string TableName { get; set; }
        public Expression WhereClause { get; set; }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}