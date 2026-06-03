using System.ComponentModel.DataAnnotations;

namespace DentalClinicApp.Models;

public class Patient
{
    public int Id { get; set; }
    [Required, StringLength(100, MinimumLength = 2)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    [Required, RegularExpression(@"^\+?[0-9]{8,15}$")] public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    [StringLength(250)] public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
