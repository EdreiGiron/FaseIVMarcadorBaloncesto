using MongoDB.Driver;
using RegistroTorneos.Api.Domain;

namespace RegistroTorneos.Api.Infrastructure;

public class InscripcionRepositorio
{
    private readonly IMongoCollection<Inscripcion> _coleccion;

    public InscripcionRepositorio(IMongoDatabase db)
    {
        _coleccion = db.GetCollection<Inscripcion>("inscripciones");
    }

    public Task<bool> ExisteAsync(string usuarioId, string torneoId) =>
        _coleccion.Find(x => x.UsuarioId == usuarioId && x.TorneoId == torneoId).AnyAsync();

    public async Task InsertarAsync(Inscripcion entidad)
    {
        await _coleccion.InsertOneAsync(entidad);
    }

    public Task<List<Inscripcion>> ListarPorUsuarioAsync(string usuarioId) =>
        _coleccion.Find(x => x.UsuarioId == usuarioId)
                  .SortByDescending(x => x.CreadoUtc)
                  .ToListAsync();

    public async Task<bool> EliminarAsync(string usuarioId, string torneoId)
    {
        var r = await _coleccion.DeleteOneAsync(x => x.UsuarioId == usuarioId && x.TorneoId == torneoId);
        return r.DeletedCount > 0;
    }
}
