using CareFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new Configurations.UserConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.RoleConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PatientConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.DoctorConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.DepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.MedicalRecordConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.MedicineConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PrescriptionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PrescriptionItemConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.InvoiceItemConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.AuditLogConfiguration());
    }
}
