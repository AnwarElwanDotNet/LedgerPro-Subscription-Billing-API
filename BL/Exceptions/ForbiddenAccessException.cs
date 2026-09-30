using System;

namespace BL.Exceptions
{
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException(string message)
            : base(message)
        {
        }
    }
}