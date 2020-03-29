using System;

namespace YDSCodeSample.Services.ErrorEventSink
{
    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; protected set; }

        public ErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
