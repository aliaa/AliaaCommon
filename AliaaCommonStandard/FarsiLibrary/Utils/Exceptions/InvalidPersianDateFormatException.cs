using System;

namespace FarsiLibrary.Utils.Exceptions
{
    public class InvalidPersianDateFormatException : Exception
    {
        public InvalidPersianDateFormatException(string message)
            : base(message)
        {
        }

        public InvalidPersianDateFormatException(string message, Exception ex)
            : base(message, ex)
        {

        }

        public InvalidPersianDateFormatException() : base(string.Empty)
        { 
        }
    }
}
