using System;
using System.Windows.Input;
using SimpleRDBMS.Domain.Entities;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;

namespace SimpleRDBMS.Application.Commands
{
    public class InsertCommand : ICommand
    {
        private readonly ITableRepository _tableRepository;
        private bool _isExecuting;
        
        public string TableName { get; }
        public Row Row { get; }
        
        public event EventHandler CanExecuteChanged;

        public InsertCommand(ITableRepository tableRepository, string tableName, Row row)
        {
            _tableRepository = tableRepository ?? throw new ArgumentNullException(nameof(tableRepository));
            TableName = !string.IsNullOrWhiteSpace(tableName) 
                ? tableName 
                : throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            Row = row ?? throw new ArgumentNullException(nameof(row));
        }

        public bool CanExecute(object parameter)
        {
            // Can execute if not already executing and has a valid table name and row
            return !_isExecuting && 
                   !string.IsNullOrWhiteSpace(TableName) && 
                   Row != null;
        }

        // In InsertCommand.cs, update the Execute method:
        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                throw new InvalidOperationException("Cannot execute the command in the current state.");
            }

            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();

                var table = _tableRepository.GetTable(TableName);
                if (table == null)
                {
                    throw new InvalidOperationException($"Table '{TableName}' not found.");
                }

                // Change this line from table.Insert(Row) to:
                table.AddRow(Row);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void Execute()
        {
            Execute(null);
        }

        protected virtual void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}