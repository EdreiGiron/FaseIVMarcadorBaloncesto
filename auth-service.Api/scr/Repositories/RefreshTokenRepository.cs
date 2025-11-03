﻿using System;
using System.Threading.Tasks;
using AuthService.Api.Data;
using AuthService.Api.Models;
using AuthService.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Repositories;

public class RefreshTokenRepository(AuthDbContext ctx) : IRefreshTokenRepository
{
    private readonly AuthDbContext _ctx = ctx;

    public Task<RefreshToken?> GetActiveByTokenAsync(string token) =>
        _ctx.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow);
    public async Task AddAsync(RefreshToken token) => await _ctx.RefreshTokens.AddAsync(token);

    public async Task<int> RevokeAllForUserAsync(int userId)
    {
        var tokens = await _ctx.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow).ToListAsync();
        foreach (var t in tokens) t.IsRevoked = true;
        return tokens.Count;
    }

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}