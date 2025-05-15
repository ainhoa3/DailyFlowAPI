using DailyFlow.Entities;

namespace DailyFlow.DTOs
{
    public class UserDTO
    {
        public int Streak { get; set; }
        public Preference Preference { get; set; }
    }
}
