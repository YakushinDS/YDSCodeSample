using System;
using YDSCodeSample.Models;

namespace YDSCodeSample.Services.EventSink
{
    public class EventSink : IEventSink
    {
        public event EventHandler<ErrorEventArgs> ErrorOccurred;
        public event EventHandler<ModelChangedEventArgs<TaskRecord>> TaskChanged;
        public event EventHandler<ModelChangedEventArgs<Tag>> TagChanged;
        public event EventHandler<FileOpenedEventArgs> FileOpened;

        public void InvokeErrorOccurred(object sender, Exception exception)
        {
            ErrorOccurred?.Invoke(sender, new ErrorEventArgs(exception));
        }

        public void InvokeFileOpened(object sender, FileOpenedEventArgs arg)
        {
            FileOpened?.Invoke(sender, arg);
        }

        public void InvokeTagChanged(object sender, ModelChangedEventArgs<Tag> arg)
        {
            TagChanged?.Invoke(sender, arg);
        }

        public void InvokeTaskChanged(object sender, ModelChangedEventArgs<TaskRecord> arg)
        {
            TaskChanged?.Invoke(sender, arg);
        }
    }
}
