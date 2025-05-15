using DailyFlow.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DailyFlow.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<_Task>().HasQueryFilter(b => !b.Done);
            //modelBuilder.Entity<_Task>().HasQueryFilter(b => b.DaysLeft() <= 7);
            //crea un filtro general que cada vez que se consulten las tareas solo aparecerán las no hechas de menos de 7 días 
        }

        public DbSet<_Task> Tasks { get; set; }
        public DbSet<Habit> Habits { get; set; }
        
    }
}
