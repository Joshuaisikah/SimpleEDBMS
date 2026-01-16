using SimpleRDBMS.Application.Interfaces;
using SimpleRDBMS.Application.Services;
using SimpleRDBMS.CLI;
using SimpleRDBMS.Domain.Interfaces;
using SimpleRDBMS.Infrastructure.Persistence.InMemory;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;
using SimpleRDBMS.Parser;

class Program
{
    static void Main()
    {
        // Use the default constructor that initializes everything internally
        var repl = new ReplEngine();
        repl.Run();
    }
}