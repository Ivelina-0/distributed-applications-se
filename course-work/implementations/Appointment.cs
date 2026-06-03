using System.ComponentModel.DataAnnotations;

namespace DentalClinicApp.Models;

public class Appointment
{
    public int Id { get; set; }
    [Required] public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    [Required] public int DentistId { get; set; }
    public Dentist? Dentist { get; set; }
    [Required] public DateOnly AppointmentDate { get; set; }
    [Required] public TimeOnly AppointmentTime { get; set; }
    [StringLength(500)] public string? Reason { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
