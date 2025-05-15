using DailyFlow.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyFlow.DTOs
{
    public class HabitDTO
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public string Description { get; set; }
        public bool Done { get; set; }
        public int ProgrammDays { get; set; }
        public DateOnly LastDay { get; set; }
        public _Environment _Environment { get; set; }
    }
}
