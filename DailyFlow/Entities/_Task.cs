using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Xml;
using DailyFlow.ConfigClasses;

namespace DailyFlow.Entities
{
    public enum _Environment
    {
      Personal,
      Work
    }
    public class _Task
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public required string Title { get; set; }
        [Required]
        [Column(TypeName = "TEXT")]
        public required string Description { get; set; }
        [Required]
        public required DateTime DeuDate { get; set; }
        [Required]
        public required int Importance { get; set; }
        [Required]
        public _Environment _Environment { get; set; }
        public bool Done { get; set; }
        [Required]
        public required string UserId { get; set; }

        public User? User { get; set; }

        public bool IsTodays() // todays tasks are the ones with todays deu date or tomorrows deudate
        {
            DateTime today = DateTime.Today;
            return DeuDate == today || today.AddDays(1) == DeuDate;

        }
        public int DaysLeft()
        {
            var span = DeuDate.Subtract(DateTime.Today);
            return span.Days;
        }

        public float CalcualatePriority()
        {

            // constant values from app settings


            var configuration = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .AddJsonFile($"appsettings.{
                 Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                 }.json", optional: true, reloadOnChange: true)
             .Build();

            PriorityConsts priorityConsts = new PriorityConsts();
            configuration.GetSection("PriorityConsts").Bind(priorityConsts);


            /*var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            PriorityConsts priorityConsts = new PriorityConsts();
            configuration.GetSection("PriorityConsts").Bind(priorityConsts);*/

            // calculating urgency
            int urgency = 8 - DaysLeft();

            // geting users preference 
            var preference = User.Preference;

            // calculating base porcentage
            float priority = Importance + urgency;
            priority = priority / (priorityConsts.MaxImportance + priorityConsts.MaxUrgency);
            priority = priority * 100;

            // apliying preference and modifiers
            float relativeImportance = Importance / priorityConsts.MaxImportance;
            float relativeUrgency = urgency / priorityConsts.MaxUrgency;
            float maxPorcentage = 100;

            switch (preference)
            {
                case Preference.Importance:

                    if (relativeImportance >= 0.7) { priority += priorityConsts.Modifier1; }
                    maxPorcentage = 110;

                    break;
                case Preference.Balance:

                    if (relativeImportance >= 0.7) { priority += priorityConsts.Modifier1; }
                    if (relativeUrgency >= 0.7) { priority += priorityConsts.Modifier2; }
                    maxPorcentage = 121;

                    break;
                case Preference.Urgency:


                    if (relativeUrgency >= 0.7) { priority += priorityConsts.Modifier1; }
                    maxPorcentage = 110;

                    break;
            }

            // normalizing porcentage

            priority = (priority / maxPorcentage) * 100;

            return priority;
        }
    }
}

