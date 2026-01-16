using System.Collections.Generic;

namespace SimpleRDBMS.Domain.Entities
{
    public class Row
    {
        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        public Row(IEnumerable<KeyValuePair<string, object>> values)
        {
            foreach (var kvp in values)
            {
                Values[kvp.Key] = kvp.Value;
            }
        }
    }
}