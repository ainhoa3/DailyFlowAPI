﻿using AutoMapper;
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
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;


        public UsuariosController(UserManager<User> userManager, IConfiguration configuration,
            SignInManager<User> signInManager, ServiciosUsers serviciosUsers,
            ApplicationDbContext context, IMapper mapper)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.serviciosUsers = serviciosUsers;
            this.context = context;
            this.mapper = mapper;
        }


        [HttpPost("RegistroUsuario")]
      
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Register(CredencialesUserDTO credencialesUserDTO)
        {
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

            var tokenDeSeguridad = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion);
           
            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad);

            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion,
            };
        }

        [HttpGet("users")]
        public async Task<IEnumerable<UserDTO>> Get()
        {
            var usuarios = await context.Users.ToListAsync();
            var usuariosDTO = mapper.Map<IEnumerable<UserDTO>>(usuarios);

            return usuariosDTO;
        }
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
        [HttpGet("AddStrike")]
        public async Task<ActionResult> AddStrike()
        {
            var user = await serviciosUsers.ObtenerUsuario();

            if (user is null)
            {
                return BadRequest();
            }

            user.Streak += 1;

            context.Users.Update(user);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
