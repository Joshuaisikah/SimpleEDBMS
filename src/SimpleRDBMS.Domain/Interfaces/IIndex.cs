using System.Collections.Generic;
using SimpleRDBMS.Domain.Entities;

namespace SimpleRDBMS.Domain.Interfaces
{
    public interface IIndex
    {
        void Insert(object key, Row row);
        void Delete(object key, Row row);
        IEnumerable<Row> Search(object key);
        IEnumerable<Row> RangeScan(object start, object end);
    }
}