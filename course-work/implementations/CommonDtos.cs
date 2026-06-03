using DentalClinicApp.Models;
using System.ComponentModel.DataAnnotations;

namespace DentalClinicApp.DTOs;

public record RegisterRequest([Required, StringLength(100, MinimumLength=2)] string FullName, [Required, EmailAddress] string Email, [Required, MinLength(8)] string Password, [Required, RegularExpression(@"^\+?[0-9]{8,15}$")] string PhoneNumber, UserRole Role);
public record LoginRequest([Required, EmailAddress] string Email, [Required, MinLength(8)] string Password);
public record LoginResponse(string Token, int UserId, string FullName, UserRole Role);
public record UserResponse(int Id, string FullName, string Email, string PhoneNumber, UserRole Role);
public record ErrorResponse(int StatusCode, string Message, IEnumerable<string>? Errors = null);
public record MessageResponse(string Message);
public record PagedResponse<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize);

public record PatientRequest([Required, StringLength(100, MinimumLength=2)] string FullName, [Required, EmailAddress] string Email, [Required, RegularExpression(@"^\+?[0-9]{8,15}$")] string PhoneNumber, DateOnly? BirthDate, string? Address);
public record PatientResponse(int Id, string FullName, string Email, string PhoneNumber, DateOnly? BirthDate, string? Address, bool IsActive);

public record DentistRequest([Required, StringLength(100, MinimumLength=2)] string FullName, [Required, StringLength(100, MinimumLength=2)] string Specialty, [Required, RegularExpression(@"^\+?[0-9]{8,15}$")] string PhoneNumber, [Required, EmailAddress] string Email, string? Bio, decimal ConsultationPrice);
public record DentistResponse(int Id, string FullName, string Specialty, string PhoneNumber, string Email, string? Bio, decimal ConsultationPrice, bool IsActive);

public record ScheduleRequest([Required] int DentistId, [Required] DateOnly Date, [Required] TimeOnly StartTime, [Required] TimeOnly EndTime, bool IsAvailable, string? Notes);
public record ScheduleResponse(int Id, int DentistId, string DentistName, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, bool IsAvailable, string? Notes);

public record AppointmentRequest([Required] int PatientId, [Required] int DentistId, [Required] DateOnly AppointmentDate, [Required] TimeOnly AppointmentTime, string? Reason);
public record AppointmentResponse(int Id, int PatientId, string PatientName, string PatientPhone, int DentistId, string DentistName, DateOnly AppointmentDate, TimeOnly AppointmentTime, string? Reason, AppointmentStatus Status);
public record AppointmentHistoryResponse(DateOnly AppointmentDate, TimeOnly AppointmentTime, string DentistName, AppointmentStatus Status, string? Reason);
