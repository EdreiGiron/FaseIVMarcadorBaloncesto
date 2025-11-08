namespace MarcadorFaseIIApi.Models.DTOs;

public class RegisterResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
