using System.ComponentModel.DataAnnotations;

namespace DentalClinicApp.Models;

public class Schedule
{
    public int Id { get; set; }
    [Required] public int DentistId { get; set; }
    public Dentist? Dentist { get; set; }
    [Required] public DateOnly Date { get; set; }
    [Required] public TimeOnly StartTime { get; set; }
    [Required] public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    [StringLength(200)] public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
