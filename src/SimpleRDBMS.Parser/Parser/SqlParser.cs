using System;
using SimpleRDBMS.Parser.AST;
using SimpleRDBMS.Parser.Lexer;

namespace SimpleRDBMS.Parser.Parser
{
    public class SqlParser
    {
        private SqlLexer _lexer;
        private StatementParser _statementParser;

        public SqlParser()
        {
            // Initialize with empty SQL, will be set in Parse method
            _lexer = new SqlLexer(string.Empty);
            _statementParser = new StatementParser(_lexer);
        }

        public Statement Parse(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL statement cannot be empty");

            // Create new lexer instance with the SQL to parse
            _lexer = new SqlLexer(sql);
            _statementParser = new StatementParser(_lexer);

            // Tokenization happens automatically in GetNextToken()
            try
            {
                return _statementParser.ParseStatement();
            }
            catch (Exception ex)
            {
                throw new FormatException($"Failed to parse SQL: {ex.Message}", ex);
            }
        }
    }
}