using DailyFlow.Entities;

namespace DailyFlow.DTOs
{
    public class HabitUpdatingDTO
    {
        public required string Title { get; set; }
        public string Description { get; set; }
        public bool Done { get; set; }
        public int ProgrammDays { get; set; }
        public DateOnly StartingDay { get; set; }
        public _Environment _Environment { get; set; }
    }
}
