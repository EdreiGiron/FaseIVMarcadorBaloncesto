using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RegistroTorneos.Api.Domain;

public class Inscripcion
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonElement("usuarioId")]
    public string UsuarioId { get; set; } = default!;

    [BsonElement("torneoId")]
    public string TorneoId { get; set; } = default!;

    [BsonElement("creadoUtc")]
    public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;

    [BsonElement("preferencias")]
    public Preferencias? Preferencias { get; set; }
}
