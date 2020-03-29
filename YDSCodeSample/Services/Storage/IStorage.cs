using System;
using System.Collections.Generic;
using YDSCodeSample.Models;

namespace YDSCodeSample.Services
{
    interface IStorage
    {
        event Action<TaskRecord> TaskCreated;
        event Action<TaskRecord> TaskUpdated;
        event Action<TaskRecord> TaskDeleted;
        event Action<string> FileOpened;

        void OpenFile(string path);

        void CreateFile(string path);

        List<TaskRecord> GetTasks();

        void CreateTask(TaskRecord task);

        void ModifyTask(TaskRecord task);

        void DeleteTask(TaskRecord task);
    }
}
