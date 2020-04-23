using System;

namespace YDSCodeSample.Services.EventSink
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
