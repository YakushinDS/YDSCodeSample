using System;
using System.Collections.Generic;
using YDSCodeSample.Models;

namespace YDSCodeSample.Views.Main
{
    public interface IMainView : IView
    {
        event Action<string> OpenFile;
        event Action<string> CreateFile;
        event Action CreateTask;
        event Action<TaskRecord> DeleteTask;
        event Action<TaskRecord> ModifyTask;
        event Action<TaskRecord> SetTaskDone;
        event Action<TaskRecord> SetTaskUndone;
        event Action Exit;
        event Action Undo;
        event Action Redo;

        bool UndoPossible { set; }

        bool RedoPossible { set; }

        void SetTasks(List<TaskRecord> value);

        void SetFilePath(string value);

        void ShowError(string message);
    }
}
