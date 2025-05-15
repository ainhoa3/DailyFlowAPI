using System.IdentityModel.Tokens.Jwt;
using DailyFlow.Entities;
using Microsoft.AspNetCore.Identity;

namespace DailyFlow.Services
{
    public class ServiciosUsers
    {
        private readonly UserManager<User> userManager;
        private readonly IHttpContextAccessor contextAccessor;

        public ServiciosUsers(UserManager<User> userManager, IHttpContextAccessor contextAccessor)
        {
            this.userManager = userManager;
            this.contextAccessor = contextAccessor;
        }

        string? ObtenerUsuarioId()
        {
            var idClaim = contextAccessor.HttpContext!.User.Claims.Where(x => x.Type == "usuarioId")
                .FirstOrDefault();

            if (idClaim is null)
            {
                return null;
            }
            var email = idClaim.Value;
            return email;
        }


        public string GetEmailFromToken()
        {
            var authorizationHeader = contextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader != null && authorizationHeader.StartsWith("Bearer "))
            {
                var token = authorizationHeader.Substring("Bearer ".Length).Trim();
                var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
                var emailClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value;
                return emailClaim;
            }
            return null;
        }



        public async Task<User?> ObtenerUsuario()
        {
            var emailClaim = GetEmailFromToken();

            if (emailClaim is null)
            {
                return null;
            }

            return await userManager.FindByEmailAsync(emailClaim);

        }
    }
}
