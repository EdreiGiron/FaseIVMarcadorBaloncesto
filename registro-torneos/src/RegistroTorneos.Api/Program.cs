using Microsoft.AspNetCore.Http.Json;
using MongoDB.Driver;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
var origenesPermitidos = (builder.Configuration["ALLOWED_ORIGINS"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(opc =>
{
    opc.AddPolicy("CorsFrontend", p =>
    {
        if (origenesPermitidos.Length > 0)
            p.WithOrigins(origenesPermitidos).AllowAnyHeader().AllowAnyMethod();
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

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Endpoints
app.MapGet("/api/registro-torneos/estado", () =>
{
    return Results.Ok(new
    {
        ok = true,
        servicio = "registro-torneos",
        ambiente = app.Environment.EnvironmentName,
        fechaUtc = DateTime.UtcNow
    });
})
.WithName("EstadoServicio");

app.MapGet("/api/registro-torneos/db/estado", async (IMongoDatabase db) =>
{
    var doc = await db.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));

    bool mongoOk = false;
    if (doc.TryGetValue("ok", out var valor))
    {
        if (valor.IsBoolean) mongoOk = valor.AsBoolean;
        else if (valor.IsNumeric) mongoOk = valor.ToDouble() == 1.0;
        else mongoOk = valor.ToString() == "1";
    }

    return Results.Ok(new
    {
        ok = true,
        baseDatos = nombreBaseDatos,
        mongoOk,
        detalle = doc.ToJson()
    });
})
.WithName("EstadoBaseDatos");

app.Run();
