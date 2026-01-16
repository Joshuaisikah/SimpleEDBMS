using System;

namespace SimpleRDBMS.Domain.Exceptions
{
    public class InvalidDataTypeException : Exception
    {
        public InvalidDataTypeException(string message) : base(message) { }
    }
}