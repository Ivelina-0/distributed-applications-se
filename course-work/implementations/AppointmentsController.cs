using DentalClinicApp.Data;
using DentalClinicApp.DTOs;
using DentalClinicApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicApp.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AppointmentsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AppointmentResponse>>> Get(
        int? dentistId, int? patientId, DateOnly? date, AppointmentStatus? status,
        int page = 1, int pageSize = 10, string? sortBy = "appointmentDate", string sortOrder = "asc")
    {
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 100);
        var q = _db.Appointments.Include(x => x.Patient).Include(x => x.Dentist).AsQueryable();
        if (dentistId.HasValue) q = q.Where(x => x.DentistId == dentistId);
        if (patientId.HasValue) q = q.Where(x => x.PatientId == patientId);
        if (date.HasValue) q = q.Where(x => x.AppointmentDate == date);
        if (status.HasValue) q = q.Where(x => x.Status == status);

        q = (sortBy, sortOrder.ToLower()) switch
        {
            ("appointmentTime", "desc") => q.OrderByDescending(x => x.AppointmentTime),
            ("appointmentTime", _) => q.OrderBy(x => x.AppointmentTime),
            ("dentistName", "desc") => q.OrderByDescending(x => x.Dentist!.FullName),
            ("dentistName", _) => q.OrderBy(x => x.Dentist!.FullName),
            ("patientName", "desc") => q.OrderByDescending(x => x.Patient!.FullName),
            ("patientName", _) => q.OrderBy(x => x.Patient!.FullName),
            ("status", "desc") => q.OrderByDescending(x => x.Status),
            ("status", _) => q.OrderBy(x => x.Status),
            (_, "desc") => q.OrderByDescending(x => x.AppointmentDate).ThenByDescending(x => x.AppointmentTime),
            _ => q.OrderBy(x => x.AppointmentDate).ThenBy(x => x.AppointmentTime)
        };

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new AppointmentResponse(x.Id, x.PatientId, x.Patient!.FullName, x.Patient.PhoneNumber,
                x.DentistId, x.Dentist!.FullName, x.AppointmentDate, x.AppointmentTime, x.Reason, x.Status))
            .ToListAsync();
        return Ok(new PagedResponse<AppointmentResponse>(items, total, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentResponse>> GetById(int id)
    {
        var x = await _db.Appointments.Include(a => a.Patient).Include(a => a.Dentist).FirstOrDefaultAsync(a => a.Id == id);
        if (x == null) return NotFound(new ErrorResponse(404, "Appointment not found"));
        return Ok(new AppointmentResponse(x.Id, x.PatientId, x.Patient!.FullName, x.Patient.PhoneNumber,
            x.DentistId, x.Dentist!.FullName, x.AppointmentDate, x.AppointmentTime, x.Reason, x.Status));
    }

    [HttpPost]
    public async Task<IActionResult> Create(AppointmentRequest r)
    {
        var error = await ValidateAppointment(r);
        if (error != null) return BadRequest(new ErrorResponse(400, error));
        if (await _db.Appointments.AnyAsync(x => x.DentistId == r.DentistId && x.AppointmentDate == r.AppointmentDate && x.AppointmentTime == r.AppointmentTime && x.Status == AppointmentStatus.Scheduled))
            return Conflict(new ErrorResponse(409, "Appointment time is already booked"));
        var x = new Appointment { PatientId = r.PatientId, DentistId = r.DentistId, AppointmentDate = r.AppointmentDate, AppointmentTime = r.AppointmentTime, Reason = r.Reason, Status = AppointmentStatus.Scheduled };
        _db.Appointments.Add(x);
        await _db.SaveChangesAsync();
        return Ok(new MessageResponse("Appointment created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AppointmentRequest r)
    {
        var x = await _db.Appointments.FindAsync(id);
        if (x == null) return NotFound(new ErrorResponse(404, "Appointment not found"));
        var error = await ValidateAppointment(r);
        if (error != null) return BadRequest(new ErrorResponse(400, error));
        if (await _db.Appointments.AnyAsync(a => a.Id != id && a.DentistId == r.DentistId && a.AppointmentDate == r.AppointmentDate && a.AppointmentTime == r.AppointmentTime && a.Status == AppointmentStatus.Scheduled))
            return Conflict(new ErrorResponse(409, "New appointment time is unavailable"));
        x.PatientId = r.PatientId; x.DentistId = r.DentistId; x.AppointmentDate = r.AppointmentDate; x.AppointmentTime = r.AppointmentTime; x.Reason = r.Reason; x.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new MessageResponse("Appointment updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var x = await _db.Appointments.FindAsync(id);
        if (x == null) return NotFound(new ErrorResponse(404, "Appointment not found"));
        if (x.Status == AppointmentStatus.Completed) return Conflict(new ErrorResponse(409, "Completed appointment cannot be cancelled"));
        x.Status = AppointmentStatus.Cancelled; x.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new MessageResponse("Appointment cancelled successfully"));
    }

    [HttpPatch("{id:int}/complete")]
    [Authorize(Roles = "Admin,Dentist")]
    public async Task<IActionResult> Complete(int id)
    {
        var x = await _db.Appointments.FindAsync(id);
        if (x == null) return NotFound(new ErrorResponse(404, "Appointment not found"));
        x.Status = AppointmentStatus.Completed; x.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new MessageResponse("Appointment completed successfully"));
    }

    private async Task<string?> ValidateAppointment(AppointmentRequest r)
    {
        if (!await _db.Patients.AnyAsync(x => x.Id == r.PatientId)) return "Patient not found";
        if (!await _db.Dentists.AnyAsync(x => x.Id == r.DentistId)) return "Dentist not found";
        var inSchedule = await _db.Schedules.AnyAsync(s => s.DentistId == r.DentistId && s.Date == r.AppointmentDate && s.IsAvailable && r.AppointmentTime >= s.StartTime && r.AppointmentTime < s.EndTime);
        if (!inSchedule) return "Appointment must be inside an available dentist schedule";
        return null;
    }
}
