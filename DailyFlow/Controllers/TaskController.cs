using AutoMapper;
using DailyFlow.Data;
using DailyFlow.DTOs;
using DailyFlow.Entities;
using DailyFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyFlow.Controllers
{
    [Authorize]
    [ApiController]
    [Route("DailyFlow/Api/Tasks")]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly ServiciosUsers serviciosUsers;

        public TaskController(ApplicationDbContext context, IMapper mapper, ServiciosUsers serviciosUsers)
        {
            this.context = context;
            this.mapper = mapper;
            this.serviciosUsers = serviciosUsers;
        }

        [HttpPost("NewTask")]
        public async Task<ActionResult> NewTask(TaskCreatingDTO taskCreatingDTO)
        {
            if (taskCreatingDTO is null)
            {
                return BadRequest();
            }
            var user = await this.serviciosUsers.ObtenerUsuario();
            taskCreatingDTO.UserId = user.Id;


            var task = mapper.Map<_Task>(taskCreatingDTO);

            context.Tasks.Add(task);
            await context.SaveChangesAsync();
            return Ok(taskCreatingDTO);
        }
        [HttpGet("GetTasksOfTheDayPreview")]
        public async Task<ActionResult<IEnumerable<TaskPreviewDTO>>> GetTasksOfTheDayPreview()
        {
            var user = await this.serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }
            var tasks = await context.Tasks
                .Where(t => t.UserId == user.Id)
                .ToListAsync();

            var filteredTasks = tasks.Where(t => t.IsTodays()).ToList();
            var FilteredAndOrderedTasks = filteredTasks.OrderBy(t => t.CalcualatePriority()).ToList();

            var previews = mapper.Map<IEnumerable<TaskPreviewDTO>>(FilteredAndOrderedTasks);
            return Ok(previews);
        }

        [HttpGet("GetAtask/{id:int}")]
        public async Task<ActionResult<TaskDTO>> GetAtask(int id)
        {
            var user = await this.serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }
            var tasks = await context.Tasks
                .Where(t => t.UserId == user.Id)
                .FirstOrDefaultAsync(t => t.Id == id);
           if (tasks is null)
           {
              return BadRequest();
           }
                
            var previews = mapper.Map<TaskDTO>(tasks);
            return Ok(previews);
        }

        [HttpGet("Search/{search}")]
        public async Task<ActionResult<IEnumerable<TaskPreviewDTO>>> SearchTasksPreview(string search)
        {
            var user = await this.serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }
            var tasks = await context.Tasks
                .Where(t => t.UserId == user.Id)
                .Where(t => t.Title.Contains(search) || 
                t.Description.Contains(search))
                .ToListAsync();

           
            var previews = mapper.Map<IEnumerable<TaskPreviewDTO>>(tasks);
            return Ok(previews);
        }

        [HttpGet("GetTasksByDatePreview/{date:DateTime}")]
        public async Task<ActionResult<IEnumerable<TaskPreviewDTO>>> GetTasksByDatePreview(DateTime date)
        {
            var user = await this.serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }
            var tasks = await context.Tasks
                .Where(t => t.UserId == user.Id)
                .Where(t => t.DeuDate == date)
                .ToListAsync();
            var previews = mapper.Map<IEnumerable<TaskPreviewDTO>>(tasks);
            return Ok(previews);
        }
        [HttpGet("GetExtraTasks")]
        public async Task<ActionResult<IEnumerable<TaskPreviewDTO>>> ExtraTasks()
        {
            var user = await this.serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }
            var tasks = await context.Tasks
                .Where(t => t.UserId == user.Id)
                .ToListAsync();

            var filteredTasks = tasks.Where(t => t.DaysLeft() > 2 && t.DaysLeft() < 8)
                // tasks from the day ufter tomorrow to a week(7 days) ufter
                // tomorrow(8 days from today)
                .OrderBy(t => t.CalcualatePriority()).ToList();

            var previews = mapper.Map<IEnumerable<TaskPreviewDTO>>(filteredTasks);
            return Ok(previews);
        }


        [HttpPut("UpdateTask/{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskUpdatingDTO taskUpdatingDTO)
        {
            var task = await context.Tasks.FindAsync(id);
            var user = await serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }
            
            if (task is null)
            {
                return BadRequest();
            }

            mapper.Map(taskUpdatingDTO, task);
            
            context.Tasks.Update(task);
            await context.SaveChangesAsync();
            
            return Ok(task);
        }

        [HttpGet("MarkAsDone/{id}")]
        public async Task<IActionResult> MarkAsDone(int id)
        {
            var user = await serviciosUsers.ObtenerUsuario();
            if (user is null)
            {
                return BadRequest();
            }

            var task = await context.Tasks.FindAsync(id);
            if (task is null)
            {
                return BadRequest();
            }

            task.Done = true;
            context.Tasks.Update(task);
            await context.SaveChangesAsync();

            return Ok(task);
        }

    }
}
