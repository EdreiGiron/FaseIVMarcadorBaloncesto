using AuthService.Api.Data;
using AuthService.Api.Repositories;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using AspNet.Security.OAuth.GitHub;
using AuthService.Api.Repositories.Interfaces;
using AuthService.Api.Security;
using AuthService.Api.Services;
using AuthService.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AuthService.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AuthDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService.Api.Services.AuthService>();

// RSA + JWT
builder.Services.AddSingleton<RsaKeyService>();

var jwt = builder.Configuration.GetSection("Jwt");
var issuer = jwt["Issuer"];
var audience = jwt["Audience"];




// JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<RsaKeyService, IConfiguration>((o, rsaSvc, cfg) =>
    {
        var jwt = cfg.GetSection("Jwt");
        o.RequireHttpsMetadata = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsaSvc.PublicKey
        };
    });


builder.Services.AddAuthentication()
    .AddCookie("External", o =>
    {
        o.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        o.SlidingExpiration = false;
    })
    // Google
    .AddGoogle("Google", o =>
    {
        o.ClientId = builder.Configuration["OAuth:Google:ClientId"]!;
        o.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"]!;
        o.CallbackPath = "/signin-google";
        o.SignInScheme = "External";
        o.Scope.Add("email");
        o.SaveTokens = true;
    })
    // GitHub
    .AddGitHub("GitHub", o =>
    {
        o.ClientId = builder.Configuration["OAuth:GitHub:ClientId"]!;
        o.ClientSecret = builder.Configuration["OAuth:GitHub:ClientSecret"]!;
        o.CallbackPath = "/signin-github";
        o.SignInScheme = "External";
        o.Scope.Add("user:email");
        o.SaveTokens = true;
    });

// CORS
var allowed = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(o => o.AddPolicy("cors", p => p.WithOrigins(allowed).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migraciones
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.Roles.AnyAsync())
    {
        db.Roles.AddRange(
            new Role { Name = "ADMIN" },
            new Role { Name = "USER" }
        );
        await db.SaveChangesAsync();
    }
}


app.UseCors("cors");
app.UseSwagger(); app.UseSwaggerUI();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
