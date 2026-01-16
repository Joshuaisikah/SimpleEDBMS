using System;
using System.Linq;
using SimpleRDBMS.Application.Queries;

namespace SimpleRDBMS.CLI
{
    public class OutputFormatter
    {
        public void Format(QueryResult result)
        {
            if (result.Rows.Count == 0)
            {
                Console.WriteLine("No results found.");
                return;
            }

            var columnNames = result.Columns.Select(c => c.Name).ToList();
            var columnWidths = columnNames.ToDictionary(
                name => name,
                name => Math.Max(name.Length, result.Rows.Max(r => 
                    (r.Values[name]?.ToString() ?? "NULL").Length))
            );

            // Print header
            Console.WriteLine(string.Join(" | ", columnNames.Select(n => n.PadRight(columnWidths[n]))));
            Console.WriteLine(new string('-', columnWidths.Values.Sum() + (columnNames.Count - 1) * 3));

            // Print rows
            foreach (var row in result.Rows)
            {
                var values = columnNames.Select(n => 
                    (row.Values[n]?.ToString() ?? "NULL").PadRight(columnWidths[n]));
                Console.WriteLine(string.Join(" | ", values));
            }

            Console.WriteLine($"\n{result.Rows.Count} row(s) returned");
        }
    }
}