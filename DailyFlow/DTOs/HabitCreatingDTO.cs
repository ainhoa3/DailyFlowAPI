using DailyFlow.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyFlow.DTOs
{
    public class HabitCreatingDTO
    {
        public required string Title { get; set; }
        public string Description { get; set; }
        public bool Done { get; set; }
        public int ProgrammDays { get; set; }// days frequency
        public DateOnly StartingDay { get; set; }
        public _Environment _Environment { get; set; }
        public string? UserId { get; set; }
    }
}
