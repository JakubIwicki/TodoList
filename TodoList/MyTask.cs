using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoList
{
    public enum TaskStatus
    {
        InProgress,
        Completed
    }

    public interface IMyTaskStatus
    {
        void ChangeStatus(TaskStatus status);
    }

    public class MyTask : IMyTaskStatus
    {
        [Key] public int Id { get; set; }

        [StringLength(100)]
        public string? Title { get; set; } = "New Task";

        [StringLength(300)]
        public string? Description { get; set; } = "Description";

        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);
        public TaskStatus Status { get; set; } = TaskStatus.InProgress;

        public bool IsOverdue => DueDate < DateTime.Now && Status != TaskStatus.Completed;

        public void ChangeStatus(TaskStatus status)
        {
            Status = status;
        }

        public override string ToString()
        {
            return $"Title: {Title}, DueDate: {DueDate}, Status: {Status}";
        }
    }
}
