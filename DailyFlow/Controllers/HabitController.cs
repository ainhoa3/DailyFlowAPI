using System.Threading.Tasks;
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
    [Route("DailyFlow/Api/Habits")]
    public class HabitController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly ServiciosUsers serviciosUsers;

        public HabitController(ApplicationDbContext context, IMapper mapper, ServiciosUsers serviciosUsers)
        {
            this.context = context;
            this.mapper = mapper;
            this.serviciosUsers = serviciosUsers;
        }

        [HttpPost("NewHabit")]
        public async Task<ActionResult> NewHabit(HabitCreatingDTO habitCreatingDTO)
        {
            if (habitCreatingDTO is null)
            {
                return BadRequest();
            }
            var user = await this.serviciosUsers.ObtenerUsuario();
            if (user is null)
            {
                return BadRequest();
            }
            habitCreatingDTO.UserId = user.Id;
            var habit = mapper.Map<Habit>(habitCreatingDTO);
            context.Habits.Add(habit);
            await context.SaveChangesAsync();
            return Ok(habitCreatingDTO);
        }

        [HttpGet("GetHabitsOfTheDayPreview")]
        public async Task<ActionResult<IEnumerable<HabitDTO>>> GetHabitsOfTheDayPreview()
        {
            var user = await this.serviciosUsers.ObtenerUsuario();
            if (user is null)
            {
                return BadRequest();
            }

            var habits = await context.Habits
                .Where(h => h.UserId == user.Id)
                .ToListAsync();
            var filteredHabits = habits.Where(t => t.IsTodays()).ToList();
            var previews = mapper.Map<IEnumerable<HabitDTO>>(habits);
            return Ok(previews);
        }

        [HttpGet("GetAHabit/{id:int}")]
        public async Task<ActionResult<HabitDTO>> GetAHabit(int id)
        {
            var user = await this.serviciosUsers.ObtenerUsuario();
            if (user is null)
            {
                return BadRequest();
            }

            var habit = await context.Habits
                .Where(h => h.UserId == user.Id)
                .FirstOrDefaultAsync(h => h.Id == id);
            if (habit is null)
            {
                return BadRequest();
            }

            var habitDTO = mapper.Map<HabitDTO>(habit);
            return Ok(habitDTO);
        }

        [HttpGet("Search/{search}")]
        public async Task<ActionResult<IEnumerable<HabitDTO>>> SearchHabitsPreview(string search)
        {
            var user = await this.serviciosUsers.ObtenerUsuario();
            if (user is null)
            {
                return BadRequest();
            }

            var habits = await context.Habits
                .Where(h => h.UserId == user.Id)
                .Where(h => h.Title.Contains(search) || h.Description.Contains(search))
                .ToListAsync();
            var previews = mapper.Map<IEnumerable<HabitDTO>>(habits);
            return Ok(previews);
        }

        [HttpPut("UpdateHabit/{id}")]
        public async Task<IActionResult> UpdateHabit(int id, HabitUpdatingDTO habitUpdatingDTO)
        {
            var habit = await context.Habits.FirstOrDefaultAsync(t => t.Id == id);
            var user = await this.serviciosUsers.ObtenerUsuario();
            if (user is null)
            {
                return BadRequest();
            }

            if (habit is null)
            {
                return BadRequest();
            }


            if (user.Id != habit.UserId)
            {
                return Forbid();
            }

            mapper.Map(habitUpdatingDTO, habit);
            context.Habits.Update(habit);
            await context.SaveChangesAsync();
            return Ok(habit);
        }
      
        [HttpDelete("DeleteHabit/{id:int}")]
        public async Task<ActionResult> DeleteHabit(int id)
        {
            var user = await serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }

            var habit = await context.Habits.FirstOrDefaultAsync(h => h.Id == id);

            if (habit.UserId != user.Id)
            {
                return Forbid();
            }

            context.Remove(user);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
    
}
