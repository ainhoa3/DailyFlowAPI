using DailyFlow.Entities;

namespace DailyFlow.Services
{
    public class TaskService
    {
        public List<_Task> TodaysTasks(List<_Task> allTasks, int planningDays = 10, int threshold = 10)
        {
            DateTime today = DateTime.Today;
            DateTime end = today.AddDays(planningDays);

            // Tareas completadas no se consideran
            var pendingTasks = allTasks.Where(t => !t.Done).ToList();

            // Atrasadas: no hechas y vencidas
            var lateTasks = pendingTasks.Where(t => t.DeuDate < today).ToList();

            // Programadas específicamente para hoy
            var scheduledToday = pendingTasks.Where(t => t.Scheduled && t.DeuDate == today).ToList();

            // Tareas en rango que no son atrasadas ni programadas
            var futureTasks = pendingTasks
                .Where(t => !t.Scheduled && t.DeuDate >= today && t.DeuDate <= end)
                .OrderBy(t => t.DeuDate)
                .ToList();

            // Crear estructura por día
            var byDay = new Dictionary<DateTime, List<_Task>>();
            for (DateTime d = today; d <= end; d = d.AddDays(1))
                byDay[d] = new List<_Task>();

            foreach (var task in futureTasks)
                byDay[task.DeuDate].Add(task);

            // Comprobar si hay algún día con más tareas del umbral
            bool needsRedistribution = byDay.Any(kv => kv.Value.Count > threshold);

            if (needsRedistribution)
            {
                // Repartir tareas desde días con exceso hacia atrás
                for (DateTime day = end; day > today; day = day.AddDays(-1))
                {
                    while (byDay[day].Count > threshold)
                    {
                        var taskToMove = byDay[day].Last();
                        byDay[day].Remove(taskToMove);

                        bool moved = false;

                        for (int i = 1; i <= 7; i++)
                        {
                            DateTime prevDay = day.AddDays(-i);
                            if (prevDay < today) break;

                            if (byDay[prevDay].Count < threshold)
                            {
                                byDay[prevDay].Add(taskToMove);
                                moved = true;
                                break;
                            }
                        }

                        if (!moved)
                        {
                            byDay[day].Add(taskToMove);
                            break;
                        }
                    }
                }
            }

            var todayTasks = byDay[today];

            // Resultado combinado
            return lateTasks
                .Concat(scheduledToday)
                .Concat(todayTasks)
                .Distinct()
                .ToList();
        }


    }
}
