// In ExpressionParser.cs
using SimpleRDBMS.Parser.AST;
using SimpleRDBMS.Parser.Lexer;

namespace SimpleRDBMS.Parser.Parser
{
    public class ExpressionParser
    {
        private readonly SqlLexer _lexer;
        private Token _currentToken;

        public ExpressionParser(SqlLexer lexer)
        {
            _lexer = lexer;
            _currentToken = _lexer.GetNextToken();
        }

        public AST.Expression ParseExpression()
        {
            _currentToken = _lexer.GetNextToken();
            return ParseSimpleExpression();
        }

        public AST.Expression ParseSimpleExpression()
        {
            var left = ParsePrimary();

            if (_currentToken.Type == TokenType.OPERATOR)
            {
                var op = _currentToken.Value;
                _currentToken = _lexer.GetNextToken();

                var right = ParsePrimary();

                return new BinaryExpression
                {
                    Left = left,
                    Operator = op,
                    Right = right
                };
            }

            return left;
        }

        private AST.Expression ParsePrimary()
        {
            return _currentToken.Type switch
            {
                TokenType.IDENTIFIER => new ColumnReferenceExpression
                {
                    ColumnName = _currentToken.Value
                },
                TokenType.NUMBER => new LiteralExpression
                {
                    Value = int.Parse(_currentToken.Value)
                },
                TokenType.STRING => new LiteralExpression
                {
                    Value = _currentToken.Value.Trim('\'')
                },
                _ => throw new FormatException($"Unexpected token in expression: {_currentToken}")
            };
        }
    }
}