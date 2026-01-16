namespace SimpleRDBMS.Application.Queries
{
    public class JoinQuery
    {
        public SelectQuery LeftQuery { get; }
        public SelectQuery RightQuery { get; }
        public JoinType JoinType { get; }
        public string JoinCondition { get; } // Simplified

        public JoinQuery(SelectQuery left, SelectQuery right, JoinType joinType, string condition)
        {
            LeftQuery = left;
            RightQuery = right;
            JoinType = joinType;
            JoinCondition = condition;
        }
    }

    public enum JoinType
    {
        Inner,
        Left,
        Right,
        FullOuter
    }
}