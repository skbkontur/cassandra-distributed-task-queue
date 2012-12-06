using System;

namespace ExchangeService.Exceptions
{
    public class TooLateException : Exception
    {
        public TooLateException(string format, params object[] parameters)
            : base(string.Format(format, parameters))
        {
        }
    }
}