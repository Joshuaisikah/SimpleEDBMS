using SimpleRDBMS.Domain.ValueObjects;
using SimpleRDBMS.Parser.AST;
using SimpleRDBMS.Parser.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
namespace SimpleRDBMS.Parser.Parser
{
    public class StatementParser
    {
        private readonly SqlLexer _lexer;
        private readonly ExpressionParser _expressionParser;
        private Token _currentToken;

        public StatementParser(SqlLexer lexer)
        {
            _lexer = lexer;
            _expressionParser = new ExpressionParser(lexer);
            _currentToken = _lexer.GetNextToken();
        }

        private AST.Expression ParseSimpleExpression()
        {
            return _expressionParser.ParseSimpleExpression();
        }

        private AST.Expression ParseExpression()
        {
            return _expressionParser.ParseExpression();
        }

        private List<AST.Expression> ParseValueList()
        {
            var values = new List<AST.Expression>();
            while (_currentToken.Type != TokenType.PUNCTUATION || _currentToken.Value != ")")
            {
                values.Add(ParseSimpleExpression());
                if (_currentToken.Value == ",")
                    _lexer.GetNextToken();
            }
            return values;
        }

        public Statement ParseStatement()
        {
            _currentToken = _lexer.GetNextToken();

            return _currentToken.Type switch
            {
                TokenType.KEYWORD when _currentToken.Value.Equals("CREATE", StringComparison.OrdinalIgnoreCase) =>
                    ParseCreateTableStatement(),

                TokenType.KEYWORD when _currentToken.Value.Equals("INSERT", StringComparison.OrdinalIgnoreCase) =>
                    ParseInsertStatement(),

                TokenType.KEYWORD when _currentToken.Value.Equals("SELECT", StringComparison.OrdinalIgnoreCase) =>
                    ParseSelectStatement(),

                TokenType.KEYWORD when _currentToken.Value.Equals("UPDATE", StringComparison.OrdinalIgnoreCase) =>
                    ParseUpdateStatement(),

                TokenType.KEYWORD when _currentToken.Value.Equals("DELETE", StringComparison.OrdinalIgnoreCase) =>
                    ParseDeleteStatement(),

                _ => throw new NotSupportedException(
                    $"Unsupported statement starting with: {_currentToken.Value}")
            };
        }

        private CreateTableStatement ParseCreateTableStatement()
        {
            Expect(TokenType.KEYWORD, "TABLE");
            var tableName = Expect(TokenType.IDENTIFIER).Value;

            Expect(TokenType.PUNCTUATION, "(");

            var columns = new List<ColumnDefinition>();
            while (_currentToken.Type != TokenType.PUNCTUATION || _currentToken.Value != ")")
            {
                var colName = Expect(TokenType.IDENTIFIER).Value;
                var dataTypeToken = ExpectAny(TokenType.KEYWORD, TokenType.IDENTIFIER);

                var dataType = ParseDataType(dataTypeToken.Value);
                int? size = null;

                // Handle size for VARCHAR(n)
                if (_currentToken.Type == TokenType.PUNCTUATION && _currentToken.Value == "(")
                {
                    _lexer.GetNextToken(); // consume '('
                    var sizeToken = Expect(TokenType.NUMBER);
                    size = int.Parse(sizeToken.Value);
                    Expect(TokenType.PUNCTUATION, ")");
                }

                columns.Add(new ColumnDefinition
                {
                    Name = colName,
                    DataType = new DataType(
                        type: dataType,
                        size: size
                    )
                });

                if (_currentToken.Value == ",")
                    _lexer.GetNextToken();
                else if (_currentToken.Value != ")")
                    throw new FormatException("Expected ',' or ')' after column definition");
            }

            Expect(TokenType.PUNCTUATION, ")");

            // Map Domain ColumnDefinition to AST ColumnDefinitionAst
            var astColumns = columns.Select(col => new ColumnDefinitionAst
            {
                Name = col.Name,
                DataType = col.DataType,
                IsNotNull = !col.IsNullable,
                IsPrimaryKey = false, // Default to false, can be set based on constraints if needed
                IsUnique = false      // Default to false, can be set based on constraints if needed
            }).ToList();

            return new CreateTableStatement
            {
                TableName = tableName,
                Columns = astColumns
            };
        }

