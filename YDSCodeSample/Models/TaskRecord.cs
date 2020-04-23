using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;

namespace YDSCodeSample.Models
{
    public class TaskRecord : Model
    {
        public string Title;
        public bool? Completed;
        public DateTime? CreationTime;
        public DateTime? ModificationTime;
        public DateTime? CompletionTime;
        public DateTime? NotificationTime;
        public DateTime? DeadlineTime;
        public string Description;
        public List<Tag> Tags;

        public TaskRecord()
        {
            Tags = new List<Tag>();
        }
    }
}
