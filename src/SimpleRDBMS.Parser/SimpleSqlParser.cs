using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SimpleRDBMS.Application.Queries;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;
using SimpleRDBMS.Parser.AST;
using SimpleRDBMS.Parser.Visitors;
using SimpleRDBMS.Domain.ValueObjects;

namespace SimpleRDBMS.Parser
{
    public class SimpleSqlParser
    {
        private readonly ITableRepository _tableRepository;

        public SimpleSqlParser(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository ?? throw new ArgumentNullException(nameof(tableRepository));
        }
        public object Parse(string sql)
        {
            sql = sql.Trim().TrimEnd(';');

            Statement stmt;
    
            if (sql.StartsWith("CREATE TABLE", StringComparison.OrdinalIgnoreCase))
                stmt = ParseCreateTableStatement(sql);
            else if (sql.StartsWith("INSERT INTO", StringComparison.OrdinalIgnoreCase))
                stmt = ParseInsertStatement(sql);
            else if (sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                return ParseSelectStatementOrJoin(sql); // CHANGED: Now handles JOINs
            else if (sql.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
                stmt = ParseUpdateStatement(sql);
            else if (sql.StartsWith("DELETE FROM", StringComparison.OrdinalIgnoreCase))
                stmt = ParseDeleteStatement(sql);
            else if (sql.StartsWith("CREATE INDEX", StringComparison.OrdinalIgnoreCase))
                return ParseCreateIndexDirect(sql);
            else
                throw new Exception("Unsupported SQL statement");

            var builder = new CommandBuilder(_tableRepository);
            stmt.Accept(builder);
            return builder.Result;
        }

        private CreateTableStatement ParseCreateTableStatement(string sql)
        {
            // CREATE TABLE users (id INT PRIMARY KEY, name VARCHAR(50) NOT NULL, age INT)
            var match = Regex.Match(sql, @"CREATE TABLE (\w+)\s*\((.*)\)", RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new Exception("Invalid CREATE TABLE syntax");

            var tableName = match.Groups[1].Value;
            var columnDefs = match.Groups[2].Value;

            var columns = new List<ColumnDefinitionAst>();
            foreach (var colDef in columnDefs.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                columns.Add(ParseColumnDefinitionAst(colDef));
            }

            return new CreateTableStatement
            {
                TableName = tableName,
                Columns = columns
            };
        }
        private object ParseSelectStatementOrJoin(string sql)
        {
            // Check if this is a JOIN query
            if (sql.Contains("JOIN", StringComparison.OrdinalIgnoreCase))
            {
                return ParseJoinQuery(sql);
            }
    
            // Regular SELECT
            return ParseSelectStatement(sql);
        }

        private object ParseJoinQuery(string sql)
        {
            // Simple JOIN parsing: SELECT * FROM table1 JOIN table2 ON table1.col = table2.col
            var match = Regex.Match(sql, 
                @"SELECT (.*?) FROM (\w+)\s+(?:INNER\s+)?JOIN\s+(\w+)\s+ON\s+(.*)",
                RegexOptions.IgnoreCase);
    
            if (!match.Success)
                throw new Exception("Invalid JOIN syntax. Use: SELECT * FROM table1 JOIN table2 ON table1.col = table2.col");

            var columns = match.Groups[1].Value.Split(',').Select(c => c.Trim()).ToList();
            var leftTable = match.Groups[2].Value;
            var rightTable = match.Groups[3].Value;
            var joinCondition = match.Groups[4].Value.Trim();

            // Create left and right queries
            var leftQuery = new SelectQuery(new List<string> { "*" }, leftTable, "");
            var rightQuery = new SelectQuery(new List<string> { "*" }, rightTable, "");

            return new JoinQuery(
                leftQuery,
                rightQuery,
                JoinType.Inner,
                joinCondition
            );
        }
        private ColumnDefinitionAst ParseColumnDefinitionAst(string colDef)
        {
            var parts = colDef.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var name = parts[0];
            var typeStr = parts[1];

            var typeMatch = Regex.Match(typeStr, @"(\w+)(?:\((\d+)\))?", RegexOptions.IgnoreCase);
            var typeName = typeMatch.Groups[1].Value.ToUpper();
            var size = typeMatch.Groups[2].Success ? int.Parse(typeMatch.Groups[2].Value) : (int?)null;

            var dataType = new DataType(
                (DataTypeEnum)Enum.Parse(typeof(DataTypeEnum), typeName), 
                size
            );

            var isPrimaryKey = colDef.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase);
            var isUnique = colDef.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
            var isNotNull = colDef.Contains("NOT NULL", StringComparison.OrdinalIgnoreCase);

            return new ColumnDefinitionAst
            {
                Name = name,
                DataType = dataType,
                IsPrimaryKey = isPrimaryKey,
                IsUnique = isUnique,
                IsNotNull = isNotNull
            };
        }

        private InsertStatement ParseInsertStatement(string sql)
        {
            // Support two formats:
            // 1. INSERT INTO users (id, name, age) VALUES (1, 'Alice', 30)
            // 2. INSERT INTO users VALUES (1, 'Alice', 30)

            Match match;
            string tableName;
            List<string> columns = null;
            List<Expression> values;

            // Try format with column names first
            match = Regex.Match(sql, @"INSERT INTO (\w+)\s*\((.*?)\)\s*VALUES\s*\((.*?)\)", RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                // Format 1: columns specified
                tableName = match.Groups[1].Value;
                columns = match.Groups[2].Value.Split(',').Select(c => c.Trim()).ToList();
                values = ParseValueExpressions(match.Groups[3].Value);
            }
            else
            {
                // Format 2: no columns specified - try simpler pattern
                match = Regex.Match(sql, @"INSERT INTO (\w+)\s+VALUES\s*\((.*?)\)", RegexOptions.IgnoreCase);
                
                if (!match.Success)
                    throw new Exception("Invalid INSERT syntax");

                tableName = match.Groups[1].Value;
                values = ParseValueExpressions(match.Groups[2].Value);
                
                // Get columns from table schema
                var table = _tableRepository.GetTable(tableName);
                if (table == null)
                    throw new Exception($"Table '{tableName}' not found");
                
                columns = table.Columns.Select(c => c.Name).ToList();
            }

            return new InsertStatement
            {
                TableName = tableName,
                Columns = columns,
                Values = values
            };
        }

        private SelectStatement ParseSelectStatement(string sql)
        {
            // SELECT * FROM users WHERE age > 25
            var match = Regex.Match(sql, @"SELECT (.*?) FROM (\w+)(?:\s+WHERE\s+(.+))?", RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new Exception("Invalid SELECT syntax");

            var columns = match.Groups[1].Value.Split(',').Select(c => c.Trim()).ToList();
            var tableName = match.Groups[2].Value;
            var whereClauseStr = match.Groups[3].Success ? match.Groups[3].Value : null;

            Expression whereClause = null;
            if (whereClauseStr != null)
            {
                whereClause = ParseWhereExpression(whereClauseStr);
            }

            return new SelectStatement
            {
                Columns = columns,
                TableName = tableName,
                WhereClause = whereClause
            };
        }

      private UpdateStatement ParseUpdateStatement(string sql)
{
    // UPDATE users SET age = 31 WHERE id = 1
    // UPDATE users SET age = 31, name = 'Bob' WHERE id = 1
    
    var match = Regex.Match(sql, @"UPDATE (\w+) SET (.*?)(?:\s+WHERE\s+(.+))?$", RegexOptions.IgnoreCase);
    if (!match.Success)
        throw new Exception("Invalid UPDATE syntax");

    var tableName = match.Groups[1].Value;
    var setClause = match.Groups[2].Value;
    var whereClauseStr = match.Groups[3].Success ? match.Groups[3].Value : null;

    var assignments = new List<UpdateStatement.Assignment>();
    
    // Parse SET clause - handle "col = value" pairs
    // Need to be careful with commas inside quoted strings
    var setPairs = ParseSetClause(setClause);
    
    foreach (var pair in setPairs)
    {
        // Each pair should be "column = value"
        var eqIndex = pair.IndexOf('=');
        
        if (eqIndex == -1 || eqIndex == 0 || eqIndex == pair.Length - 1)
        {
            throw new Exception($"Invalid SET clause: {pair}");
        }
        
        var columnName = pair.Substring(0, eqIndex).Trim();
        var valueStr = pair.Substring(eqIndex + 1).Trim();
        var value = ParseValueExpression(valueStr);
        
        assignments.Add(new UpdateStatement.Assignment
        {
            ColumnName = columnName,
            Value = value
        });
    }

    Expression whereClause = null;
    if (whereClauseStr != null)
    {
        whereClause = ParseWhereExpression(whereClauseStr);
    }

    return new UpdateStatement
    {
        TableName = tableName,
        Assignments = assignments,
        WhereClause = whereClause
    };
}

// Helper method to split SET clause by commas (but not commas inside quotes)
private List<string> ParseSetClause(string setClause)
{
    var pairs = new List<string>();
    var currentPair = new System.Text.StringBuilder();
    bool inQuotes = false;
    char quoteChar = '\0';

    for (int i = 0; i < setClause.Length; i++)
    {
        char c = setClause[i];

        if ((c == '\'' || c == '"') && (i == 0 || setClause[i - 1] != '\\'))
        {
            if (!inQuotes)
            {
                inQuotes = true;
                quoteChar = c;
            }
            else if (c == quoteChar)
            {
                inQuotes = false;
            }
        }

        if (c == ',' && !inQuotes)
        {
            // End of current pair
            var pair = currentPair.ToString().Trim();
            if (!string.IsNullOrEmpty(pair))
            {
                pairs.Add(pair);
            }
            currentPair.Clear();
        }
        else
        {
            currentPair.Append(c);
        }
    }

    // Add last pair
    var lastPair = currentPair.ToString().Trim();
    if (!string.IsNullOrEmpty(lastPair))
    {
        pairs.Add(lastPair);
    }

    return pairs;
}


        private DeleteStatement ParseDeleteStatement(string sql)
        {
            // DELETE FROM users WHERE id = 1
            var match = Regex.Match(sql, @"DELETE FROM (\w+)(?:\s+WHERE\s+(.+))?", RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new Exception("Invalid DELETE syntax");

            var tableName = match.Groups[1].Value;
            var whereClauseStr = match.Groups[2].Success ? match.Groups[2].Value : null;

            Expression whereClause = null;
            if (whereClauseStr != null)
            {
                whereClause = ParseWhereExpression(whereClauseStr);
            }

            return new DeleteStatement
            {
                TableName = tableName,
                WhereClause = whereClause
            };
        }

        private object ParseCreateIndexDirect(string sql)
        {
            // CREATE INDEX idx_age ON users(age)
            var match = Regex.Match(sql, @"CREATE INDEX (\w+) ON (\w+)\((.*?)\)", RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new Exception("Invalid CREATE INDEX syntax");

            var indexName = match.Groups[1].Value;
            var tableName = match.Groups[2].Value;
            var columns = match.Groups[3].Value.Split(',').Select(c => c.Trim()).ToList();

            return new SimpleRDBMS.Application.Commands.CreateIndexCommand(indexName, tableName, columns);
        }

        private Expression ParseWhereExpression(string whereStr)
        {
            // Simple WHERE parsing: "column = value" or "column > value"
            var operatorMatch = Regex.Match(whereStr, @"(\w+)\s*([=<>]+)\s*(.+)");
            if (!operatorMatch.Success)
                return null;

            var columnName = operatorMatch.Groups[1].Value;
            var op = operatorMatch.Groups[2].Value;
            var valueStr = operatorMatch.Groups[3].Value.Trim();

            var left = new ColumnReferenceExpression { ColumnName = columnName };
            var right = ParseValueExpression(valueStr);

            return new BinaryExpression
            {
                Left = left,
                Operator = op,
                Right = right
            };
        }

        private List<Expression> ParseValueExpressions(string valuesStr)
        {
            var expressions = new List<Expression>();
            
            // Match quoted strings, numbers, booleans
            var matches = Regex.Matches(valuesStr, @"'[^']*'|""[^""]*""|\d+\.?\d*|true|false|null", RegexOptions.IgnoreCase);
            
            foreach (Match match in matches)
            {
                expressions.Add(ParseValueExpression(match.Value));
            }

            return expressions;
        }

        private Expression ParseValueExpression(string valueStr)
        {
            valueStr = valueStr.Trim();

            // NULL
            if (valueStr.Equals("null", StringComparison.OrdinalIgnoreCase))
                return new LiteralExpression { Value = null };

            // String (quoted)
            if ((valueStr.StartsWith("'") && valueStr.EndsWith("'")) ||
                (valueStr.StartsWith("\"") && valueStr.EndsWith("\"")))
            {
                return new LiteralExpression { Value = valueStr.Trim('\'', '"') };
            }

            // Boolean
            if (bool.TryParse(valueStr, out bool boolVal))
                return new LiteralExpression { Value = boolVal };

            // Decimal
            if (valueStr.Contains(".") && decimal.TryParse(valueStr, out decimal decVal))
                return new LiteralExpression { Value = decVal };

            // Integer
            if (int.TryParse(valueStr, out int intVal))
                return new LiteralExpression { Value = intVal };

            // Default to string
            return new LiteralExpression { Value = valueStr };
        }
    }
}