        private InsertStatement ParseInsertStatement()
        {
            Expect(TokenType.KEYWORD, "INTO");
            var tableName = Expect(TokenType.IDENTIFIER).Value;

            // Handle column list if present
            var columns = new List<string>();
            if (_currentToken.Type == TokenType.PUNCTUATION && _currentToken.Value == "(")
            {
                _lexer.GetNextToken(); // consume '('
                while (_currentToken.Type != TokenType.PUNCTUATION || _currentToken.Value != ")")
                {
                    columns.Add(Expect(TokenType.IDENTIFIER).Value);
                    if (_currentToken.Value == ",")
                        _lexer.GetNextToken();
                }
                _lexer.GetNextToken(); // consume ')'
            }

            Expect(TokenType.KEYWORD, "VALUES");
            Expect(TokenType.PUNCTUATION, "(");
            
            var values = ParseValueList();
            
            Expect(TokenType.PUNCTUATION, ")");

            return new InsertStatement
            {
                TableName = tableName,
                Columns = columns,
                Values = values
            };
        }

        private SelectStatement ParseSelectStatement()
        {
            // Very basic SELECT * FROM ... support only for MVP
            var columns = new List<string>();

            if (_currentToken.Value.Equals("*"))
            {
                columns.Add("*");
                _lexer.GetNextToken();
            }
            else
            {
                // Could add support for column lists later
                throw new NotSupportedException("Only SELECT * supported in this version");
            }

            Expect(TokenType.KEYWORD, "FROM");
            var tableName = Expect(TokenType.IDENTIFIER).Value;

            // Optional WHERE (very basic)
            AST.Expression where = null;
            if (_currentToken.Type == TokenType.KEYWORD && _currentToken.Value.Equals("WHERE", StringComparison.OrdinalIgnoreCase))
            {
                _lexer.GetNextToken();
                where = ParseExpression();
            }

            return new SelectStatement
            {
                Columns = columns,
                TableName = tableName,
                WhereClause = where
            };
        }

        private UpdateStatement ParseUpdateStatement()
        {
            throw new NotImplementedException("UPDATE not implemented yet in this simple parser");
        }

        private DeleteStatement ParseDeleteStatement()
        {
            throw new NotImplementedException("DELETE not implemented yet in this simple parser");
        }

        // ──────────────────────────────────────────────────────────────────────────────
        //   Helper methods
        // ──────────────────────────────────────────────────────────────────────────────

        private Token Expect(TokenType type, string value = null)
        {
            if (_currentToken == null)
                throw new FormatException($"Expected {type} but reached end of input");

            if (_currentToken.Type != type || (value != null && !_currentToken.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                throw new FormatException(
                    $"Expected {type}{(value != null ? $" '{value}'" : "")} but found {_currentToken.Type} '{_currentToken.Value}' at position {_currentToken.Position}");
            }
    
            var token = _currentToken;
            _currentToken = _lexer.GetNextToken();
            return token;
        }

        private Token ExpectAny(params TokenType[] types)
        {
            if (!Array.Exists(types, t => t == _currentToken.Type))
            {
                throw new FormatException($"Expected one of {string.Join(", ", types)} but found {_currentToken}");
            }
            var token = _currentToken;
            _currentToken = _lexer.GetNextToken();
            return token;
        }

        private DataTypeEnum ParseDataType(string typeStr)
        {
            return typeStr.ToUpperInvariant() switch
            {
                "INT" or "INTEGER" => DataTypeEnum.INT,
                "VARCHAR" => DataTypeEnum.VARCHAR,
                "BOOL" or "BOOLEAN" => DataTypeEnum.BOOL,
                "DECIMAL" or "NUMERIC" => DataTypeEnum.DECIMAL,
                "DATE" => DataTypeEnum.DATE,
                _ => throw new NotSupportedException($"Unknown data type: {typeStr}")
            };
        }
    }
}