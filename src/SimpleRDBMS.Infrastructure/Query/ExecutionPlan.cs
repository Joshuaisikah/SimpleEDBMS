using System.Collections.Generic;

namespace SimpleRDBMS.Infrastructure.Query
{
    /// <summary>
    /// Represents the physical execution plan for a query (or part of it)
    /// </summary>
    public class ExecutionPlan
    {
        public string TableName { get; set; }
        public ExecutionStrategy Strategy { get; set; }
        public long EstimatedCost { get; set; }
        public string IndexUsed { get; set; }           // Name of index used, if any
        public string FilterCondition { get; set; }     // Simplified representation of remaining filter
        public List<JoinPlan> Joins { get; set; } = new List<JoinPlan>();
    }
}