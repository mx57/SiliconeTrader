using System;
using System.Net;

namespace SiliconeTrader.Machine.Client
{
    public class TransientException : Exception
    {
        public TransientException(HttpStatusCode statusCode) : base("Http request failed: " + statusCode)
        {
            this.StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}