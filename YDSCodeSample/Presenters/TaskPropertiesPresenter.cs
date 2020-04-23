using System;
using YDSCodeSample.Models;
using YDSCodeSample.Services;
using YDSCodeSample.Services.EventSink;
using YDSCodeSample.Services.Storage;
using YDSCodeSample.Views.TaskProperties;

namespace YDSCodeSample.Presenters
{
    class TaskPropertiesPresenter : IPresenter<TaskRecord>
    {
        private ITaskPropertiesView view;
        private IStorage storage;
        private IEventSink eventSink;
        private TaskRecord task;

        public TaskPropertiesPresenter(ITaskPropertiesView view, IStorage storage, IEventSink eventSink)
        {
            this.view = view;
            this.storage = storage;
            this.eventSink = eventSink;

            view.OK += View_OK;
            view.Cancel += View_Cancel;
        }

        private void View_Cancel()
        {
            view.Close();
        }

        private void View_OK()
        {
            if (task == null)
            {
                task = new TaskRecord()
                {
                    Id = (new Random(DateTime.Now.Millisecond)).Next(),
                    Title = view.TaskTitle,
                    Completed = false,
                    CreationTime = DateTime.Now,
                    ModificationTime = DateTime.Now
                };

                storage.CreateTask(task);
                eventSink.InvokeTaskChanged(this, new ModelChangedEventArgs<TaskRecord>(task, ModelChangedAction.Create));
            }
            else
            {
                task.Title = view.TaskTitle;
                task.ModificationTime = DateTime.Now;
                storage.UpdateTask(task);
                eventSink.InvokeTaskChanged(this, new ModelChangedEventArgs<TaskRecord>(task, ModelChangedAction.Update));
            }

            view.Close();
        }

        public void Run(TaskRecord task)
        {
            this.task = task;

            if (task == null)
            {
                view.CreatingNewTask = true;
            }
            else
            {
                view.CreatingNewTask = false;
                view.TaskTitle = task.Title;
            }

            view.Open();
        }
    }
}
