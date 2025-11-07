using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using MongoDB.Bson;
using MongoDB.Driver;

using RegistroTorneos.Api.Domain;
using RegistroTorneos.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

//JSON
builder.Services.AddControllers();
builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

//Swagger y Bearer
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
        Description = "Escribe solamente el token JWT (sin 'Bearer ')."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        },
        Array.Empty<string>()
    }
});
});

//CORS
var listaOrigenes = new List<string>();
var envOrigins = builder.Configuration["ALLOWED_ORIGINS"];
if (!string.IsNullOrWhiteSpace(envOrigins))
    listaOrigenes.AddRange(envOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

var cfgOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
if (cfgOrigins is { Length: > 0 })
    listaOrigenes.AddRange(cfgOrigins);

// Desarrollo por defecto: localhost:4200
if (listaOrigenes.Count == 0 && builder.Environment.IsDevelopment())
    listaOrigenes.Add("http://localhost:4200");

builder.Services.AddCors(opc =>
{
    opc.AddPolicy("CorsFrontend", p =>
    p.WithOrigins(listaOrigenes.ToArray())
    .AllowAnyHeader()
    .AllowAnyMethod());
});

//MongoDB
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

// Cliente HTTP para api_torneos
var baseUrlTorneos =
builder.Configuration["TORNEOS__BASEURL"] ??
builder.Configuration["Torneos:BaseUrl"] ??
"http://127.0.0.1:8083";

builder.Services.AddHttpClient<TorneosClient>(c =>
{
c.BaseAddress = new Uri(baseUrlTorneos);
});

//JWT / Auth
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var jwtKey =
builder.Configuration["JWT_KEY"] ??
builder.Configuration["Jwt:Key"] ??
builder.Configuration["Jwt__Key"] ??
"dev-key-cambiar";

var jwtIssuer =
builder.Configuration["JWT_ISSUER"] ??
builder.Configuration["Jwt:Issuer"] ??
builder.Configuration["Jwt__Issuer"];

var jwtAudience =
builder.Configuration["JWT_AUDIENCE"] ??
builder.Configuration["Jwt:Audience"] ??
builder.Configuration["Jwt__Audience"];

builder.Services
.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false; // dev
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

builder.Services.AddSingleton<InscripcionRepositorio>();

var app = builder.Build();

//Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("CorsFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Índice único
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
Results.Ok(new
{
    ok = true,
    servicio = "registro-torneos",
    ambiente = app.Environment.EnvironmentName,
    fechaUtc = DateTime.UtcNow
}))
.WithName("EstadoServicio");

// Estado DB
app.MapGet("/api/registro-torneos/db/estado", async (IMongoDatabase db) =>
{
    var doc = await db.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
    bool mongoOk = doc.TryGetValue("ok", out var v) && (v.IsBoolean ? v.AsBoolean : v.ToDouble() == 1.0);
    return Results.Ok(new
    {
        ok = true,
        baseDatos = nombreBaseDatos,
        mongoOk,
        detalle = doc.ToJson()
    });

})
.WithName("EstadoBaseDatos");

app.MapGet("/api/registro-torneos/_whoami", [Microsoft.AspNetCore.Authorization.Authorize] (ClaimsPrincipal user) =>
{
    var claims = user.Claims.Select(c => new { c.Type, c.Value });
    return Results.Ok(new { ok = true, claims });
})
.WithName("WhoAmI");

app.Run();