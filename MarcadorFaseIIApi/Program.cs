using MarcadorFaseIIApi.Services;
using MarcadorFaseIIApi.Models;
using MarcadorFaseIIApi.Services.Interfaces;
using MarcadorFaseIIApi.Repositories.Interfaces;
using MarcadorFaseIIApi.Repositories;
using MarcadorFaseIIApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Servicios (DI)
// -------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// EF Core
builder.Services.AddDbContext<MarcadorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Repos / Servicios propios
builder.Services.AddScoped<MarcadorService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<EquipoService>();
builder.Services.AddScoped<JugadorService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Auth JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "YourSuperSecretKey123!");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});

// -------------------------
// Build de la app
// -------------------------
var app = builder.Build();

// -------------------------
// Migraciones + inicialización
// -------------------------
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;

    // Aplica migraciones pendientes
    var db = sp.GetRequiredService<MarcadorDbContext>();
    db.Database.Migrate();

    // Inicialización al arrancar (tu lógica)
    var svc = sp.GetRequiredService<MarcadorService>();
    svc.InicializarEnCero();
}

// -------------------------
// Pipeline HTTP
// -------------------------
app.UseRouting();  
app.UseCors("CorsPolicy");

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();          

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
