using System.ComponentModel.DataAnnotations;
using DailyFlow.Entities;

namespace DailyFlow.DTOs
{
    public class CredencialesUserDTO
    {
        [Required]
        [EmailAddress]
        public required string Email {  get; set; }
      
        public string? UserName { get; set; }

        public  string? Password { get; set; }
        [Required]
        public Preference Preference { get; set; } = Preference.Balance;
        public int Streak { get; set; } = 0;

    }
}
