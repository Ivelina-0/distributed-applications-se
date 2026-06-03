using DentalClinicApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicApp.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        var hasher = new PasswordHasher<User>();
        if (!await db.Users.AnyAsync())
        {
            var admin = new User { FullName = "Admin User", Email = "admin@clinic.com", PhoneNumber = "+359888000000", Role = UserRole.Admin };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
            db.Users.Add(admin);
            var dentistUser = new User { FullName = "Dr. Georgi Georgiev", Email = "dentist@clinic.com", PhoneNumber = "+359887654321", Role = UserRole.Dentist };
            dentistUser.PasswordHash = hasher.HashPassword(dentistUser, "Dentist123!");
            db.Users.Add(dentistUser);
            var patientUser = new User { FullName = "Maria Ivanova", Email = "patient@clinic.com", PhoneNumber = "+359888123456", Role = UserRole.Patient };
            patientUser.PasswordHash = hasher.HashPassword(patientUser, "Patient123!");
            db.Users.Add(patientUser);
        }
        if (!await db.Patients.AnyAsync())
        {
            db.Patients.AddRange(
                new Patient { FullName = "Maria Ivanova", Email = "maria@example.com", PhoneNumber = "+359888123456", BirthDate = new DateOnly(1995, 5, 12), Address = "Sofia" },
                new Patient { FullName = "Ivan Petrov", Email = "ivan@example.com", PhoneNumber = "+359887111222", BirthDate = new DateOnly(1988, 3, 2), Address = "Plovdiv" },
                new Patient { FullName = "Petya Dimitrova", Email = "petya@example.com", PhoneNumber = "+359889333444", BirthDate = new DateOnly(2000, 7, 21), Address = "Varna" });
        }
        if (!await db.Dentists.AnyAsync())
        {
            db.Dentists.AddRange(
                new Dentist { FullName = "Dr. Georgi Georgiev", Specialty = "Orthodontics", PhoneNumber = "+359887654321", Email = "georgiev@example.com", Bio = "Specialist in orthodontics", ConsultationPrice = 80 },
                new Dentist { FullName = "Dr. Elena Dimitrova", Specialty = "General dentistry", PhoneNumber = "+359889222333", Email = "elena@example.com", Bio = "Preventive and cosmetic dentistry", ConsultationPrice = 65 },
                new Dentist { FullName = "Dr. Nikolay Kolev", Specialty = "Oral surgery", PhoneNumber = "+359886555666", Email = "kolev@example.com", Bio = "Oral surgery and implantology", ConsultationPrice = 120 });
        }
        await db.SaveChangesAsync();
        if (!await db.Schedules.AnyAsync())
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var list = new List<Schedule>();
            for (int day = 1; day <= 14; day++)
            {
                list.Add(new Schedule { DentistId = 1, Date = today.AddDays(day), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0), IsAvailable = true, Notes = "Regular work day" });
                list.Add(new Schedule { DentistId = 2, Date = today.AddDays(day), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(18, 0), IsAvailable = true, Notes = "Regular work day" });
            }
            db.Schedules.AddRange(list);
            await db.SaveChangesAsync();
        }
        if (!await db.Appointments.AnyAsync())
        {
            var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            db.Appointments.Add(new Appointment { PatientId = 1, DentistId = 1, AppointmentDate = tomorrow, AppointmentTime = new TimeOnly(10, 0), Reason = "Preventive check-up", Status = AppointmentStatus.Scheduled });
            await db.SaveChangesAsync();
        }
    }
}
