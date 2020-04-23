using System;
using System.Collections.Generic;
using YDSCodeSample.Models;

namespace YDSCodeSample.Services.Storage
{
    interface IStorage
    {
        string FilePath { get; }

        bool OpenFile(string path);
        bool CreateFile(string path);
        bool CloseFile();
        List<TaskRecord> GetTasks();
        List<Tag> GetTags();
        //List<Tag> GetTaskTags(TaskRecord task);
        //bool SetTaskTags(TaskRecord task, List<Tag> tags);
        bool CreateTask(TaskRecord task);
        bool UpdateTask(TaskRecord task);
        bool DeleteTask(TaskRecord task);
        bool CreateTag(Tag tag);
        bool UpdateTag(Tag tag);
        bool DeleteTag(Tag tag);
    }
}
