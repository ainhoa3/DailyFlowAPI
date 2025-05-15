using System.Text;
using DailyFlow.Data;
using DailyFlow.Entities;
using DailyFlow.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
    policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowCredentials()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddDataProtection();

builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer("name=DefaultConnection"));

builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<User>>();
builder.Services.AddScoped<SignInManager<User>>();

builder.Services.AddIdentity<User, IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();//permite acceder al contexto http desde cualquier clase

builder.Services.AddAuthentication().AddJwtBearer(opciones =>
{
    opciones.MapInboundClaims = false;// deshabilita que se cambie el nombre de un clame por otro que provee el entity
    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        // aqui especificamos los parametros a tener en cuenta a ala hora de validar un token
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["llavejwt"])),
        ClockSkew = TimeSpan.Zero,// tiempo de espiracion del token
    };
});


builder.Services.AddScoped<ServiciosUsers>(); // Registro del servicio ServiciosUsers

builder.Services.AddControllers().AddNewtonsoftJson();



builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


app.UseCors("AllowSpecificOrigins");

app.MapControllers();

app.Run();