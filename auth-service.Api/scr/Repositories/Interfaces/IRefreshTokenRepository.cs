using System.Threading.Tasks;
using AuthService.Api.Models;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetActiveByTokenAsync(string token);
    Task AddAsync(RefreshToken token);
    Task<int> RevokeAllForUserAsync(int userId);
    Task SaveChangesAsync();
}