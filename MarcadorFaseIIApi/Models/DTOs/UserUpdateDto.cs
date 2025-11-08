namespace MarcadorFaseIIApi.Models.DTOs
{
    public class UserUpdateDto
    {
        public string? Username { get; set; }
        public string? Password { get; set; } // opcional para que no siempre cambie
        public int? RoleId { get; set; }
    }
}
