using System.ComponentModel.DataAnnotations;

namespace DentalClinicApp.Models;

public class User
{
    public int Id { get; set; }
    [Required, StringLength(100, MinimumLength = 2)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    [Required, RegularExpression(@"^\+?[0-9]{8,15}$")] public string PhoneNumber { get; set; } = string.Empty;
    [Required] public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
