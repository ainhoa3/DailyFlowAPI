using DailyFlow.Entities;

namespace DailyFlow.DTOs
{
    public class UserDTO
    {
        public string Id { get; set; }
        public int Streak { get; set; }
        public Preference Preference { get; set; }
    }
}
