using DailyFlow.Entities;

namespace DailyFlow.DTOs
{
    public class TaskCreatingDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public _Environment Environment { get; set; }
        public DateTime DueDate { get; set; }
        public int Importance { get; set; }
        public bool Done { get; set; } = false;
        public bool Scheduled { get; set; } = false;
        public string? UserId {  get; set; }
    }
}
