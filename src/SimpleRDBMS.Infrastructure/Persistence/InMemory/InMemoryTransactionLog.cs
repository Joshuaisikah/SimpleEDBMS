using System.Collections.Generic;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;

namespace SimpleRDBMS.Infrastructure.Persistence.InMemory
{
    public class InMemoryTransactionLog : ITransactionLog
    {
        private readonly List<string> _log = new();
        private bool _inTransaction = false;

        public void BeginTransaction()
        {
            _inTransaction = true;
            _log.Clear();
        }

        public void LogOperation(string operation)
        {
            if (_inTransaction)
                _log.Add(operation);
        }

        public void Commit()
        {
            _inTransaction = false;
            _log.Clear();
        }

        public void Rollback()
        {
            // In a real implementation, replay inverse operations
            _inTransaction = false;
            _log.Clear();
        }
    }
}