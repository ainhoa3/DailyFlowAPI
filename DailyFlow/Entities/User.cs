using Microsoft.AspNetCore.Identity;

namespace DailyFlow.Entities
{
    public class User : IdentityUser
    {
        public int Streak { get; set; }
        public Preference Preference { get; set; }


    }

    public enum Preference
    {
        Importance,
        Balance,
        Urgency
    }
}
