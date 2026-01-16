using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRDBMS.Domain.Exceptions;
using SimpleRDBMS.Domain.Interfaces;
using SimpleRDBMS.Domain.Entities;

namespace SimpleRDBMS.Domain.Entities
{
    public abstract class Constraint : IConstraintValidator
    {
        public string ColumnName { get; protected set; }
        public ConstraintType Type { get; protected set; }
        public string Name { get; protected set; }  // Optional constraint name (e.g. "PK_Users_Id")

        protected Constraint(string columnName, ConstraintType type, string name = null)
        {
            ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
            Type = type;
            Name = name;
        }

        public abstract void Validate(object value, Table table);
    }

    public enum ConstraintType
    {
        PrimaryKey,
        Unique,
        NotNull,
        Check,
        ForeignKey     // Basic support - full FK enforcement needs more infrastructure
    }

    
    public class NotNullConstraint : Constraint
    {
        public NotNullConstraint(string columnName, string name = null)
            : base(columnName, ConstraintType.NotNull, name ?? $"NN_{columnName}")
        {
        }

        public override void Validate(object value, Table table)
        {
            if (value == null)
            {
                throw new ConstraintViolationException(
                    $"NOT NULL constraint violated on column '{ColumnName}' " +
                    $"(constraint: {Name})");
            }
        }
    }

    /// <summary>
    /// UNIQUE constraint (single column)
    /// </summary>
    public class UniqueConstraint : Constraint
    {
        public UniqueConstraint(string columnName, string name = null)
            : base(columnName, ConstraintType.Unique, name ?? $"UQ_{columnName}")
        {
        }

        public override void Validate(object value, Table table)
        {
            if (value == null) return; // NULL is allowed in UNIQUE unless NOT NULL is also present

            var existingRows = table.GetRows()
                .Where(r => r.Values.TryGetValue(ColumnName, out var existing) &&
                           existing?.Equals(value) == true);

            // If this is a new row → count should be 0
            // If this is an update → we should exclude current row (but we don't have current row id here)
            // → Simplest realistic approach for in-memory: we check after insertion attempt
            //   (real DBs use indexes + deferred checking)

            // For strict immediate check we can only do this safely for new rows:
            if (existingRows.Any())
            {
                throw new ConstraintViolationException(
                    $"UNIQUE constraint violated on column '{ColumnName}' " +
                    $"with value '{value}' (constraint: {Name})");
            }
        }
    }

    /// <summary>
    /// PRIMARY KEY constraint (single column version)
    /// Usually implies NOT NULL + UNIQUE
    /// </summary>
    public class PrimaryKeyConstraint : Constraint
    {
        public PrimaryKeyConstraint(string columnName, string name = null)
            : base(columnName, ConstraintType.PrimaryKey, name ?? $"PK_{columnName}")
        {
        }

        public override void Validate(object value, Table table)
        {
            if (value == null)
            {
                throw new ConstraintViolationException(
                    $"PRIMARY KEY cannot be NULL on column '{ColumnName}' " +
                    $"(constraint: {Name})");
            }

            var existing = table.GetRows()
                .Where(r => r.Values.TryGetValue(ColumnName, out var v) &&
                           v?.Equals(value) == true);

            if (existing.Any())
            {
                throw new ConstraintViolationException(
                    $"PRIMARY KEY violation on column '{ColumnName}' - duplicate value '{value}' " +
                    $"(constraint: {Name})");
            }
        }
    }

    /// <summary>
    /// Very simple CHECK constraint (only equality and IS NOT NULL supported in this basic version)
    /// </summary>
    public class CheckConstraint : Constraint
    {
        public string ConditionExpression { get; }   // Very simplified - real parser would be needed
        private readonly Func<object, bool> _checkFunc;

        public CheckConstraint(string columnName, string conditionExpression, Func<object, bool> checkFunc, string name = null)
            : base(columnName, ConstraintType.Check, name ?? $"CK_{columnName}")
        {
            ConditionExpression = conditionExpression;
            _checkFunc = checkFunc ?? throw new ArgumentNullException(nameof(checkFunc));
        }

        public override void Validate(object value, Table table)
        {
            if (!_checkFunc(value))
            {
                throw new ConstraintViolationException(
                    $"CHECK constraint violated on column '{ColumnName}': {ConditionExpression} " +
                    $"for value '{value}' (constraint: {Name})");
            }
        }

        // Factory helpers for common cases
        public static CheckConstraint PositiveNumber(string columnName)
            => new CheckConstraint(columnName, $"{columnName} > 0",
                v => v is IComparable c && c.CompareTo(0) > 0);

        public static CheckConstraint NotEmptyString(string columnName)
            => new CheckConstraint(columnName, $"{columnName} != ''",
                v => v is string s && !string.IsNullOrEmpty(s));
    }

    // ──────────────────────────────────────────────────────────────────────────────
    //                  Foreign Key (very basic skeleton)
    // ──────────────────────────────────────────────────────────────────────────────
    // Note: Full FK support requires:
    // 1. Reference to parent table
    // 2. Reference column
    // 3. ON DELETE/UPDATE actions
    // For MVP we just check existence

    public class ForeignKeyConstraint : Constraint
    {
        public string ReferencedTableName { get; }
        public string ReferencedColumnName { get; }

        private readonly Table _parentTable; // Should be resolved at validation time in real system

        public ForeignKeyConstraint(
            string columnName,
            string referencedTable,
            string referencedColumn,
            string name = null,
            Table parentTable = null) // ← normally resolved dynamically
            : base(columnName, ConstraintType.ForeignKey,
                   name ?? $"FK_{columnName}_to_{referencedTable}_{referencedColumn}")
        {
            ReferencedTableName = referencedTable;
            ReferencedColumnName = referencedColumn;
            _parentTable = parentTable; // for MVP - in real system use repository
        }

        public override void Validate(object value, Table table)
        {
            if (value == null) return; // allow NULL unless NOT NULL is also present

            // Very naive implementation - real system would use index
            if (_parentTable == null)
            {
                throw new NotSupportedException("Foreign key validation requires parent table reference (MVP limitation)");
            }

            bool exists = _parentTable.GetRows()
                .Any(r => r.Values.TryGetValue(ReferencedColumnName, out var refValue) &&
                         refValue?.Equals(value) == true);

            if (!exists)
            {
                throw new ConstraintViolationException(
                    $"FOREIGN KEY constraint violation on column '{ColumnName}' " +
                    $"referencing {ReferencedTableName}.{ReferencedColumnName} - value '{value}' not found " +
                    $"(constraint: {Name})");
            }
        }
    }
}