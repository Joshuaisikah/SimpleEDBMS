using System;

namespace SimpleRDBMS.Domain.Exceptions
{
    public class ConstraintViolationException : Exception
    {
        public ConstraintViolationException(string message) : base(message) { }
    }
}