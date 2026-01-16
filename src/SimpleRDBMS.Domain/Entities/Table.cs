
using System.Data;
using SimpleRDBMS.Domain.ValueObjects;
using SimpleRDBMS.Domain.Exceptions;
using SimpleRDBMS.Domain.Interfaces;

namespace SimpleRDBMS.Domain.Entities
{
    public class Table : ITable
    {
        public string Name { get; private set; }
        private readonly List<Column> _columns = new List<Column>();
        public IReadOnlyList<Column> Columns => _columns.AsReadOnly();
        private readonly List<Row> _rows = new List<Row>();
        public IReadOnlyList<Row> Rows => _rows.AsReadOnly();
        private readonly List<Constraint> _constraints = new List<Constraint>();
        public IReadOnlyList<Constraint> Constraints => _constraints.AsReadOnly();
        public Table(string name, IEnumerable<ColumnDefinition> columnDefinitions)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            foreach (var def in columnDefinitions)
            {
                AddColumn(new Column(def));
            }
        }

        public void AddColumn(Column column)
        {
            if (_columns.Any(c => c.Name.Equals(column.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Column '{column.Name}' already exists.");
            _columns.Add(column);
        }

        public void AddRow(Row row)
        {
            ValidateRow(row);
            _rows.Add(row);
        }

        public void RemoveRow(Row row)
        {
            _rows.Remove(row);
        }

        public IEnumerable<Row> GetRows() => _rows;

        public void AddConstraint(Constraint constraint)
        {
            _constraints.Add(constraint);
        }

        private void ValidateRow(Row row)
        {
            if (row.Values.Count != _columns.Count)
                throw new ConstraintViolationException("Row column count mismatch.");

            for (int i = 0; i < _columns.Count; i++)
            {
                var col = _columns[i];
                if (!row.Values.TryGetValue(col.Name, out var value))
                    throw new ConstraintViolationException($"Missing value for column '{col.Name}'.");

                if (!col.DataType.IsValid(value))
                    throw new InvalidDataTypeException($"Invalid data type for column '{col.Name}'. Expected: {col.DataType.Type}");

                foreach (var constraint in _constraints.Where(c => c.ColumnName == col.Name))
                {
                    constraint.Validate(value, this);
                }
            }
        }
    }
}