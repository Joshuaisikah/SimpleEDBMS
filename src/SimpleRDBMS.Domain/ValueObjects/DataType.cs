// src/SimpleRDBMS.Domain/ValueObjects/DataType.cs
using System;

namespace SimpleRDBMS.Domain.ValueObjects
{
    public class DataType
    {
        public DataTypeEnum Type { get; private set; }
        public int? Size { get; private set; } // For VARCHAR(size)

        public DataType(DataTypeEnum type, int? size = null)
        {
            Type = type;
            Size = size;
        }

        public bool IsValid(object value)
        {
            return Type switch
            {
                DataTypeEnum.INT => value is int,
                DataTypeEnum.VARCHAR => value is string str && (Size == null || str.Length <= Size.Value),
                DataTypeEnum.BOOL => value is bool,
                DataTypeEnum.DECIMAL => value is decimal,
                DataTypeEnum.DATE => value is DateTime,
                _ => false
            };
        }
    }

    public enum DataTypeEnum
    {
        INT,
        VARCHAR,
        BOOL,
        DECIMAL,
        DATE
    }
}