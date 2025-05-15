using DailyFlow.Entities;
namespace DailyFlow.DTOs
{
    public class TaskDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public _Environment Environment { get; set; }
        public DateTime DueDate { get; set; }
        public int Importance { get; set; }
        public float Priority {  get; set; }
    }
}
