using DailyFlow.Entities;

namespace DailyFlow.DTOs
{
    public class TaskPreviewDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } //shorter description just x first characters
        public _Environment Environment { get; set; }
        public DateTime DueDate { get; set; }
        public int Importance { get; set; }
        public float Priority { get; set; }
    }

}
