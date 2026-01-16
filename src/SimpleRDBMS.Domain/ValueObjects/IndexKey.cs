
namespace SimpleRDBMS.Domain.ValueObjects
{
    public class IndexKey : IComparable<IndexKey>
    {
        public object[] Values { get; private set; }

        public IndexKey(params object[] values)
        {
            Values = values;
        }

        public int CompareTo(IndexKey other)
        {
            if (other == null) return 1;
            for (int i = 0; i < Values.Length; i++)
            {
                var cmp = ((IComparable)Values[i]).CompareTo(other.Values[i]);
                if (cmp != 0) return cmp;
            }
            return 0;
        }

        // Override Equals and GetHashCode for use in dictionaries
        public override bool Equals(object obj)
        {
            if (!(obj is IndexKey other)) return false;
            if (Values.Length != other.Values.Length) return false;
            for (int i = 0; i < Values.Length; i++)
            {
                if (!Values[i].Equals(other.Values[i])) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var val in Values)
                {
                    hash = hash * 23 + (val?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }
    }
}