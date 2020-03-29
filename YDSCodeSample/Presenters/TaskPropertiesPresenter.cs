using System;
using YDSCodeSample.Models;
using YDSCodeSample.Services;
using YDSCodeSample.Views.TaskProperties;

namespace YDSCodeSample.Presenters
{
    class TaskPropertiesPresenter : IPresenter<TaskRecord>
    {
        private ITaskPropertiesView view;
        private IStorage storage;
        private TaskRecord task;

        public TaskPropertiesPresenter(ITaskPropertiesView view, IStorage storage)
        {
            this.view = view;
            this.storage = storage;

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
                    Completed = false
                };

                storage.CreateTask(task);
            }
            else
            {
                task.Title = view.TaskTitle;
                storage.ModifyTask(task);
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
