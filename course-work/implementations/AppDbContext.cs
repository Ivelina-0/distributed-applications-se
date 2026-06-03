using DentalClinicApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Dentist> Dentists => Set<Dentist>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();
        b.Entity<Patient>().HasIndex(x => x.Email).IsUnique();
        b.Entity<Dentist>().HasIndex(x => x.Email).IsUnique();
        b.Entity<Dentist>().Property(x => x.ConsultationPrice).HasPrecision(10, 2);
        b.Entity<Appointment>().HasIndex(x => new { x.DentistId, x.AppointmentDate, x.AppointmentTime }).IsUnique();
        b.Entity<Appointment>().HasOne(x => x.Patient).WithMany(x => x.Appointments).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Appointment>().HasOne(x => x.Dentist).WithMany(x => x.Appointments).HasForeignKey(x => x.DentistId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Schedule>().HasOne(x => x.Dentist).WithMany(x => x.Schedules).HasForeignKey(x => x.DentistId).OnDelete(DeleteBehavior.Cascade);
    }
}
