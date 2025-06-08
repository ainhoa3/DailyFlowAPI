using AutoMapper;
using DailyFlow.Data;
using DailyFlow.Entities;
using DailyFlow.DTOs;
using DailyFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("DailyFlow/api/users")]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<User> signInManager;
        private readonly ServiciosUsers serviciosUsers;
        private readonly TaskService taskService;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;


        public UsuariosController(UserManager<User> userManager, IConfiguration configuration,
            SignInManager<User> signInManager, ServiciosUsers serviciosUsers,
            ApplicationDbContext context, IMapper mapper, TaskService taskService)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.serviciosUsers = serviciosUsers;
            this.context = context;
            this.mapper = mapper;
            this.taskService = taskService;
        }

        [AllowAnonymous]
        [HttpPost("RegistroUsuario")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Register(CredencialesUserDTO credencialesUserDTO)
        {
            var emailExist = await context.Users.AnyAsync(x => x.Email == credencialesUserDTO.Email);
            if (emailExist)
            {
                return BadRequest("Email already exists");
            }

            var user = new User
            {
                UserName = credencialesUserDTO.UserName,
                Email = credencialesUserDTO.Email,
                Preference = credencialesUserDTO.Preference
            };

            var resultado = await userManager.CreateAsync(user, credencialesUserDTO!.Password);
            
            if (resultado.Succeeded)
            {
               
                var respuestaAutenticacion = await ConstruirToken(credencialesUserDTO, user.Id);
               
                return respuestaAutenticacion;
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return ValidationProblem();
            }
        }
        [AllowAnonymous]
        [HttpPost("LoginUsuario")] 
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(CredencialesUserDTO credencialesUserDTO)
        {
            var user = await userManager.FindByEmailAsync(credencialesUserDTO.Email);

            if (user is null)
            {
                return RetornarLoginIncorecto();
            }
             
            var resultado = await signInManager.CheckPasswordSignInAsync(user,credencialesUserDTO.Password,lockoutOnFailure:false);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUserDTO, user.Id);
            }
            else
            {
                return RetornarLoginIncorecto();
            }
        }
        [Authorize]
        [HttpGet("RenovarToken")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> RenovarToken()
        {
            var usuario = await serviciosUsers.ObtenerUsuario();

            if (usuario is null)
            {
                return BadRequest();
            }

            var credencialesUserDTO = new CredencialesUserDTO { Email = usuario.Email! };

            var respuestaAutenticacion = await ConstruirToken(credencialesUserDTO,usuario.Id);
            return respuestaAutenticacion;
        }
        private ActionResult RetornarLoginIncorecto()
        {
            ModelState.AddModelError(string.Empty, "LogIn incorrecto");
            return ValidationProblem();
        }

        private async Task<RespuestaAutenticacionDTO> ConstruirToken(CredencialesUserDTO credencialesUserDTO, string usuarioId)
        {
            var claims = new List<Claim>
            {
                new Claim("email", credencialesUserDTO.Email),
                new Claim("usuarioId",usuarioId),
                new Claim("preference",credencialesUserDTO.Preference.ToString())
            };

            var usuario = await userManager.FindByEmailAsync(credencialesUserDTO.Email);
            var claimsDB = await userManager.GetClaimsAsync(usuario!);

            claims.AddRange(claimsDB);
            
            var expiracion = DateTime.UtcNow.AddDays(10);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));

            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var tokenDeSeguridad = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiracion,
                signingCredentials: creds
            );


            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad);

            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion,
            };
        }

        
        [Authorize]
        [HttpGet("streak")]
        public async Task<ActionResult> Strike()
        {
            var user = await serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }


            return Ok(user.Streak);
        }
        [Authorize]
        [HttpGet("AddStrike")]
        public async Task<ActionResult> AddStrike()
        {
            var user = await serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }

            var tasks = await context.Tasks
               .Where(t => t.UserId == user.Id && t.Done != true)
               .ToListAsync();

            var tasksForToday = taskService.TodaysTasks(tasks);

            var habits = await context.Habits
               .Where(h => h.UserId == user.Id && h.Done != true)
               .ToListAsync();
            var habitsForToday = habits.Where(h => h.IsTodays()).ToList();

            // no strike is added if ther has already been 1 added today or there are steel tasks for today not done or there are habits for today not done
            if (
                user.LastStreak.Date == DateTime.Now.Date ||
                tasksForToday.Any(t => t.Done != true) || habitsForToday.Any(t => t.Done != true))
            {
               return NoContent();
            }
            var newStreak = new Streaks();

            //if last date + 1 day is earlyer than today that means that last date is not yestorday so that means that the streak has been broken the day ufter last date
            if (user.LastStreak.Date.AddDays(1) < DateTime.Now.Date)
            {
                // Día en que se rompió la racha
                var breakDate = user.LastStreak.Date.AddDays(1);

                user.Streak = 0;
                user.LastStreak = breakDate;

                var brokenStreak = new Streaks
                {
                    date = breakDate,
                    streak = 0,
                    userId = user.Id
                };

                context.Streaks.Add(brokenStreak);
            }

            user.Streak += 1;
            user.LastStreak = DateTime.Now;
            newStreak = new Streaks { date = DateTime.Today, streak = user.Streak, userId = user.Id };
            
            context.Streaks.Add(newStreak);

            context.Users.Update(user);
            await context.SaveChangesAsync();

            return Ok(newStreak);
        }
        [Authorize]
        [HttpPost("UpdateUser")]
        public async Task<ActionResult> UpdateUserPreferenc(UserUpdatingDTO userDTO)
        {
            var user = await serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }
            if (user.Id != userDTO.Id)
            {
                return Forbid();
            }

            if (userDTO.Username is not null)
            {
                user.UserName = userDTO.Username;
            }

            if (userDTO.UserEmail is not null)
            {
                var emailExist = await context.Users.AnyAsync(x => x.Email == userDTO.UserEmail);
                if (emailExist)
                {
                    return BadRequest("Email already exists");
                }
                user.Email = userDTO.UserEmail;

            }

            if (userDTO.Preference is not null)
            {
                user.Preference = (Preference)userDTO.Preference;
            }

            
            context.Users.Update(user);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize]
        [HttpGet("GetAllStrikes")]
        public async Task<ActionResult> GetAllStrikes()
        {
            var user = await serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }

            var history = await context.Streaks.Where(s => s.userId == user.Id).ToListAsync();
           
            var srtreakDTOs = mapper.Map<IEnumerable<StrikeDTO>>(history);

            return Ok(srtreakDTOs);
        }

        [Authorize]
        [HttpGet("GetStrikesByMonth/{year:int}/{month:int}")]
        public async Task<ActionResult> GetStrikesByMonth( int year, int month)
        {
            var user = await serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }

            var history = await context.Streaks.Where(s => s.userId == user.Id && s.date.Year == year && s.date.Month == month)
                .ToListAsync();

            var srtreakDTOs = mapper.Map<IEnumerable<StrikeDTO>>(history);

            return Ok(srtreakDTOs);
        }

        [Authorize]
        [HttpGet("GetStrikesByYear/{year:int}")]
        public async Task<ActionResult> GetStrikesByYear(int year)
        {
            var user = await serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }

            var history = await context.Streaks.Where(s => s.userId == user.Id && s.date.Year == year)
                .ToListAsync();

            var srtreakDTOs = mapper.Map<IEnumerable<StrikeDTO>>(history);

            return Ok(srtreakDTOs);
        }

        [Authorize]
        [HttpDelete("DeleteUser")]
        public async Task<ActionResult> DeleteUser()
        {
            var user = await serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }

            context.Remove(user);
            await context.SaveChangesAsync();

            return NoContent();
        }
        [Authorize]
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult> GetCurrentUser()
        {
            var user = await serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }

            return Ok(new UserUpdatingDTO { Id = user.Id, Preference = user.Preference, 
                UserEmail = user.Email,Username = user.UserName, streak =user.Streak });
        }
    }
}
