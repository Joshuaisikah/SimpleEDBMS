using SimpleRDBMS.Parser.Visitors;

namespace SimpleRDBMS.Parser.AST
{
    public class UpdateStatement : Statement
    {
        public string TableName { get; set; }
        
        /// <summary>
        /// SET column = value assignments
        /// </summary>
        public List<Assignment> Assignments { get; set; } = new List<Assignment>();
        
        /// <summary>
        /// Optional WHERE clause
        /// </summary>
        public Expression WhereClause { get; set; }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <summary>
        /// Simple assignment node used in UPDATE
        /// </summary>
        public class Assignment
        {
            public string ColumnName { get; set; }
            public Expression Value { get; set; }
        }
    }
}