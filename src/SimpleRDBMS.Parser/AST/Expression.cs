namespace SimpleRDBMS.Parser.AST
{
    /// <summary>
    /// Base class for all expression nodes (WHERE conditions, VALUES, SET expressions, etc.)
    /// </summary>
    public abstract class Expression
    {
    }

    public class LiteralExpression : Expression
    {
        public object Value { get; set; }
        public LiteralType Type { get; set; }

        public enum LiteralType
        {
            Integer,
            String,
            Boolean,
            Decimal,
            Null
        }
    }

    public class ColumnReferenceExpression : Expression
    {
        public string ColumnName { get; set; }
        // Future: could have TableAlias.ColumnName
    }

    public class BinaryExpression : Expression
    {
        public Expression Left { get; set; }
        public string Operator { get; set; } // "=", ">", "<", ">=", "<=", "!=", "AND", "OR", etc.
        public Expression Right { get; set; }
    }

    // You can add more expression types later:
    // UnaryExpression (NOT, -), FunctionCallExpression (COUNT(), etc.)
}