using Primarie.Entities;

namespace Primarie.Dtos;

public class AuthResponse
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public UserRole Role { get; set; }
    public string Token { get; set; } = "";
}
