using System;

namespace YDSCodeSample.Services.ErrorEventSink
{
    public class ErrorEventSink : IErrorEventSink
    {
        public event EventHandler<ErrorEventArgs> ErrorOccurred;

        public void ReportError(object sender, Exception exception)
        {
            ErrorOccurred?.Invoke(sender, new ErrorEventArgs(exception));
        }
    }
}
