# Simple RDBMS - Pesapal Junior Dev Challenge '26

A lightweight, in-memory relational database management system built from scratch in C# with Clean Architecture principles.

![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)
![Status](https://img.shields.io/badge/status-complete-success)

## 🎯 Project Overview

This project is my submission for the **Pesapal Junior Developer Challenge 2026**. It demonstrates the implementation of a fully functional RDBMS from scratch, including:

- ✅ **Complete SQL Parser** - Tokenizes and parses SQL statements into Abstract Syntax Trees
- ✅ **CRUD Operations** - CREATE, INSERT, SELECT, UPDATE, DELETE fully working
- ✅ **Data Types** - INT, VARCHAR(n), BOOL, DECIMAL support
- ✅ **Constraints** - PRIMARY KEY, UNIQUE, NOT NULL enforcement
- ✅ **Indexing** - Hash-based indexes for fast lookups
- ✅ **JOIN Operations** - Inner join logic implemented
- ✅ **Interactive REPL** - Command-line SQL interface
- ✅ **Web Demo** - ASP.NET Core task management application

---

## 🏗️ Architecture & Design Approach

### Clean Architecture Implementation

I chose **Clean Architecture** to ensure separation of concerns and maintainability:

```
┌─────────────────────────────────────────────────────┐
│                  Presentation Layer                  │
│              (CLI, Web API, Controllers)             │
└────────────────────┬────────────────────────────────┘
                     │ depends on
┌────────────────────▼────────────────────────────────┐
│                   Parser Layer                       │
│        (SQL Lexer, Parser, AST, Visitors)           │
└────────────────────┬────────────────────────────────┘
                     │ depends on
┌────────────────────▼────────────────────────────────┐
│               Infrastructure Layer                   │
│    (In-Memory Storage, Indexes, Query Execution)    │
└────────────────────┬────────────────────────────────┘
                     │ depends on
┌────────────────────▼────────────────────────────────┐
│                Application Layer                     │
│       (Commands, Queries, Services, DTOs)           │
└────────────────────┬────────────────────────────────┘
                     │ depends on
┌────────────────────▼────────────────────────────────┐
│                  Domain Layer                        │
│    (Entities, Value Objects, Business Rules)        │
│              NO EXTERNAL DEPENDENCIES                │
└─────────────────────────────────────────────────────┘
```

### Key Design Decisions

**1. Visitor Pattern for AST**
- SQL is parsed into an Abstract Syntax Tree
- `CommandBuilder` visitor converts AST to application commands
- Enables easy extension with new SQL features

**2. Repository Pattern for Data Access**
- `ITableRepository` abstracts storage implementation
- Easy to swap in-memory storage for disk-based storage
- Testable and mockable

**3. Command/Query Separation (CQS)**
- Write operations: `CreateTableCommand`, `InsertCommand`, `UpdateCommand`, `DeleteCommand`
- Read operations: `SelectQuery`, `JoinQuery`
- Clear separation of concerns

**4. Hash-Based Indexing**
- O(1) lookups for equality searches on indexed columns
- Automatically created for PRIMARY KEY and UNIQUE constraints
- Manual index creation supported via `CREATE INDEX`

**5. In-Memory Storage with Concurrency Safety**
- `ConcurrentDictionary` for thread-safe table storage
- Ideal for MVP and demonstration purposes
- Foundation ready for disk persistence layer

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.net/download/dotnet/8.0) or later

### Clone the Repository
```bash
git clone https://github.com/Joshuaisikah/SimpleEDBMS.git
cd SimpleEDBMS
```

### Build the Solution
```bash
dotnet build
```

---

## 💻 Running the Interactive REPL

The REPL (Read-Eval-Print Loop) provides an interactive SQL command-line interface.

### Start the REPL
```bash
cd src/SimpleRDBMS.CLI
dotnet run
```

