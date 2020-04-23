using System;
using System.Collections.Generic;
using YDSCodeSample.Models;

namespace YDSCodeSample.Views.Main
{
    public interface IMainView : IView
    {
        event Action<string> OpenFileRequest;
        event Action<string> CreateFileRequest;
        event Action CreateTaskRequest;
        event Action<TaskRecord> DeleteTaskRequest;
        event Action<TaskRecord> ModifyTaskRequest;
        event Action<TaskRecord> SetTaskDoneRequest;
        event Action<TaskRecord> SetTaskUndoneRequest;
        //event Action RefreshRequest;
        event Action ExitRequest;
        event Action UndoRequest;
        event Action RedoRequest;

        bool UndoPossible { set; }
        bool RedoPossible { set; }

        void SetTasks(List<TaskRecord> tasks);
        void RefreshTasks(List<TaskRecord> tasks);
        void SetFilePath(string value);
        void ShowError(string message);
        void DeleteTask(TaskRecord task);
        void AddTask(TaskRecord task);
    }
}
