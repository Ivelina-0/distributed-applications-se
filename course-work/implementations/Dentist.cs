using System.ComponentModel.DataAnnotations;

namespace DentalClinicApp.Models;

public class Dentist
{
    public int Id { get; set; }
    [Required, StringLength(100, MinimumLength = 2)] public string FullName { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 2)] public string Specialty { get; set; } = string.Empty;
    [Required, RegularExpression(@"^\+?[0-9]{8,15}$")] public string PhoneNumber { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    [StringLength(250)] public string? Bio { get; set; }
    public decimal ConsultationPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