You should see:
```
Simple RDBMS - Type 'EXIT' or '.exit' to quit
Type '.help' for help

sql>
```

### Example Session
```sql
-- Create a table
CREATE TABLE users (id INT PRIMARY KEY, name VARCHAR(50) NOT NULL, age INT);

-- Insert data
INSERT INTO users (id, name, age) VALUES (1, 'Alice', 30);
INSERT INTO users (id, name, age) VALUES (2, 'Bob', 25);
INSERT INTO users (id, name, age) VALUES (3, 'Charlie', 35);

-- Query all users
SELECT * FROM users;

-- Query with WHERE clause
SELECT * FROM users WHERE age > 26;

-- Update a user
UPDATE users SET age = 31 WHERE id = 1;

-- Delete a user
DELETE FROM users WHERE id = 2;

-- Create an index for faster queries
CREATE INDEX idx_age ON users(age);

-- Exit
.exit
```

### Available SQL Commands

#### Data Definition Language (DDL)
```sql
CREATE TABLE table_name (
    column1 TYPE [PRIMARY KEY] [NOT NULL] [UNIQUE],
    column2 TYPE,
    ...
);

CREATE INDEX index_name ON table_name(column_name);
```

#### Data Manipulation Language (DML)
```sql
-- INSERT (with or without column names)
INSERT INTO table_name (col1, col2) VALUES (val1, val2);
INSERT INTO table_name VALUES (val1, val2, val3);

-- SELECT
SELECT * FROM table_name;
SELECT col1, col2 FROM table_name WHERE condition;

-- UPDATE
UPDATE table_name SET col1 = val1, col2 = val2 WHERE condition;

-- DELETE
DELETE FROM table_name WHERE condition;
```

#### Supported Data Types
- `INT` - Integer numbers
- `VARCHAR(n)` - Variable-length strings (max length n)
- `BOOL` - Boolean (true/false)
- `DECIMAL` - Decimal numbers

#### Supported Constraints
- `PRIMARY KEY` - Unique identifier, not null, auto-indexed
- `UNIQUE` - Unique values, auto-indexed
- `NOT NULL` - Cannot be null

#### WHERE Clause Operators
- `=` - Equality
- `>` - Greater than
- `<` - Less than
- `>=` - Greater than or equal
- `<=` - Less than or equal
- `!=` - Not equal

---

## 🌐 Running the Web Application

The web demo is a task management application that demonstrates all CRUD operations.

### Start the Web Server
```bash
cd src/SimpleRDBMS.WebApi
dotnet run
```

You should see:
```
✅ Database initialized - 'tasks' table created
Now listening on: http://localhost:5154
```

### Access the Application

#### Web Interface
Open your browser and navigate to:
```
http://localhost:5154
```

You'll see a beautiful task manager where you can:
- ✅ **Create** tasks
- ✅ **Read** all tasks or filter by status
- ✅ **Update** tasks (mark complete, change priority)
- ✅ **Delete** tasks

#### Swagger API Documentation
Explore the REST API at:
```
http://localhost:5154/swagger
```

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/tasks` | Get all tasks |
| `GET` | `/api/tasks?completed=true` | Get completed tasks |
| `GET` | `/api/tasks/{id}` | Get task by ID |
| `POST` | `/api/tasks` | Create new task |
| `PUT` | `/api/tasks/{id}` | Update task |
| `DELETE` | `/api/tasks/{id}` | Delete task |
| `GET` | `/api/tasks/stats` | Get task statistics |

### Example API Usage

**Create a Task:**
```bash
curl -X POST http://localhost:5154/api/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Learn RDBMS internals",
    "description": "Build a database from scratch",
    "priority": 3
  }'
```

**Get All Tasks:**
```bash
curl http://localhost:5154/api/tasks
```

**Update a Task:**
```bash
curl -X PUT http://localhost:5154/api/tasks/1 \
  -H "Content-Type: application/json" \
  -d '{ "completed": true }'
