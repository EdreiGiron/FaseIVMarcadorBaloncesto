using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// --- Configuración de servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var origenesPermitidos = (builder.Configuration["ALLOWED_ORIGINS"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(opc =>
{
    opc.AddPolicy("CorsFrontend", p =>
    {
        if (origenesPermitidos.Length > 0)
            p.WithOrigins(origenesPermitidos).AllowAnyHeader().AllowAnyMethod();
        else
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

//Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsFrontend");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/registro-torneos/estado", () =>
{
    var ahora = DateTime.UtcNow;
    return Results.Ok(new
    {
        ok = true,
        servicio = "registro-torneos",
        ambiente = app.Environment.EnvironmentName,
        fechaUtc = ahora
    });
})
.WithName("EstadoServicio");

app.Run();
