using SimpleRDBMS.Domain.Entities;

namespace SimpleRDBMS.Domain.Interfaces
{
    public interface IConstraintValidator
    {
        void Validate(object value, Table table);
    }
}