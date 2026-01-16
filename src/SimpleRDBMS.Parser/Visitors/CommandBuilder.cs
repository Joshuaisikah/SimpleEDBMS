using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRDBMS.Application.Commands;
using SimpleRDBMS.Application.Queries;
using SimpleRDBMS.Domain.Entities;
using SimpleRDBMS.Domain.Exceptions;
using SimpleRDBMS.Domain.ValueObjects;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;
using SimpleRDBMS.Parser.AST;

namespace SimpleRDBMS.Parser.Visitors
{
    public class CommandBuilder : IStatementVisitor
    {
        private object _result;
        public object Result => _result;

        private readonly ITableRepository _tableRepository;

        public CommandBuilder(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository ?? throw new ArgumentNullException(nameof(tableRepository));
        }

        public void Visit(InsertStatement stmt)
        {
            var table = _tableRepository.GetTable(stmt.TableName);
            if (table == null)
                throw new TableNotFoundException(stmt.TableName);

            // Map values to columns
            var rowData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < stmt.Values.Count; i++)
            {
                if (i >= stmt.Columns.Count)
                    throw new InvalidOperationException($"Value at position {i} has no corresponding column");

                var columnName = stmt.Columns[i];
                var valueExpr = stmt.Values[i];
                var value = ExtractValue(valueExpr);

                // Find the column definition
                var column = table.Columns.FirstOrDefault(c => 
                    c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                if (column == null)
                    throw new InvalidOperationException($"Column '{columnName}' not found in table '{stmt.TableName}'");

                // Type validation and conversion
                if (value != null && !column.DataType.IsValid(value))
                {
                    // Try to convert
                    value = ConvertValue(value, column.DataType);
                }

                rowData[columnName] = value;
            }

            var row = new Row(rowData);
            _result = new InsertCommand(_tableRepository, stmt.TableName, row);
        }

        public void Visit(SelectStatement stmt)
        {
            string whereClause = null;
            
            if (stmt.WhereClause is BinaryExpression binExpr)
            {
                if (binExpr.Left is ColumnReferenceExpression col && 
                    binExpr.Right is LiteralExpression lit)
                {
                    whereClause = $"{col.ColumnName} {binExpr.Operator} {lit.Value}";
                }
            }

            _result = new SelectQuery(
                columns: stmt.Columns,
                tableName: stmt.TableName,
                whereClause: whereClause
            );
        }

        public void Visit(CreateTableStatement stmt)
        {
            var columnDefs = stmt.Columns.Select(c => new ColumnDefinition
            {
                Name = c.Name,
                DataType = c.DataType,
                IsNullable = !c.IsNotNull && !c.IsPrimaryKey,
                DefaultValue = null
            }).ToList();

            _result = new CreateTableCommand(
                tableName: stmt.TableName,
                columns: columnDefs
            );
        }

        public void Visit(UpdateStatement stmt)
        {
            var setValues = new Dictionary<string, object>();

            foreach (var assignment in stmt.Assignments)
            {
                if (assignment.Value is LiteralExpression lit)
                {
                    setValues[assignment.ColumnName] = lit.Value;
                }
                else
                {
                    throw new NotSupportedException("UPDATE currently supports only literal values");
                }
            }

            string whereClause = null;
            if (stmt.WhereClause is BinaryExpression bin)
            {
                if (bin.Left is ColumnReferenceExpression col && 
                    bin.Right is LiteralExpression lit)
                {
                    whereClause = $"{col.ColumnName} {bin.Operator} {lit.Value}";
                }
            }

            _result = new UpdateCommand(
                tableName: stmt.TableName,
                setValues: setValues,
                whereClause: whereClause
            );
        }

        public void Visit(DeleteStatement stmt)
        {
            string whereClause = null;
            if (stmt.WhereClause is BinaryExpression bin)
            {
                if (bin.Left is ColumnReferenceExpression col && 
                    bin.Right is LiteralExpression lit)
                {
                    whereClause = $"{col.ColumnName} {bin.Operator} {lit.Value}";
                }
            }

            _result = new DeleteCommand(
                tableName: stmt.TableName,
                whereClause: whereClause
            );
        }

        // ====================================================================
        // HELPER METHODS
        // ====================================================================

        private object ExtractValue(Expression expr)
        {
            if (expr is LiteralExpression lit)
                return lit.Value;

            if (expr == null)
                return null;

            throw new NotSupportedException($"Unsupported expression type: {expr.GetType().Name}");
        }

        private object ConvertValue(object value, DataType dataType)
        {
            try
            {
                return dataType.Type switch
                {
                    DataTypeEnum.INT when value is decimal dec => (int)dec,
                    DataTypeEnum.INT when value is double dbl => (int)dbl,
                    DataTypeEnum.INT when value is float flt => (int)flt,
                    DataTypeEnum.DECIMAL when value is int i => (decimal)i,
                    DataTypeEnum.DECIMAL when value is double dbl => (decimal)dbl,
                    DataTypeEnum.DECIMAL when value is float flt => (decimal)flt,
                    DataTypeEnum.VARCHAR => value.ToString(),
                    DataTypeEnum.BOOL when value is int i => i != 0,
                    DataTypeEnum.BOOL when value is string s => bool.Parse(s),
                    _ => value
                };
            }
            catch (Exception ex)
            {
                throw new InvalidDataTypeException(
                    $"Cannot convert value '{value}' ({value.GetType().Name}) to type {dataType.Type}: {ex.Message}");
            }
        }
    }
}