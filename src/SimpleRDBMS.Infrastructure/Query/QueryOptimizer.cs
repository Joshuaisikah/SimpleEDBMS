using System.Collections.Generic;
using System.Linq;
using SimpleRDBMS.Application.Queries;
using SimpleRDBMS.Domain.Entities;
using SimpleRDBMS.Domain.Exceptions;
using SimpleRDBMS.Infrastructure.Persistence.Interfaces;

namespace SimpleRDBMS.Infrastructure.Query
{
    public class QueryOptimizer
    {
        private readonly ITableRepository _repository;
        private readonly IIndexManager _indexManager;

        public QueryOptimizer(ITableRepository repository, IIndexManager indexManager)
        {
            _repository = repository;
            _indexManager = indexManager;
        }

        /// <summary>
        /// Very naive optimizer — tries to choose between table scan and index scan
        /// Returns simple execution strategy suggestion
        /// </summary>
        public ExecutionPlan Optimize(SelectQuery query)
        {
            var table = _repository.GetTable(query.TableName);
            if (table == null)
                throw new TableNotFoundException(query.TableName);

            var plan = new ExecutionPlan
            {
                TableName = query.TableName,
                EstimatedCost = 999999 // default: very expensive
            };

            // Very simple heuristic: if there's equality condition on indexed column → use index
            if (HasEqualityConditionOnIndexedColumn(query.WhereClause, table))
            {
                plan.Strategy = ExecutionStrategy.IndexScan;
                plan.EstimatedCost = 10 + table.Rows.Count / 100; // fake cost
            }
            else
            {
                plan.Strategy = ExecutionStrategy.TableScan;
                plan.EstimatedCost = table.Rows.Count; // full scan cost
            }

            // For real optimizer you would:
            // - Parse WHERE into predicates
            // - Check statistics / cardinality
            // - Consider index selectivity
            // - Choose join order (for joins)

            return plan;
        }

        private bool HasEqualityConditionOnIndexedColumn(string whereClause, Table table)
        {
            // Extremely naive — real version would parse expression tree
            if (string.IsNullOrWhiteSpace(whereClause))
                return false;

            // Just example keywords — you should really parse the condition
            var indexedColumns = table.Constraints
                .OfType<PrimaryKeyConstraint>()
                .Select(c => c.ColumnName)
                .Concat(table.Constraints.OfType<UniqueConstraint>().Select(c => c.ColumnName))
                .Distinct()
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return whereClause.Contains("=") &&
                   indexedColumns.Any(col => whereClause.Contains(col, StringComparison.OrdinalIgnoreCase));
        }
    }

    public enum ExecutionStrategy
    {
        TableScan,
        IndexScan,
        IndexSeek,
        // HashJoin, MergeJoin, NestedLoop, etc.
    }
}