```

**Delete a Task:**
```bash
curl -X DELETE http://localhost:5154/api/tasks/1
```

---

## 🔧 Technical Implementation Details

### SQL Parsing Strategy

**Lexer → Parser → AST → Command/Query**

1. **Lexer** (`SqlLexer`): Tokenizes SQL strings using regex patterns
2. **Parser** (`SimpleSqlParser`): Builds Abstract Syntax Tree (AST) from tokens
3. **Visitor** (`CommandBuilder`): Converts AST to application-layer commands
4. **Executor**: Executes commands against the repository

Example flow for `INSERT INTO users VALUES (1, 'Alice', 30)`:
```
SQL String
    ↓
Lexer (regex tokenization)
    ↓
Parser (creates InsertStatement AST)
    ↓
CommandBuilder.Visit(InsertStatement)
    ↓
InsertCommand created
    ↓
CommandExecutor.Execute(InsertCommand)
    ↓
Table.AddRow() → validates constraints
    ↓
Row added to in-memory storage
```

### Constraint Validation

Constraints are validated at multiple levels:

1. **Domain Layer** - `Table.ValidateRow()` checks constraints before adding
2. **Application Layer** - `ConstraintValidator` orchestrates validation
3. **Infrastructure Layer** - Indexes enforce uniqueness

**Primary Key Enforcement:**
```csharp
public class PrimaryKeyConstraint : Constraint
{
    public override void Validate(object value, Table table)
    {
        if (value == null)
            throw new ConstraintViolationException("Primary key cannot be null");
        
        if (table.GetRows().Any(r => r.Values[ColumnName]?.Equals(value) == true))
            throw new ConstraintViolationException("Primary key violation");
    }
}
```

### Indexing Implementation

**Hash Index** for O(1) lookups:
```csharp
public class HashIndex : Index
{
    private Dictionary<object, List<Row>> _index = new();
    
    public override IEnumerable<Row> Search(object key)
    {
        return _index.TryGetValue(key, out var rows) 
            ? rows 
            : Enumerable.Empty<Row>();
    }
}
```

**Automatic Index Creation:**
- PRIMARY KEY columns → Hash index created automatically
- UNIQUE columns → Hash index created automatically
- Manual: `CREATE INDEX idx_name ON table(column)` → Hash index created

### JOIN Implementation

JOIN operations are fully implemented in `QueryExecutor`:

```csharp
public QueryResult Execute(JoinQuery query)
{
    var leftResult = Execute(query.LeftQuery);
    var rightResult = Execute(query.RightQuery);
    
    // Parse join condition: "table1.col = table2.col"
    var joinParts = query.JoinCondition.Split('=');
    var leftCol = joinParts[0].Trim().Split('.')[1];
    var rightCol = joinParts[1].Trim().Split('.')[1];
    
    // Nested loop join (simplified for MVP)
    var joinedRows = new List<Row>();
    foreach (var leftRow in leftResult.Rows)
    {
        foreach (var rightRow in rightResult.Rows)
        {
            if (leftRow.Values[leftCol]?.Equals(rightRow.Values[rightCol]) == true)
            {
                // Merge rows with qualified column names
                var merged = new Dictionary<string, object>();
                foreach (var kvp in leftRow.Values)
                    merged[$"{leftTable}.{kvp.Key}"] = kvp.Value;
                foreach (var kvp in rightRow.Values)
                    merged[$"{rightTable}.{kvp.Key}"] = kvp.Value;
                
                joinedRows.Add(new Row(merged));
            }
        }
    }
    
    return new QueryResult(columns, joinedRows);
}
```

**Current Status:**
- ✅ INNER JOIN logic fully implemented
- ✅ LEFT, RIGHT, FULL OUTER join types defined
- ⚠️ SQL parsing for JOIN is basic (demonstrates concept)
- ✅ Programmatic JOIN calls work perfectly

**Example (programmatic):**
```csharp
var leftQuery = new SelectQuery(new List<string> { "*" }, "users", "");
var rightQuery = new SelectQuery(new List<string> { "*" }, "orders", "");
var joinQuery = new JoinQuery(leftQuery, rightQuery, JoinType.Inner, 
    "users.id = orders.user_id");
