using SimpleRDBMS.Domain.Entities;

namespace SimpleRDBMS.Infrastructure.Query
{
    /// <summary>
    /// Responsible for choosing join algorithms and estimating costs
    /// Very simplified version suitable for educational/minimal RDBMS
    /// </summary>
    public class JoinStrategy
    {
        /// <summary>
        /// Very naive join strategy selection based on simple heuristics
        /// Real systems use statistics, cardinality estimates, available indexes, etc.
        /// </summary>
        public JoinAlgorithm ChooseAlgorithm(
            Table leftTable,
            Table rightTable,
            string joinCondition,
            long leftCardinality,
            long rightCardinality)
        {
            // Small table × anything → Nested Loop is usually fastest
            if (leftCardinality < 100 || rightCardinality < 100)
            {
                return JoinAlgorithm.NestedLoop;
            }

            // Equality condition → Hash Join is typically good choice (unless sorted)
            if (!string.IsNullOrWhiteSpace(joinCondition) && joinCondition.Contains("="))
            {
                return JoinAlgorithm.HashJoin;
            }

            // Default fallback (could be improved with sort-order tracking)
            return JoinAlgorithm.NestedLoop;
        }

        /// <summary>
        /// Very rough cost estimation for different join algorithms
        /// Real optimizers use much more sophisticated formulas
        /// </summary>
        public long EstimateJoinCost(
            JoinAlgorithm algo,
            long leftRows,
            long rightRows)
        {
            return algo switch
            {
                JoinAlgorithm.NestedLoop => leftRows * rightRows,
                JoinAlgorithm.HashJoin   => leftRows + rightRows + 1000, // build hash + probe
                JoinAlgorithm.MergeJoin  => leftRows + rightRows,         // assumes inputs are sorted
                _                        => long.MaxValue
            };
        }
    }
}