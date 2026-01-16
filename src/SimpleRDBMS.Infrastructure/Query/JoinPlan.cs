using SimpleRDBMS.Application.Queries;

namespace SimpleRDBMS.Infrastructure.Query
{
    /// <summary>
    /// Describes how two tables/datasets are joined in the execution plan
    /// </summary>
    public class JoinPlan
    {
        public string RightTable { get; set; }
        public JoinType JoinType { get; set; }
        public string JoinCondition { get; set; }
        public JoinAlgorithm Algorithm { get; set; }
        public long EstimatedCost { get; set; }
    }
}