namespace SimpleRDBMS.Infrastructure.Query
{
    public enum JoinAlgorithm
    {
        NestedLoop,
        HashJoin,
        MergeJoin     // requires sorted inputs
    }
}