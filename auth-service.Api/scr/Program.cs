using AuthService.Api.Data;
using AuthService.Api.Repositories;
using AspNet.Security.OAuth.GitHub;
using AuthService.Api.Repositories.Interfaces;
using AuthService.Api.Security;
using AuthService.Api.Services;
using AuthService.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AuthService.Api.Models;
using Microsoft.AspNetCore.HttpOverrides;

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

// 1) Registrar el esquema por defecto = JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// 2) Configurar validación JWT con la clave pública RSA
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

// 3) Proveedores externos + cookie temporal "External"
builder.Services.AddAuthentication()
    .AddCookie("External", o =>
    {
        o.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        o.SlidingExpiration = false;
    })
    .AddGoogle("Google", o =>
    {
        o.ClientId = builder.Configuration["OAuth:Google:ClientId"]!;
        o.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"]!;
        o.CallbackPath = "/auth/signin-google";
        o.SignInScheme = "External";
        o.Scope.Add("email");
        o.SaveTokens = true;
    })
    .AddGitHub("GitHub", o =>
    {
        o.ClientId = builder.Configuration["OAuth:GitHub:ClientId"]!;
        o.ClientSecret = builder.Configuration["OAuth:GitHub:ClientSecret"]!;
        o.CallbackPath = "/auth/signin-github";
        o.SignInScheme = "External";
        o.Scope.Add("user:email");
        o.SaveTokens = true;
    });

// CORS
var allowed = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(o => o.AddPolicy("cors",
    p => p.WithOrigins(allowed).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migraciones + seed
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

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors("cors");
app.UseAuthentication();
app.UseAuthorization();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
});


app.MapControllers();
app.Run();