var result = queryExecutor.Execute(joinQuery);
```

---

## 📊 Project Structure

```
SimpleRDBMS/
├── SimpleRDBMS.sln
├── README.md
├── .gitignore
├── src/
│   ├── SimpleRDBMS.Domain/              # Core business logic (no dependencies)
│   │   ├── Entities/
│   │   │   ├── Table.cs                 # Table entity with validation
│   │   │   ├── Column.cs                # Column definition
│   │   │   ├── Row.cs                   # Type-safe row wrapper
│   │   │   ├── Index.cs                 # Abstract index base class
│   │   │   └── Constraint.cs            # Constraint validators
│   │   ├── ValueObjects/
│   │   │   ├── DataType.cs              # Supported data types enum
│   │   │   ├── ColumnDefinition.cs      # Column metadata
│   │   │   └── IndexKey.cs              # Composite index key
│   │   ├── Interfaces/
│   │   │   ├── ITable.cs
│   │   │   ├── IIndex.cs
│   │   │   └── IConstraintValidator.cs
│   │   └── Exceptions/
│   │       ├── TableNotFoundException.cs
│   │       ├── ConstraintViolationException.cs
│   │       └── InvalidDataTypeException.cs
│   │
│   ├── SimpleRDBMS.Application/          # Use cases & business rules
│   │   ├── Interfaces/
│   │   │   ├── IQueryExecutor.cs         # Query execution contract
│   │   │   ├── ICommandExecutor.cs       # Command execution contract
│   │   │   └── ITransactionManager.cs
│   │   ├── Commands/                     # Write operations
│   │   │   ├── CreateTableCommand.cs
│   │   │   ├── InsertCommand.cs
│   │   │   ├── UpdateCommand.cs
│   │   │   ├── DeleteCommand.cs
│   │   │   └── CreateIndexCommand.cs
│   │   ├── Queries/                      # Read operations
│   │   │   ├── SelectQuery.cs
│   │   │   ├── JoinQuery.cs
│   │   │   └── QueryResult.cs
│   │   ├── Services/
│   │   │   ├── QueryExecutor.cs          # Executes SELECT & JOIN
│   │   │   ├── CommandExecutor.cs        # Executes INSERT/UPDATE/DELETE
│   │   │   └── ConstraintValidator.cs
│   │   └── DTOs/
│   │       ├── TableDto.cs
│   │       ├── QueryResultDto.cs
│   │       └── ExecutionResultDto.cs
│   │
│   ├── SimpleRDBMS.Infrastructure/       # External implementations
│   │   ├── Persistence/
│   │   │   ├── InMemory/
│   │   │   │   ├── InMemoryTableRepository.cs   # Thread-safe storage
│   │   │   │   └── InMemoryIndexManager.cs
│   │   │   └── Interfaces/
│   │   │       ├── ITableRepository.cs
│   │   │       └── IIndexManager.cs
│   │   └── Indexing/
│   │       ├── HashIndex.cs              # O(1) hash-based index
│   │       ├── BTreeIndex.cs             # B-tree for range queries
│   │       └── IndexFactory.cs
│   │
│   ├── SimpleRDBMS.Parser/               # SQL parsing
│   │   ├── SimpleSqlParser.cs            # Main parser entry point
│   │   ├── AST/                          # Abstract Syntax Tree nodes
│   │   │   ├── Statement.cs
│   │   │   ├── CreateTableStatement.cs
│   │   │   ├── InsertStatement.cs
│   │   │   ├── SelectStatement.cs
│   │   │   ├── UpdateStatement.cs
│   │   │   ├── DeleteStatement.cs
│   │   │   ├── Expression.cs
│   │   │   ├── LiteralExpression.cs
│   │   │   ├── BinaryExpression.cs
│   │   │   ├── ColumnReferenceExpression.cs
│   │   │   └── ColumnDefinitionAst.cs
│   │   └── Visitors/
│   │       ├── IStatementVisitor.cs
│   │       └── CommandBuilder.cs         # AST → Command converter
│   │
│   ├── SimpleRDBMS.CLI/                  # Interactive REPL
│   │   ├── Program.cs
│   │   ├── ReplEngine.cs                 # Read-Eval-Print loop
│   │   └── OutputFormatter.cs            # ASCII table formatter
│   │
│   └── SimpleRDBMS.WebApi/               # Web demonstration
│       ├── Program.cs                     # API configuration
│       ├── Controllers/
│       │   └── TasksController.cs        # REST API endpoints
│       └── wwwroot/
│           └── index.html                # Task manager UI
```

---

## 🎓 What I Learned

### Technical Skills Developed

1. **Database Internals Understanding**
   - How indexes work (hash vs B-tree tradeoffs)
   - Constraint enforcement at multiple layers
   - Query execution and optimization basics
   - ACID properties and transaction concepts

2. **Parser Design & Implementation**
   - Lexical analysis and tokenization
   - Recursive descent parsing
   - Abstract Syntax Tree (AST) construction
   - Visitor pattern for AST traversal

3. **Clean Architecture in Practice**
   - Dependency inversion principle
   - Domain-driven design (DDD)
   - Repository and Command patterns
   - Separation of concerns

4. **Advanced C# Concepts**
   - Concurrent data structures (`ConcurrentDictionary`)
   - Reflection for dynamic type handling
   - Extension methods and LINQ
   - Nullable reference types

### Design Pattern Applications

- **Visitor Pattern** - AST traversal and command building
- **Repository Pattern** - Data access abstraction
- **Command Pattern** - Encapsulating operations
- **Factory Pattern** - Index creation
- **Strategy Pattern** - Different join algorithms
- **Template Method** - Abstract base classes (Statement, Expression)

### Problem-Solving Approaches

**Challenge: WHERE Clause Parsing**
- **Initial approach**: String.Split() - fragile, error-prone
- **Solution**: Regex pattern matching for robust parsing
- **Learning**: Always validate input and handle edge cases

**Challenge: Type Conversion**
- **Problem**: User inputs strings, database needs typed values
- **Solution**: ParseValue() with multiple type attempts
- **Learning**: Graceful degradation and type coercion

**Challenge: Constraint Validation**
- **Problem**: Multiple validation layers needed
- **Solution**: Domain validates business rules, Application orchestrates
- **Learning**: Single Responsibility Principle in action

---

## 🚧 Known Limitations & Future Enhancements

### Current Limitations

1. **In-Memory Only** - No disk persistence (data lost on restart)
2. **No Transactions** - No ACID guarantees or rollback support
3. **Basic JOIN SQL Parsing** - Join logic works, SQL parsing simplified
4. **Sequential Scans** - No query optimization for complex queries
5. **Single-Threaded Execution** - No concurrent query execution
6. **Limited SQL Standard** - Subset of SQL features implemented

### Future Enhancements

**Phase 1: Persistence**
- [ ] Page-based disk storage
- [ ] Write-Ahead Logging (WAL)
- [ ] Crash recovery mechanism
- [ ] Buffer pool with LRU eviction

**Phase 2: Advanced Querying**
- [ ] Full JOIN SQL parsing (LEFT, RIGHT, OUTER)
- [ ] Aggregate functions (COUNT, SUM, AVG, MIN, MAX)
- [ ] GROUP BY and HAVING clauses
- [ ] ORDER BY and LIMIT
- [ ] Subqueries and correlated queries

**Phase 3: Optimization**
- [ ] Query optimizer with cost estimation
- [ ] B+ Tree indexes for range queries
- [ ] Statistics collection for cardinality estimation
- [ ] Join reordering and predicate pushdown

**Phase 4: Transactions**
- [ ] BEGIN/COMMIT/ROLLBACK support
- [ ] Multi-version concurrency control (MVCC)
- [ ] Isolation levels (Read Committed, Repeatable Read, Serializable)
- [ ] Deadlock detection

**Phase 5: Advanced Features**
- [ ] Views and stored procedures
- [ ] Triggers
- [ ] Foreign key constraints
- [ ] Full-text search
- [ ] JSON data type support

---

## 🙏 Credits & Acknowledgments

### AI Assistance

This project was developed with assistance from **Claude AI (Anthropic)** for:
- Initial architecture design and clean architecture guidance
- SQL parser implementation patterns and regex strategies
- Code structure templates and boilerplate generation
- Debugging assistance and optimization suggestions
- Best practices recommendations

### My Contributions

**All code was reviewed, understood, and adapted by me.** I made the following key decisions:

1. **Architecture Choice** - Decided on Clean Architecture for maintainability
2. **In-Memory Storage** - Chose `ConcurrentDictionary` for thread-safety
3. **Parsing Strategy** - Selected regex-based parsing for MVP speed
4. **Constraint Design** - Implemented multi-layer validation approach
5. **Web Demo Choice** - Task manager as practical CRUD demonstration
6. **Error Handling** - Added comprehensive exception handling throughout
7. **Index Strategy** - Hash indexes for O(1) lookups on equality

**I can explain:**
- How the SQL parser tokenizes and builds AST
- Why indexes improve query performance
- How constraints are validated across layers
- The flow of data from SQL → Command → Repository → Storage
- Tradeoffs between different join algorithms
- Why Clean Architecture benefits this codebase

### Learning Resources

- **Database System Concepts** by Silberschatz, Korth, and Sudarshan
- **Clean Architecture** by Robert C. Martin
- **SQLite Architecture Documentation** - Excellent resource on database internals
- **Microsoft C# Documentation** - Language features and best practices
- **PostgreSQL Internals** - Query planning and optimization insights

---

## 📝 Development Notes

### Time Investment
- **Project Setup & Architecture**: 3 hours
- **Domain & Application Layers**: 4 hours
- **SQL Parser Implementation**: 5 hours
- **Infrastructure & Storage**: 3 hours
- **CLI REPL**: 2 hours
- **Web API & Demo**: 3 hours
- **Testing & Debugging**: 4 hours
- **Documentation**: 2 hours
- **Total**: ~26 hours over 3 days

### Challenges Overcome

1. **SQL Parsing Complexity** - Initially tried string splitting, moved to regex
2. **WHERE Clause Array Index Errors** - Fixed with proper regex groups
3. **Type Conversion Issues** - Implemented flexible ParseValue() method
4. **Constraint Validation Order** - Separated domain rules from application logic
5. **INSERT Without Columns** - Added support for schema-based column inference

### Testing Approach

While formal unit tests were removed for submission simplicity, the project was thoroughly tested through:

- **Manual REPL Testing** - All SQL commands tested interactively
- **Web API Testing** - Full CRUD cycle verified via browser and Swagger
- **Edge Case Testing** - Null values, type mismatches, constraint violations
- **Integration Testing** - End-to-end flows from SQL to storage

---

## 📄 License

MIT License - This project is open source and available for learning purposes.

---

## 🚀 Submission Details

**Challenge**: Pesapal Junior Dev Challenge '26  
**Submission Date**: January 16, 2026  
**Repository**: [https://github.com/YOUR_USERNAME/pesapal-rdbms-challenge](https://github.com/YOUR_USERNAME/pesapal-rdbms-challenge)

---

**Built with ❤️ and lots of ☕ in Nairobi, Kenya**
