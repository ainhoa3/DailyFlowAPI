using System.ComponentModel.DataAnnotations.Schema;

namespace DailyFlow.Entities
{
    public class Habit
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        [Column(TypeName = "TEXT")]
        public string Description { get; set; }
        public bool Done { get; set; }
        public int ProgrammDays { get; set; }
        public DateOnly LastDay { get; set; }
        public _Environment _Environment { get; set; }
        public required string UserId { get; set; }
        public User? User { get; set; }

        public bool IsTodays()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            if(LastDay.AddDays(ProgrammDays) == today)
            {
                this.LastDay = today;
                return true;
            }
             
            return false;
        }
    }
}


