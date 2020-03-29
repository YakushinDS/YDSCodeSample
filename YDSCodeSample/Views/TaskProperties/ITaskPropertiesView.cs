using System;

namespace YDSCodeSample.Views.TaskProperties
{
    interface ITaskPropertiesView : IView
    {
        event Action OK;
        event Action Cancel;

        string TaskTitle { get; set; }

        bool CreatingNewTask { set; }
    }
}
