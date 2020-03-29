using System;

namespace YDSCodeSample.Services.ErrorEventSink
{
    public interface IErrorEventSink
    {
        event EventHandler<ErrorEventArgs> ErrorOccurred;

        void ReportError(Object sender, Exception exception);
    }
}
