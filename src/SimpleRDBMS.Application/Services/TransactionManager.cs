using System;
using System.Collections.Generic;
using SimpleRDBMS.Application.Interfaces;
using SimpleRDBMS.Domain.Entities;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;

namespace SimpleRDBMS.Application.Services
{
    public class TransactionManager : ITransactionManager
    {
        private readonly ITableRepository _tableRepository;
        private readonly IIndexManager _indexManager; // optional - for index maintenance

        private bool _inTransaction;
        private readonly Stack<TransactionOperation> _undoLog = new Stack<TransactionOperation>();

        public TransactionManager(
            ITableRepository tableRepository,
            IIndexManager indexManager = null)
        {
            _tableRepository = tableRepository ?? throw new ArgumentNullException(nameof(tableRepository));
            _indexManager = indexManager;
        }

        public void BeginTransaction()
        {
            if (_inTransaction)
            {
                throw new InvalidOperationException("Transaction already in progress. Nested transactions are not supported.");
            }

            _inTransaction = true;
            _undoLog.Clear();
        }

        public void Commit()
        {
            if (!_inTransaction)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            // In real DB we would flush changes here
            // Here we just clean up - changes are already applied
            _inTransaction = false;
            _undoLog.Clear();
        }

        public void Rollback()
        {
            if (!_inTransaction)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            // Apply undo operations in reverse order
            while (_undoLog.Count > 0)
            {
                var operation = _undoLog.Pop();
                operation.Undo(_tableRepository);
            }

            _inTransaction = false;
            _undoLog.Clear();
        }

        public bool IsInTransaction => _inTransaction;
        
        public void RecordTableCreated(string tableName)
        {
            if (!_inTransaction) return;

            _undoLog.Push(new TransactionOperation(
                OperationType.CreateTable,
                tableName,
                beforeState: null,
                afterState: null));
        }
        
        public void RecordRowInserted(string tableName, Row row)
        {
            if (!_inTransaction) return;

            _undoLog.Push(new TransactionOperation(
                OperationType.InsertRow,
                tableName,
                beforeState: null,
                afterState: row));
        }

        /// <summary>
        /// Records that a row was deleted (for potential rollback - we keep old value)
        /// </summary>
        public void RecordRowDeleted(string tableName, Row oldRow)
        {
            if (!_inTransaction) return;

            _undoLog.Push(new TransactionOperation(
                OperationType.DeleteRow,
                tableName,
                beforeState: oldRow,
                afterState: null));
        }

        // You can add more operation types (UpdateRow, CreateIndex, DropTable, etc.)
    }

    // ──────────────────────────────────────────────────────────────────────────────
    //   Internal undo operation representation
    // ──────────────────────────────────────────────────────────────────────────────

    internal class TransactionOperation
    {
        public OperationType Type { get; }
        public string TableName { get; }
        public Row BeforeState { get; }  // for undo delete / update
        public Row AfterState { get; }   // for undo insert

        public TransactionOperation(
            OperationType type,
            string tableName,
            Row beforeState = null,
            Row afterState = null)
        {
            Type = type;
            TableName = tableName;
            BeforeState = beforeState;
            AfterState = afterState;
        }

        public void Undo(ITableRepository repository)
        {
            var table = repository.GetTable(TableName);
            if (table == null) return; // table might have been dropped meanwhile

            switch (Type)
            {
                case OperationType.CreateTable:
                    repository.DeleteTable(TableName);
                    break;

                case OperationType.InsertRow:
                    // Find and remove the row we inserted
                    // (very naive - in real system we'd use PK or row reference)
                    var rows = table.GetRows();
                    foreach (var row in rows)
                    {
                        if (RowsAreEqual(row, AfterState))
                        {
                            table.RemoveRow(row);
                            break;
                        }
                    }
                    break;

                case OperationType.DeleteRow:
                    // Put back the deleted row
                    table.AddRow(BeforeState);
                    break;

                // Add cases for UpdateRow, etc.
                default:
                    throw new NotSupportedException($"Undo for operation {Type} not implemented");
            }
        }

        private static bool RowsAreEqual(Row a, Row b)
        {
            if (a.Values.Count != b.Values.Count) return false;

            foreach (var kvp in a.Values)
            {
                if (!b.Values.TryGetValue(kvp.Key, out var otherVal) ||
                    !Equals(kvp.Value, otherVal))
                    return false;
            }
            return true;
        }
    }

    internal enum OperationType
    {
        CreateTable,
        InsertRow,
        DeleteRow,
        // UpdateRow,
        // CreateIndex,
        // etc.
    }
}