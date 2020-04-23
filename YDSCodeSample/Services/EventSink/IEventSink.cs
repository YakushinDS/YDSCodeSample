using System;
using YDSCodeSample.Models;

namespace YDSCodeSample.Services.EventSink
{
    public interface IEventSink
    {
        event EventHandler<ErrorEventArgs> ErrorOccurred;
        event EventHandler<ModelChangedEventArgs<TaskRecord>> TaskChanged;
        event EventHandler<ModelChangedEventArgs<Tag>> TagChanged;
        event EventHandler<FileOpenedEventArgs> FileOpened;

        void InvokeErrorOccurred(Object sender, Exception exception);
        void InvokeFileOpened(Object sender, FileOpenedEventArgs arg);
        void InvokeTaskChanged(Object sender, ModelChangedEventArgs<TaskRecord> arg);
        void InvokeTagChanged(Object sender, ModelChangedEventArgs<Tag> arg);
    }
}
