﻿using System;
using System.Threading.Tasks;
using AuthService.Api.Data;
using AuthService.Api.Models;
using AuthService.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Repositories;

/// <summary>
/// Implementación de <see cref="IRefreshTokenRepository"/> con EF Core.
/// Gestiona la persistencia de <see cref="RefreshToken"/> y su consulta por token activo.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly MarcadorDbContext _ctx;
    /// <summary>
    /// Crea una instancia del repositorio usando el contexto de datos.
    /// </summary>
    /// <param name="ctx">Contexto EF Core de la aplicación.</param>
    public RefreshTokenRepository(MarcadorDbContext ctx) { _ctx = ctx; }

    /// <summary>
    /// Busca un refresh token activo por su valor (no revocado y con fecha de expiración futura).
    /// </summary>
    /// <param name="token">Cadena del refresh token.</param>
    /// <returns>La entidad encontrada o <c>null</c> si no está activa/registrada.</returns>
    public Task<RefreshToken?> GetActiveByTokenAsync(string token) =>
    _ctx.RefreshTokens.FirstOrDefaultAsync(r =>
        r.Token == token &&
        !r.IsRevoked &&
        r.ExpiresAt > DateTime.UtcNow);

    /// <summary>
    /// Agrega un nuevo refresh token al contexto.
    /// </summary>
    /// <param name="token">Entidad de refresh token a persistir.</param>
    public async Task AddAsync(RefreshToken token) => await _ctx.RefreshTokens.AddAsync(token);
    // <<--- NUEVO
    public async Task<int> RevokeAllForUserAsync(int userId)
    {
        var tokens = await _ctx.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var t in tokens) t.IsRevoked = true;

        // NO guardar aquí: lo hace el servicio para agrupar transacción
        return tokens.Count;
    }

    /// <summary>
    /// Persiste los cambios pendientes en la base de datos.
    /// </summary>
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
