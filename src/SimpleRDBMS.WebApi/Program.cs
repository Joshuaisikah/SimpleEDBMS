using SimpleRDBMS.Application.Interfaces;
using SimpleRDBMS.Application.Services;
using SimpleRDBMS.Infrastructure.Persistence.InMemory;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;
using SimpleRDBMS.Parser;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register RDBMS services
builder.Services.AddSingleton<ITableRepository, InMemoryTableRepository>();
builder.Services.AddSingleton<IIndexManager, InMemoryIndexManager>();
builder.Services.AddSingleton<IQueryExecutor, QueryExecutor>();
builder.Services.AddSingleton<ICommandExecutor, CommandExecutor>();
builder.Services.AddSingleton<SimpleSqlParser>();

var app = builder.Build();

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseDefaultFiles(); // Serve index.html by default
app.UseStaticFiles();
app.MapControllers();

// Initialize database
InitializeDatabase(app.Services);

app.Run();

void InitializeDatabase(IServiceProvider services)
{
    var parser = services.GetRequiredService<SimpleSqlParser>();
    var commandExecutor = services.GetRequiredService<ICommandExecutor>();

    // FIXED: Single line SQL
    var createTableSql = "CREATE TABLE tasks (id INT PRIMARY KEY, title VARCHAR(200) NOT NULL, description VARCHAR(500), completed INT, priority INT)";

    try
    {
        var command = parser.Parse(createTableSql);
        if (command is SimpleRDBMS.Application.Commands.CreateTableCommand createCmd)
        {
            var result = commandExecutor.Execute(createCmd);
            if (result.Success)
            {
                Console.WriteLine("✅ Database initialized - 'tasks' table created");
            }
            else
            {
                Console.WriteLine($"⚠️ Database initialization failed: {result.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Database initialization error: {ex.Message}");
    }
}