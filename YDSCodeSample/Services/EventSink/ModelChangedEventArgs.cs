using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YDSCodeSample.Models;

namespace YDSCodeSample.Services.EventSink
{
    public class ModelChangedEventArgs<T> : EventArgs
    {
        public T Model { get; }
        public ModelChangedAction Action { get; }
        public Object Initiator { get; }

        public ModelChangedEventArgs(T model, ModelChangedAction action)
        {
            Model = model;
            Action = action;
        }
    }
}
