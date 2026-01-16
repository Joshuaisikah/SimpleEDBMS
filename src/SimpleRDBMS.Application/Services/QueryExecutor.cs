using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SimpleRDBMS.Application.Interfaces;
using SimpleRDBMS.Application.Queries;
using SimpleRDBMS.Domain.Entities;
using SimpleRDBMS.Domain.Exceptions;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;

namespace SimpleRDBMS.Application.Services
{
    public class QueryExecutor : IQueryExecutor
    {
        private readonly ITableRepository _repository;

        public QueryExecutor(ITableRepository repository)
        {
            _repository = repository;
        }

        public QueryResult Execute(SelectQuery query)
        {
            var table = _repository.GetTable(query.TableName);
            if (table == null)
                throw new TableNotFoundException(query.TableName);

            var rows = table.GetRows().ToList();

            // Apply WHERE clause
            if (!string.IsNullOrWhiteSpace(query.WhereClause))
            {
                rows = ApplyWhereClause(rows, query.WhereClause);
            }

            // Select specific columns or all
            var selectedColumns = query.Columns.Contains("*") 
                ? table.Columns.ToList() 
                : table.Columns.Where(c => query.Columns.Contains(c.Name)).ToList();

            // Project rows to selected columns
            var projectedRows = rows.Select(r => new Row(
                selectedColumns.Select(c => new KeyValuePair<string, object>(c.Name, r.Values[c.Name]))
            )).ToList();

            return new QueryResult(selectedColumns, projectedRows);
        }

        public QueryResult Execute(JoinQuery query)
        {
            var leftResult = Execute(query.LeftQuery);
            var rightResult = Execute(query.RightQuery);

            var joinParts = query.JoinCondition.Split('=');
            var leftCol = joinParts[0].Trim().Split('.')[1];
            var rightCol = joinParts[1].Trim().Split('.')[1];

            var joinedRows = new List<Row>();

            foreach (var leftRow in leftResult.Rows)
            {
                foreach (var rightRow in rightResult.Rows)
                {
                    if (leftRow.Values[leftCol]?.Equals(rightRow.Values[rightCol]) == true)
                    {
                        var mergedValues = new Dictionary<string, object>();
                        foreach (var kvp in leftRow.Values)
                            mergedValues[$"{query.LeftQuery.TableName}.{kvp.Key}"] = kvp.Value;
                        foreach (var kvp in rightRow.Values)
                            mergedValues[$"{query.RightQuery.TableName}.{kvp.Key}"] = kvp.Value;

                        joinedRows.Add(new Row(mergedValues));
                    }
                }
            }

            var mergedColumns = leftResult.Columns.Concat(rightResult.Columns).ToList();
            return new QueryResult(mergedColumns, joinedRows);
        }

        // ====================================================================
        // FIXED WHERE CLAUSE PARSING
        // ====================================================================

        private List<Row> ApplyWhereClause(List<Row> rows, string whereClause)
        {
            // Use regex to parse: "column operator value"
            var match = Regex.Match(whereClause, @"(\w+)\s*(>=|<=|!=|=|>|<)\s*(.+)", RegexOptions.IgnoreCase);
            
            if (!match.Success)
                return rows; // Invalid WHERE clause, return all rows

            var columnName = match.Groups[1].Value.Trim();
            var op = match.Groups[2].Value.Trim();
            var valueStr = match.Groups[3].Value.Trim();

            return rows.Where(r => MatchesCondition(r, columnName, op, valueStr)).ToList();
        }

        private bool MatchesCondition(Row row, string columnName, string op, string valueStr)
        {
            if (!row.Values.ContainsKey(columnName))
                return false;

            var rowValue = row.Values[columnName];
            var compareValue = ParseValue(valueStr);

            return CompareValues(rowValue, compareValue, op);
        }

        private object ParseValue(string value)
        {
            if (value == null)
                return null;

            value = value.Trim().Trim('\'', '"');

            if (string.IsNullOrEmpty(value))
                return null;

            if (int.TryParse(value, out int intVal))
                return intVal;
            
            if (decimal.TryParse(value, out decimal decVal))
                return decVal;
            
            if (bool.TryParse(value, out bool boolVal))
                return boolVal;

            return value;
        }

        private bool CompareValues(object rowValue, object compareValue, string op)
        {
            if (rowValue == null)
                return op == "!=" ? compareValue != null : false;

            if (compareValue == null)
                return op == "!=";

            // Numeric comparison
            if (IsNumeric(rowValue) && IsNumeric(compareValue))
            {
                var a = Convert.ToDecimal(rowValue);
                var b = Convert.ToDecimal(compareValue);

                return op switch
                {
                    "=" => a == b,
                    "!=" => a != b,
                    ">" => a > b,
                    "<" => a < b,
                    ">=" => a >= b,
                    "<=" => a <= b,
                    _ => false
                };
            }

            // String comparison
            var strA = rowValue.ToString();
            var strB = compareValue.ToString();

            return op switch
            {
                "=" => strA.Equals(strB, StringComparison.OrdinalIgnoreCase),
                "!=" => !strA.Equals(strB, StringComparison.OrdinalIgnoreCase),
                ">" => string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) > 0,
                "<" => string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) < 0,
                ">=" => string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) >= 0,
                "<=" => string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) <= 0,
                _ => false
            };
        }

        private bool IsNumeric(object value)
        {
            return value is int || value is long || value is decimal || 
                   value is double || value is float || value is short;
        }
    }
}