using SimpleRDBMS.Domain.Entities;
using SimpleRDBMS.Domain.Exceptions;
using SimpleRDBMS.Domain.Interfaces;

namespace SimpleRDBMS.Application.Services
{
    /// <summary>
    /// Service that validates values against all constraints defined on a column/table.
    /// Used mainly during INSERT and UPDATE operations.
    /// </summary>
    public class ConstraintValidator : IConstraintValidator
    {
        /// <summary>
        /// Validates a single value against all constraints that apply to its column
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="table">The table context (contains constraints)</param>
        /// <exception cref="ConstraintViolationException">Thrown when any constraint is violated</exception>
        public void Validate(object value, Table table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            // For this overload we need to know which column this value belongs to.
            // This version assumes it's called in a context where column is known.
            // For full row validation, use ValidateRow() instead.

            throw new InvalidOperationException(
                "Use Validate(object value, string columnName, Table table) " +
                "or ValidateRow(Row row, Table table) for proper column context.");
        }

        /// <summary>
        /// Validates a value for a specific column against all applicable constraints
        /// </summary>
        public void Validate(object value, string columnName, Table table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name cannot be empty", nameof(columnName));

            // Find all constraints that apply to this column
            var relevantConstraints = table.Constraints
                .Where(c => string.Equals(c.ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!relevantConstraints.Any())
                return; // No constraints → always valid

            var violations = new List<string>();

            foreach (var constraint in relevantConstraints)
            {
                try
                {
                    constraint.Validate(value, table);
                }
                catch (ConstraintViolationException ex)
                {
                    violations.Add(ex.Message);
                }
            }

            if (violations.Any())
            {
                throw new ConstraintViolationException(
                    $"Constraint validation failed for column '{columnName}':\n" +
                    string.Join("\n", violations));
            }
        }

        /// <summary>
        /// Validates an entire row against all column-level constraints
        /// This is the most commonly used method during Insert/Update
        /// </summary>
        public void ValidateRow(Row row, Table table)
        {
            if (row == null)
                throw new ArgumentNullException(nameof(row));
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var violations = new List<string>();

            foreach (var column in table.Columns)
            {
                if (!row.Values.TryGetValue(column.Name, out var value))
                {
                    violations.Add($"Missing value for column '{column.Name}'");
                    continue;
                }

                // Validate this column's value against its constraints
                try
                {
                    Validate(value, column.Name, table);
                }
                catch (ConstraintViolationException ex)
                {
                    violations.Add(ex.Message);
                }
            }

            if (violations.Any())
            {
                throw new ConstraintViolationException(
                    $"Row validation failed:\n{string.Join("\n", violations)}");
            }

            // Future: add table-level / multi-column constraint validation here
        }

        /// <summary>
        /// Quick helper method for NOT NULL check (most common case)
        /// </summary>
        public bool IsNotNullConstraintViolated(object value, string columnName, Table table)
        {
            var hasNotNull = table.Constraints
                .OfType<NotNullConstraint>()
                .Any(c => string.Equals(c.ColumnName, columnName, StringComparison.OrdinalIgnoreCase));

            return hasNotNull && value == null;
        }
    }
}