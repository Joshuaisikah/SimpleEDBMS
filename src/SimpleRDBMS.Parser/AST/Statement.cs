// src/SimpleRDBMS.Parser/AST/Statement.cs

using SimpleRDBMS.Parser.Visitors;

namespace SimpleRDBMS.Parser.AST
{
    /// <summary>
    /// Base class for all SQL statement nodes in the AST
    /// </summary>
    public abstract class Statement
    {
        public abstract void Accept(IStatementVisitor visitor);
    }
}