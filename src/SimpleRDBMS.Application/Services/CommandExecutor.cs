using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SimpleRDBMS.Application.Commands;
using SimpleRDBMS.Application.DTOs;
using SimpleRDBMS.Application.Interfaces;
using SimpleRDBMS.Domain.Entities;
using SimpleRDBMS.Domain.Exceptions;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;

namespace SimpleRDBMS.Application.Services
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly ITableRepository _repository;
        private readonly IIndexManager _indexManager;

        public CommandExecutor(ITableRepository repository, IIndexManager indexManager)
        {
            _repository = repository;
            _indexManager = indexManager;
        }

        public ExecutionResultDto Execute(CreateTableCommand command)
        {
            try
            {
                var table = new Table(command.TableName, command.Columns);
                _repository.SaveTable(table);
                
                return new ExecutionResultDto
                {
                    Success = true,
                    RowsAffected = 0,
                    Message = $"Table '{command.TableName}' created successfully."
                };
            }
            catch (Exception ex)
            {
                return new ExecutionResultDto
                {
                    Success = false,
                    RowsAffected = 0,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public ExecutionResultDto Execute(InsertCommand command)
        {
            try
            {
                var table = _repository.GetTable(command.TableName);
                if (table == null)
                    throw new TableNotFoundException(command.TableName);

                table.AddRow(command.Row);
                _repository.SaveTable(table);

                return new ExecutionResultDto
                {
                    Success = true,
                    RowsAffected = 1,
                    Message = "Row inserted successfully."
                };
            }
            catch (Exception ex)
            {
                return new ExecutionResultDto
                {
                    Success = false,
                    RowsAffected = 0,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public ExecutionResultDto Execute(UpdateCommand command)
        {
            try
            {
                var table = _repository.GetTable(command.TableName);
                if (table == null)
                    throw new TableNotFoundException(command.TableName);

                int rowsAffected = 0;
                var allRows = table.GetRows().ToList();

                foreach (var row in allRows)
                {
                    if (MatchesWhereClause(row, command.WhereClause))
                    {
                        foreach (var kvp in command.SetValues)
                        {
                            if (row.Values.ContainsKey(kvp.Key))
                            {
                                var column = table.Columns.FirstOrDefault(c => 
                                    c.Name.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));
                                
                                if (column != null)
                                {
                                    var value = kvp.Value;
                                    if (value != null && !column.DataType.IsValid(value))
                                    {
                                        value = ConvertValue(value, column.DataType);
                                    }
                                    row.Values[kvp.Key] = value;
                                }
                                else
                                {
                                    row.Values[kvp.Key] = kvp.Value;
                                }
                            }
                        }
                        rowsAffected++;
                    }
                }

                _repository.SaveTable(table);

                return new ExecutionResultDto
                {
                    Success = true,
                    RowsAffected = rowsAffected,
                    Message = $"{rowsAffected} row(s) updated."
                };
            }
            catch (Exception ex)
            {
                return new ExecutionResultDto
                {
                    Success = false,
                    RowsAffected = 0,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public ExecutionResultDto Execute(DeleteCommand command)
        {
            try
            {
                var table = _repository.GetTable(command.TableName);
                if (table == null)
                    throw new TableNotFoundException(command.TableName);

                var rowsToDelete = table.GetRows()
                    .Where(r => MatchesWhereClause(r, command.WhereClause))
                    .ToList();

                foreach (var row in rowsToDelete)
                {
                    table.RemoveRow(row);
                }

                _repository.SaveTable(table);

                return new ExecutionResultDto
                {
                    Success = true,
                    RowsAffected = rowsToDelete.Count,
                    Message = $"{rowsToDelete.Count} row(s) deleted."
                };
            }
            catch (Exception ex)
            {
                return new ExecutionResultDto
                {
                    Success = false,
                    RowsAffected = 0,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public ExecutionResultDto Execute(CreateIndexCommand command)
        {
            try
            {
                _indexManager.CreateIndex(command.TableName, command.IndexName, command.ColumnNames);

                return new ExecutionResultDto
                {
                    Success = true,
                    RowsAffected = 0,
                    Message = $"Index '{command.IndexName}' created successfully."
                };
            }
            catch (Exception ex)
            {
                return new ExecutionResultDto
                {
                    Success = false,
                    RowsAffected = 0,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        // ====================================================================
        // HELPER METHODS - SAME AS QueryExecutor
        // ====================================================================

        private bool MatchesWhereClause(Row row, string whereClause)
        {
            if (string.IsNullOrWhiteSpace(whereClause))
                return true;

            var match = Regex.Match(whereClause, @"(\w+)\s*(>=|<=|!=|=|>|<)\s*(.+)", RegexOptions.IgnoreCase);
            
            if (!match.Success)
                return true;

            var columnName = match.Groups[1].Value.Trim();
            var op = match.Groups[2].Value.Trim();
            var valueStr = match.Groups[3].Value.Trim();

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

        private object ConvertValue(object value, SimpleRDBMS.Domain.ValueObjects.DataType dataType)
        {
            if (value == null)
                return null;

            try
            {
                return dataType.Type switch
                {
                    SimpleRDBMS.Domain.ValueObjects.DataTypeEnum.INT when value is decimal dec => (int)dec,
                    SimpleRDBMS.Domain.ValueObjects.DataTypeEnum.INT when value is double dbl => (int)dbl,
                    SimpleRDBMS.Domain.ValueObjects.DataTypeEnum.INT when value is string s => int.Parse(s),
                    SimpleRDBMS.Domain.ValueObjects.DataTypeEnum.DECIMAL when value is int i => (decimal)i,
                    SimpleRDBMS.Domain.ValueObjects.DataTypeEnum.DECIMAL when value is string s => decimal.Parse(s),
                    SimpleRDBMS.Domain.ValueObjects.DataTypeEnum.VARCHAR => value.ToString(),
                    SimpleRDBMS.Domain.ValueObjects.DataTypeEnum.BOOL when value is int i => i != 0,
                    SimpleRDBMS.Domain.ValueObjects.DataTypeEnum.BOOL when value is string s => bool.Parse(s),
                    _ => value
                };
            }
            catch
            {
                return value;
            }
        }
    }
}