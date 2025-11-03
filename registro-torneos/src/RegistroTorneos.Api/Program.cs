using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text;
using RegistroTorneos.Api.Domain;
using RegistroTorneos.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RegistroTorneos.Api", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Aquí va el token JWT (sin la palabra 'Bearer ')."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

// CORS 
var listaOrigenes = new List<string>();
var envOrigins = builder.Configuration["ALLOWED_ORIGINS"];
if (!string.IsNullOrWhiteSpace(envOrigins))
    listaOrigenes.AddRange(envOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

var cfgOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
if (cfgOrigins is { Length: >0 })
    listaOrigenes.AddRange(cfgOrigins);

builder.Services.AddCors(opc =>
{
    opc.AddPolicy("CorsFrontend", p =>
    {
        if (listaOrigenes.Count > 0)
            p.WithOrigins(listaOrigenes.ToArray()).AllowAnyHeader().AllowAnyMethod();
        else
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); // Dev
    });
});

// MongoDB
var cadenaConexionMongo =
    builder.Configuration["MONGO_CONN"] ??
    builder.Configuration["Mongo:CadenaConexion"] ??
    "mongodb://localhost:27017";

var nombreBaseDatos =
    builder.Configuration["MONGO_DB"] ??
    builder.Configuration["Mongo:BaseDatos"] ??
    "registro_torneos";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(cadenaConexionMongo));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var cliente = sp.GetRequiredService<IMongoClient>();
    return cliente.GetDatabase(nombreBaseDatos);
});

// JWT
var jwtKey =
    builder.Configuration["JWT_KEY"] ??
    builder.Configuration["Jwt:Key"] ??
    "dev-key-cambiar";

var jwtIssuer =
    builder.Configuration["JWT_ISSUER"] ??
    builder.Configuration["Jwt:Issuer"];

var jwtAudience =
    builder.Configuration["JWT_AUDIENCE"] ??
    builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
            ValidIssuer = jwtIssuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });
builder.Services.AddAuthorization();

// Repositorios
builder.Services.AddSingleton<InscripcionRepositorio>();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("CorsFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Índice único (usuarioId + torneoId)
{
    var db = app.Services.GetRequiredService<IMongoDatabase>();
    var col = db.GetCollection<Inscripcion>("inscripciones");
    var keys = Builders<Inscripcion>.IndexKeys
        .Ascending(x => x.UsuarioId)
        .Ascending(x => x.TorneoId);
    col.Indexes.CreateOne(new CreateIndexModel<Inscripcion>(
        keys, new CreateIndexOptions { Unique = true, Name = "ux_usuario_torneo" }));
}

// Estado
app.MapGet("/api/registro-torneos/estado", () =>
    Results.Ok(new { ok = true, servicio = "registro-torneos", ambiente = app.Environment.EnvironmentName, fechaUtc = DateTime.UtcNow })
).WithName("EstadoServicio");

app.MapGet("/api/registro-torneos/db/estado", async (IMongoDatabase db) =>
{
    var doc = await db.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
    bool mongoOk = doc.TryGetValue("ok", out var v) && (v.IsBoolean ? v.AsBoolean : v.ToDouble() == 1.0);
    return Results.Ok(new { ok = true, baseDatos = nombreBaseDatos, mongoOk, detalle = doc.ToJson() });
}).WithName("EstadoBaseDatos");

app.Run();
