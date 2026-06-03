using DentalClinicApp.Data; using DentalClinicApp.DTOs; using DentalClinicApp.Models;
using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;

namespace DentalClinicApp.Controllers;
[ApiController, Route("api/patients"), Authorize]
public class PatientsController : ControllerBase
{
    private readonly AppDbContext _db; public PatientsController(AppDbContext db) => _db = db;
    [HttpGet, Authorize(Roles="Admin")]
    public async Task<ActionResult<PagedResponse<PatientResponse>>> Get(string? search, int page=1, int pageSize=10, string? sortBy="fullName", string sortOrder="asc")
    {
        var q = _db.Patients.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.FullName.Contains(search) || x.Email.Contains(search) || x.PhoneNumber.Contains(search));
        q = (sortBy, sortOrder.ToLower()) switch { ("email","desc") => q.OrderByDescending(x=>x.Email), ("email",_) => q.OrderBy(x=>x.Email), ("phoneNumber","desc") => q.OrderByDescending(x=>x.PhoneNumber), ("phoneNumber",_) => q.OrderBy(x=>x.PhoneNumber), (_,"desc") => q.OrderByDescending(x=>x.FullName), _ => q.OrderBy(x=>x.FullName) };
        var total=await q.CountAsync(); var items=await q.Skip((page-1)*pageSize).Take(pageSize).Select(x=>new PatientResponse(x.Id,x.FullName,x.Email,x.PhoneNumber,x.BirthDate,x.Address,x.IsActive)).ToListAsync();
        return Ok(new PagedResponse<PatientResponse>(items,total,page,pageSize));
    }
    [HttpGet("{id:int}")] public async Task<ActionResult<PatientResponse>> GetById(int id){ var x=await _db.Patients.FindAsync(id); return x==null?NotFound(new ErrorResponse(404,"Patient not found")):Ok(new PatientResponse(x.Id,x.FullName,x.Email,x.PhoneNumber,x.BirthDate,x.Address,x.IsActive)); }
    [HttpPost, Authorize(Roles="Admin")] public async Task<ActionResult<PatientResponse>> Create(PatientRequest r){ if(await _db.Patients.AnyAsync(x=>x.Email==r.Email)) return Conflict(new ErrorResponse(409,"Patient email already exists")); var x=new Patient{FullName=r.FullName,Email=r.Email,PhoneNumber=r.PhoneNumber,BirthDate=r.BirthDate,Address=r.Address}; _db.Patients.Add(x); await _db.SaveChangesAsync(); return CreatedAtAction(nameof(GetById),new{id=x.Id},new PatientResponse(x.Id,x.FullName,x.Email,x.PhoneNumber,x.BirthDate,x.Address,x.IsActive));}
    [HttpPut("{id:int}")] public async Task<ActionResult<PatientResponse>> Update(int id, PatientRequest r){ var x=await _db.Patients.FindAsync(id)??throw new KeyNotFoundException("Patient not found"); x.FullName=r.FullName;x.Email=r.Email;x.PhoneNumber=r.PhoneNumber;x.BirthDate=r.BirthDate;x.Address=r.Address; await _db.SaveChangesAsync(); return Ok(new PatientResponse(x.Id,x.FullName,x.Email,x.PhoneNumber,x.BirthDate,x.Address,x.IsActive));}
    [HttpDelete("{id:int}"), Authorize(Roles="Admin")] public async Task<IActionResult> Delete(int id){ var x=await _db.Patients.FindAsync(id)??throw new KeyNotFoundException("Patient not found"); _db.Patients.Remove(x); await _db.SaveChangesAsync(); return NoContent();}
    [HttpGet("{id:int}/history")] public async Task<ActionResult<IEnumerable<AppointmentHistoryResponse>>> History(int id, DateOnly? fromDate, DateOnly? toDate){ if(!await _db.Patients.AnyAsync(x=>x.Id==id)) return NotFound(new ErrorResponse(404,"Patient not found")); var q=_db.Appointments.Include(x=>x.Dentist).Where(x=>x.PatientId==id); if(fromDate.HasValue) q=q.Where(x=>x.AppointmentDate>=fromDate); if(toDate.HasValue) q=q.Where(x=>x.AppointmentDate<=toDate); return Ok(await q.OrderByDescending(x=>x.AppointmentDate).Select(x=>new AppointmentHistoryResponse(x.AppointmentDate,x.AppointmentTime,x.Dentist!.FullName,x.Status,x.Reason)).ToListAsync());}
}
