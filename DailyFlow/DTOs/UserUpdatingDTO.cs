using DailyFlow.Entities;
using System.ComponentModel.DataAnnotations;

namespace DailyFlow.DTOs
{
    public class UserUpdatingDTO
    {
        public string Id { get; set; }
        public Preference? Preference { get; set; }
        
        public string? Username { get; set; }
        [EmailAddress]
        public string? UserEmail { get; set; }

        public int? streak { get; set; }
    }
}
