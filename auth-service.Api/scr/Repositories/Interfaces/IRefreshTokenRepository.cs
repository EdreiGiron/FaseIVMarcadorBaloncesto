using System.Threading.Tasks;
using AuthService.Api.Models;

namespace AuthService.Api.Repositories.Interfaces;

/// <summary>
/// Contrato de persistencia para operaciones sobre <see cref="RefreshToken"/>.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Agrega un nuevo <see cref="RefreshToken"/> al contexto de datos.
    /// </summary>
    /// <param name="token">Entidad de refresh token a persistir.</param>
    Task AddAsync(RefreshToken token);

    /// <summary>
    /// Obtiene un refresh token activo (no revocado y vigente) por su valor.
    /// </summary>
    /// <param name="token">Cadena del refresh token.</param>
    /// <returns>La entidad encontrada o <c>null</c> si no existe/está inválida.</returns>
    Task<RefreshToken?> GetActiveByTokenAsync(string token);

    Task<int> RevokeAllForUserAsync(int userId);

    /// <summary>
    /// Persiste los cambios pendientes en la fuente de datos.
    /// </summary>
    Task SaveChangesAsync();
}
