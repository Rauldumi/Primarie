using System.ComponentModel.DataAnnotations;

namespace Primarie.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
