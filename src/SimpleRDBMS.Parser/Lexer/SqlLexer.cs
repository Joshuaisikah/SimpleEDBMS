using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRDBMS.Parser.Lexer
{
    public class SqlLexer
    {
        private readonly string _input;
        private int _position;
        private List<Token> _tokens;
        private int _currentTokenIndex = -1;

        private static readonly HashSet<string> Keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CREATE", "TABLE", "INSERT", "INTO", "VALUES", "SELECT", "FROM", "WHERE",
            "UPDATE", "DELETE", "INT", "VARCHAR", "BOOL", "BOOLEAN", "DECIMAL", "DATE"
        };

        public SqlLexer(string sql)
        {
            _input = sql ?? throw new ArgumentNullException(nameof(sql));
            _position = 0;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_position < _input.Length)
            {
                char current = _input[_position];

                // Skip whitespace
                if (char.IsWhiteSpace(current))
                {
                    _position++;
                    continue;
                }

                // Identifier or keyword
                if (char.IsLetter(current) || current == '_')
                {
                    tokens.Add(ReadIdentifierOrKeyword());
                    continue;
                }

                // Number
                if (char.IsDigit(current))
                {
                    tokens.Add(ReadNumber());
                    continue;
                }

                // String literal
                if (current == '\'')
                {
                    tokens.Add(ReadString());
                    continue;
                }

                // Single character tokens
                switch (current)
                {
                    case '(':
                    case ')':
                    case ',':
                    case ';':
                    case '*':
                        tokens.Add(new Token(TokenType.PUNCTUATION, current.ToString(), _position));
                        _position++;
                        break;

                    case '=':
                    case '>':
                    case '<':
                        tokens.Add(new Token(TokenType.OPERATOR, current.ToString(), _position));
                        _position++;
                        break;

                    default:
                        throw new FormatException($"Unexpected character '{current}' at position {_position}");
                }
            }

            return tokens;
        }

        public Token GetNextToken()
        {
            if (_tokens == null)
            {
                _tokens = Tokenize();
            }
            
            _currentTokenIndex++;
            return _currentTokenIndex < _tokens.Count ? _tokens[_currentTokenIndex] : null;
        }

        private Token ReadIdentifierOrKeyword()
        {
            int start = _position;
            while (_position < _input.Length && (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
                _position++;

            string value = _input.Substring(start, _position - start);

            var type = Keywords.Contains(value)
                ? TokenType.KEYWORD
                : TokenType.IDENTIFIER;

            return new Token(type, value, start);
        }

        private Token ReadNumber()
        {
            int start = _position;
            while (_position < _input.Length && char.IsDigit(_input[_position]))
                _position++;

            string value = _input.Substring(start, _position - start);
            return new Token(TokenType.NUMBER, value, start);
        }

        private Token ReadString()
        {
            int start = _position;
            _position++; // skip opening '

            var sb = new StringBuilder();
            while (_position < _input.Length)
            {
                char c = _input[_position];
                if (c == '\'')
                {
                    _position++;
                    break;
                }
                sb.Append(c);
                _position++;
            }

            return new Token(TokenType.STRING, sb.ToString(), start);
        }
    }
}