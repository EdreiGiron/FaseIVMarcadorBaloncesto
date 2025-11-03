using AuthService.Api.Models.DTOs;

namespace AuthService.Api.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<object>> GetAllAsync();
}