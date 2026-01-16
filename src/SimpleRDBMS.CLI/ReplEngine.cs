using System;
using SimpleRDBMS.Application.Commands;
using SimpleRDBMS.Application.Interfaces;
using SimpleRDBMS.Application.Queries;
using SimpleRDBMS.Infrastructure.Persistence.InMemory;
using SimpleRDBMS.Application.Services;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;
using SimpleRDBMS.Parser;

namespace SimpleRDBMS.CLI
{
    public class ReplEngine
    {
        private readonly SimpleSqlParser _parser;
        private readonly IQueryExecutor _queryExecutor;
        private readonly ICommandExecutor _commandExecutor;
        private readonly OutputFormatter _formatter;
        private readonly ITableRepository _tableRepository;

        public ReplEngine()
        {
            var repository = new InMemoryTableRepository();
            var indexManager = new InMemoryIndexManager();
            _tableRepository = repository;

            _parser = new SimpleSqlParser(_tableRepository);
            _queryExecutor = new QueryExecutor(repository);
            _commandExecutor = new CommandExecutor(repository, indexManager);
            _formatter = new OutputFormatter();
        }

        public void Run()
        {
            Console.WriteLine("Simple RDBMS - Type 'EXIT' or '.exit' to quit");
            Console.WriteLine("Type '.help' for help\n");

            while (true)
            {
                Console.Write("sql> ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input.Trim().Equals("EXIT", StringComparison.OrdinalIgnoreCase) ||
                    input.Trim().Equals(".exit", StringComparison.OrdinalIgnoreCase))
                    break;

                if (input.Trim().Equals(".help", StringComparison.OrdinalIgnoreCase))
                {
                    ShowHelp();
                    continue;
                }

                try
                {
                    var command = _parser.Parse(input);

                    if (command is SelectQuery selectQuery)
                    {
                        var result = _queryExecutor.Execute(selectQuery);
                        _formatter.Format(result);
                    }
                    else if (command is JoinQuery joinQuery) // ADD THIS!
                    {
                        var result = _queryExecutor.Execute(joinQuery);
                        _formatter.Format(result);
                    }
                    else if (command is CreateTableCommand createCmd)
                    {
                        var result = _commandExecutor.Execute(createCmd);
                        Console.WriteLine(result.Message);
                    }
                    else if (command is InsertCommand insertCmd)
                    {
                        var result = _commandExecutor.Execute(insertCmd);
                        Console.WriteLine(result.Message);
                    }
                    else if (command is UpdateCommand updateCmd)
                    {
                        var result = _commandExecutor.Execute(updateCmd);
                        Console.WriteLine(result.Message);
                    }
                    else if (command is DeleteCommand deleteCmd)
                    {
                        var result = _commandExecutor.Execute(deleteCmd);
                        Console.WriteLine(result.Message);
                    }
                    else if (command is CreateIndexCommand indexCmd)
                    {
                        var result = _commandExecutor.Execute(indexCmd);
                        Console.WriteLine(result.Message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine();
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine(@"
Supported SQL Commands:
  CREATE TABLE table_name (col1 TYPE [constraints], ...)
  INSERT INTO table_name (col1, col2, ...) VALUES (val1, val2, ...)
  SELECT col1, col2, ... FROM table_name [WHERE condition]
  SELECT * FROM table1 JOIN table2 ON table1.col = table2.col
  UPDATE table_name SET col1 = val1 [WHERE condition]
  DELETE FROM table_name [WHERE condition]
  CREATE INDEX index_name ON table_name(column)

Supported Data Types:
  INT, VARCHAR(n), BOOL, DECIMAL, DATE

Constraints:
  PRIMARY KEY, NOT NULL, UNIQUE

Examples:
  CREATE TABLE users (id INT PRIMARY KEY, name VARCHAR(50) NOT NULL, age INT);
  CREATE TABLE orders (id INT PRIMARY KEY, user_id INT, amount DECIMAL);
  INSERT INTO users (id, name, age) VALUES (1, 'Alice', 30);
  INSERT INTO orders (id, user_id, amount) VALUES (1, 1, 99.99);
  SELECT * FROM users WHERE age > 25;
  SELECT * FROM users JOIN orders ON users.id = orders.user_id;
  UPDATE users SET age = 31 WHERE id = 1;
  DELETE FROM users WHERE id = 1;
");
        }
    }
}
