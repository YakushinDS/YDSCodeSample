using System.Data.Linq.Mapping;

namespace YDSCodeSample.Models
{
    [Table(Name = "Tasks")]
    public class TaskRecord
    {
        [Column(IsPrimaryKey = true)]
        public int Id;
        [Column]
        public string Title;
        [Column]
        public bool? Completed;
    }
}
