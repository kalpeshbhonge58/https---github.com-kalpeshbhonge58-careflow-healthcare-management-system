using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Domain.Enums;
using CareFlow.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareFlow.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (context.Database.ProviderName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true)
            await context.Database.EnsureCreatedAsync(cancellationToken);
        else
            await context.Database.MigrateAsync(cancellationToken);

        if (await context.Roles.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded. Skipping seed operation.");
            return;
        }

        logger.LogInformation("Seeding CareFlow database...");

        var roles = SeedRoles();
        context.Roles.AddRange(roles);
        await context.SaveChangesAsync(cancellationToken);

        var roleMap = await context.Roles.ToDictionaryAsync(r => r.Name, r => r.Id, cancellationToken);

        var adminUser = CreateUser("admin@careflow.com", "Admin", "User", "+1-555-0100", passwordHasher.Hash("Admin@123"));
        var receptionistUsers = new[]
        {
            CreateUser("reception1@careflow.com", "Sarah", "Johnson", "+1-555-0101", passwordHasher.Hash("Reception@123")),
            CreateUser("reception2@careflow.com", "Michael", "Brown", "+1-555-0102", passwordHasher.Hash("Reception@123"))
        };
        var doctorUsers = new[]
        {
            CreateUser("dr.smith@careflow.com", "James", "Smith", "+1-555-0201", passwordHasher.Hash("Doctor@123")),
            CreateUser("dr.patel@careflow.com", "Priya", "Patel", "+1-555-0202", passwordHasher.Hash("Doctor@123")),
            CreateUser("dr.chen@careflow.com", "Wei", "Chen", "+1-555-0203", passwordHasher.Hash("Doctor@123"))
        };
        var patientUsers = new[]
        {
            CreateUser("john.doe@email.com", "John", "Doe", "+1-555-0301", passwordHasher.Hash("Patient@123")),
            CreateUser("jane.smith@email.com", "Jane", "Smith", "+1-555-0302", passwordHasher.Hash("Patient@123")),
            CreateUser("robert.wilson@email.com", "Robert", "Wilson", "+1-555-0303", passwordHasher.Hash("Patient@123")),
            CreateUser("emily.davis@email.com", "Emily", "Davis", "+1-555-0304", passwordHasher.Hash("Patient@123")),
            CreateUser("david.miller@email.com", "David", "Miller", "+1-555-0305", passwordHasher.Hash("Patient@123"))
        };

        context.Users.Add(adminUser);
        context.Users.AddRange(receptionistUsers);
        context.Users.AddRange(doctorUsers);
        context.Users.AddRange(patientUsers);
        await context.SaveChangesAsync(cancellationToken);

        context.UserRoles.AddRange(
            new UserRole { UserId = adminUser.Id, RoleId = roleMap[Roles.Admin] },
            new UserRole { UserId = receptionistUsers[0].Id, RoleId = roleMap[Roles.Receptionist] },
            new UserRole { UserId = receptionistUsers[1].Id, RoleId = roleMap[Roles.Receptionist] },
            new UserRole { UserId = doctorUsers[0].Id, RoleId = roleMap[Roles.Doctor] },
            new UserRole { UserId = doctorUsers[1].Id, RoleId = roleMap[Roles.Doctor] },
            new UserRole { UserId = doctorUsers[2].Id, RoleId = roleMap[Roles.Doctor] },
            new UserRole { UserId = patientUsers[0].Id, RoleId = roleMap[Roles.Patient] },
            new UserRole { UserId = patientUsers[1].Id, RoleId = roleMap[Roles.Patient] },
            new UserRole { UserId = patientUsers[2].Id, RoleId = roleMap[Roles.Patient] },
            new UserRole { UserId = patientUsers[3].Id, RoleId = roleMap[Roles.Patient] },
            new UserRole { UserId = patientUsers[4].Id, RoleId = roleMap[Roles.Patient] }
        );
        await context.SaveChangesAsync(cancellationToken);

        var departments = SeedDepartments();
        context.Departments.AddRange(departments);
        await context.SaveChangesAsync(cancellationToken);

        var doctors = new[]
        {
            new Doctor
            {
                UserId = doctorUsers[0].Id,
                DepartmentId = departments[0].Id,
                LicenseNumber = "MD-10001",
                Specialization = "Cardiology",
                Qualification = "MD, FACC",
                ExperienceYears = 15,
                ConsultationFee = 250.00m,
                AvailableFrom = new TimeSpan(9, 0, 0),
                AvailableTo = new TimeSpan(17, 0, 0)
            },
            new Doctor
            {
                UserId = doctorUsers[1].Id,
                DepartmentId = departments[1].Id,
                LicenseNumber = "MD-10002",
                Specialization = "Neurology",
                Qualification = "MD, PhD",
                ExperienceYears = 12,
                ConsultationFee = 300.00m,
                AvailableFrom = new TimeSpan(8, 0, 0),
                AvailableTo = new TimeSpan(16, 0, 0)
            },
            new Doctor
            {
                UserId = doctorUsers[2].Id,
                DepartmentId = departments[2].Id,
                LicenseNumber = "MD-10003",
                Specialization = "Orthopedics",
                Qualification = "MD, MS Ortho",
                ExperienceYears = 10,
                ConsultationFee = 275.00m,
                AvailableFrom = new TimeSpan(10, 0, 0),
                AvailableTo = new TimeSpan(18, 0, 0)
            }
        };
        context.Doctors.AddRange(doctors);
        await context.SaveChangesAsync(cancellationToken);

        var patients = new[]
        {
            new Patient { UserId = patientUsers[0].Id, PatientNumber = "PAT-00001", DateOfBirth = new DateTime(1985, 3, 15), Gender = Gender.Male, BloodGroup = "O+", Address = "123 Main St, Springfield", EmergencyContactName = "Mary Doe", EmergencyContactPhone = "+1-555-0306", Allergies = "Penicillin" },
            new Patient { UserId = patientUsers[1].Id, PatientNumber = "PAT-00002", DateOfBirth = new DateTime(1990, 7, 22), Gender = Gender.Female, BloodGroup = "A+", Address = "456 Oak Ave, Springfield", EmergencyContactName = "Tom Smith", EmergencyContactPhone = "+1-555-0307" },
            new Patient { UserId = patientUsers[2].Id, PatientNumber = "PAT-00003", DateOfBirth = new DateTime(1978, 11, 8), Gender = Gender.Male, BloodGroup = "B+", Address = "789 Pine Rd, Springfield", MedicalHistory = "Hypertension" },
            new Patient { UserId = patientUsers[3].Id, PatientNumber = "PAT-00004", DateOfBirth = new DateTime(1995, 1, 30), Gender = Gender.Female, BloodGroup = "AB+", Address = "321 Elm St, Springfield", Allergies = "Latex" },
            new Patient { UserId = patientUsers[4].Id, PatientNumber = "PAT-00005", DateOfBirth = new DateTime(1982, 9, 12), Gender = Gender.Male, BloodGroup = "O-", Address = "654 Maple Dr, Springfield", MedicalHistory = "Type 2 Diabetes" }
        };
        context.Patients.AddRange(patients);
        await context.SaveChangesAsync(cancellationToken);

        var medicines = SeedMedicines();
        context.Medicines.AddRange(medicines);
        await context.SaveChangesAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var appointments = new[]
        {
            new Appointment { PatientId = patients[0].Id, DoctorId = doctors[0].Id, AppointmentDate = today.AddDays(1), StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(9, 30, 0), Status = AppointmentStatus.Scheduled, Reason = "Chest pain evaluation" },
            new Appointment { PatientId = patients[1].Id, DoctorId = doctors[0].Id, AppointmentDate = today.AddDays(1), StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(10, 30, 0), Status = AppointmentStatus.Confirmed, Reason = "Follow-up visit" },
            new Appointment { PatientId = patients[2].Id, DoctorId = doctors[1].Id, AppointmentDate = today.AddDays(2), StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(11, 45, 0), Status = AppointmentStatus.Scheduled, Reason = "Migraine consultation" },
            new Appointment { PatientId = patients[3].Id, DoctorId = doctors[1].Id, AppointmentDate = today, StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(14, 30, 0), Status = AppointmentStatus.Confirmed, Reason = "Neurological exam" },
            new Appointment { PatientId = patients[4].Id, DoctorId = doctors[2].Id, AppointmentDate = today, StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(15, 30, 0), Status = AppointmentStatus.Scheduled, Reason = "Knee pain" },
            new Appointment { PatientId = patients[0].Id, DoctorId = doctors[2].Id, AppointmentDate = today.AddDays(-7), StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(10, 30, 0), Status = AppointmentStatus.Completed, Reason = "Joint assessment", CompletedAt = today.AddDays(-7).AddHours(10) },
            new Appointment { PatientId = patients[1].Id, DoctorId = doctors[0].Id, AppointmentDate = today.AddDays(-14), StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(11, 30, 0), Status = AppointmentStatus.Completed, Reason = "ECG review", CompletedAt = today.AddDays(-14).AddHours(11) },
            new Appointment { PatientId = patients[2].Id, DoctorId = doctors[1].Id, AppointmentDate = today.AddDays(3), StartTime = new TimeSpan(9, 30, 0), EndTime = new TimeSpan(10, 0, 0), Status = AppointmentStatus.Scheduled, Reason = "MRI results discussion" },
            new Appointment { PatientId = patients[3].Id, DoctorId = doctors[2].Id, AppointmentDate = today.AddDays(5), StartTime = new TimeSpan(16, 0, 0), EndTime = new TimeSpan(16, 30, 0), Status = AppointmentStatus.Scheduled, Reason = "Physical therapy referral" },
            new Appointment { PatientId = patients[4].Id, DoctorId = doctors[0].Id, AppointmentDate = today.AddDays(-3), StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(13, 30, 0), Status = AppointmentStatus.Cancelled, Reason = "Blood pressure check", CancellationReason = "Patient rescheduled", CancelledAt = today.AddDays(-3) }
        };
        context.Appointments.AddRange(appointments);
        await context.SaveChangesAsync(cancellationToken);

        var medicalRecords = new[]
        {
            new MedicalRecord { PatientId = patients[0].Id, DoctorId = doctors[2].Id, AppointmentId = appointments[5].Id, VisitDate = today.AddDays(-7), Diagnosis = "Mild osteoarthritis", Symptoms = "Knee stiffness", Treatment = "Physical therapy prescribed", FollowUpDate = today.AddDays(23) },
            new MedicalRecord { PatientId = patients[1].Id, DoctorId = doctors[0].Id, AppointmentId = appointments[6].Id, VisitDate = today.AddDays(-14), Diagnosis = "Normal sinus rhythm", Symptoms = "Palpitations", Treatment = "No medication required", Notes = "ECG normal" }
        };
        context.MedicalRecords.AddRange(medicalRecords);
        await context.SaveChangesAsync(cancellationToken);

        var prescriptions = new[]
        {
            new Prescription
            {
                PatientId = patients[0].Id,
                DoctorId = doctors[2].Id,
                MedicalRecordId = medicalRecords[0].Id,
                PrescriptionDate = today.AddDays(-7),
                Instructions = "Take medications with food",
                Items =
                [
                    new PrescriptionItem { MedicineId = medicines[0].Id, Dosage = "500mg", Frequency = "Twice daily", DurationDays = 14, Quantity = 28, Instructions = "After meals" },
                    new PrescriptionItem { MedicineId = medicines[4].Id, Dosage = "400mg", Frequency = "As needed", DurationDays = 7, Quantity = 14, Instructions = "For pain relief" }
                ]
            },
            new Prescription
            {
                PatientId = patients[4].Id,
                DoctorId = doctors[0].Id,
                PrescriptionDate = today.AddDays(-1),
                Instructions = "Monitor blood glucose daily",
                Items =
                [
                    new PrescriptionItem { MedicineId = medicines[2].Id, Dosage = "500mg", Frequency = "Once daily", DurationDays = 30, Quantity = 30 },
                    new PrescriptionItem { MedicineId = medicines[1].Id, Dosage = "10mg", Frequency = "Once daily", DurationDays = 30, Quantity = 30 }
                ]
            }
        };
        context.Prescriptions.AddRange(prescriptions);
        await context.SaveChangesAsync(cancellationToken);

        var invoices = new[]
        {
            new Invoice
            {
                InvoiceNumber = "INV-2026-00001",
                PatientId = patients[0].Id,
                AppointmentId = appointments[5].Id,
                InvoiceDate = today.AddDays(-7),
                SubTotal = 275.00m,
                TaxAmount = 27.50m,
                DiscountAmount = 0m,
                TotalAmount = 302.50m,
                PaymentStatus = PaymentStatus.Paid,
                Items = [new InvoiceItem { Description = "Orthopedic Consultation", Quantity = 1, UnitPrice = 275.00m, TotalPrice = 275.00m }],
                Payments = [new Payment { Amount = 302.50m, PaymentDate = today.AddDays(-7), PaymentMethod = "Credit Card", TransactionReference = "TXN-001" }]
            },
            new Invoice
            {
                InvoiceNumber = "INV-2026-00002",
                PatientId = patients[1].Id,
                AppointmentId = appointments[6].Id,
                InvoiceDate = today.AddDays(-14),
                SubTotal = 250.00m,
                TaxAmount = 25.00m,
                DiscountAmount = 25.00m,
                TotalAmount = 250.00m,
                PaymentStatus = PaymentStatus.Paid,
                Items = [new InvoiceItem { Description = "Cardiology Consultation", Quantity = 1, UnitPrice = 250.00m, TotalPrice = 250.00m }],
                Payments = [new Payment { Amount = 250.00m, PaymentDate = today.AddDays(-14), PaymentMethod = "Cash", TransactionReference = "TXN-002" }]
            },
            new Invoice
            {
                InvoiceNumber = "INV-2026-00003",
                PatientId = patients[4].Id,
                InvoiceDate = today,
                SubTotal = 500.00m,
                TaxAmount = 50.00m,
                DiscountAmount = 0m,
                TotalAmount = 550.00m,
                PaymentStatus = PaymentStatus.Pending,
                Items =
                [
                    new InvoiceItem { Description = "Cardiology Consultation", Quantity = 1, UnitPrice = 250.00m, TotalPrice = 250.00m },
                    new InvoiceItem { Description = "Blood Work Panel", Quantity = 1, UnitPrice = 250.00m, TotalPrice = 250.00m }
                ]
            },
            new Invoice
            {
                InvoiceNumber = "INV-2026-00004",
                PatientId = patients[2].Id,
                InvoiceDate = today.AddDays(-5),
                SubTotal = 300.00m,
                TaxAmount = 30.00m,
                DiscountAmount = 0m,
                TotalAmount = 330.00m,
                PaymentStatus = PaymentStatus.PartiallyPaid,
                Items = [new InvoiceItem { Description = "Neurology Consultation", Quantity = 1, UnitPrice = 300.00m, TotalPrice = 300.00m }],
                Payments = [new Payment { Amount = 150.00m, PaymentDate = today.AddDays(-5), PaymentMethod = "Debit Card", TransactionReference = "TXN-003" }]
            }
        };
        context.Invoices.AddRange(invoices);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("CareFlow database seeded successfully.");
    }

    private static List<Role> SeedRoles() =>
    [
        new Role { Name = Roles.Admin, Description = "System administrator with full access" },
        new Role { Name = Roles.Doctor, Description = "Medical practitioner" },
        new Role { Name = Roles.Receptionist, Description = "Front desk and scheduling staff" },
        new Role { Name = Roles.Patient, Description = "Registered patient" }
    ];

    private static List<Department> SeedDepartments() =>
    [
        new Department { Name = "Cardiology", Description = "Heart and cardiovascular care", Location = "Building A, Floor 2" },
        new Department { Name = "Neurology", Description = "Brain and nervous system care", Location = "Building A, Floor 3" },
        new Department { Name = "Orthopedics", Description = "Bone and joint care", Location = "Building B, Floor 1" },
        new Department { Name = "Pediatrics", Description = "Child healthcare", Location = "Building C, Floor 1" },
        new Department { Name = "Emergency", Description = "Emergency medical services", Location = "Building A, Ground Floor" }
    ];

    private static List<Medicine> SeedMedicines() =>
    [
        new Medicine { Name = "Ibuprofen", GenericName = "Ibuprofen", Manufacturer = "PharmaCo", DosageForm = "Tablet", UnitPrice = 0.50m, StockQuantity = 5000, Description = "NSAID for pain and inflammation" },
        new Medicine { Name = "Lisinopril", GenericName = "Lisinopril", Manufacturer = "MediGen", DosageForm = "Tablet", UnitPrice = 1.25m, StockQuantity = 3000, Description = "ACE inhibitor for blood pressure" },
        new Medicine { Name = "Metformin", GenericName = "Metformin", Manufacturer = "DiabeCare", DosageForm = "Tablet", UnitPrice = 0.75m, StockQuantity = 4000, Description = "Type 2 diabetes medication" },
        new Medicine { Name = "Amoxicillin", GenericName = "Amoxicillin", Manufacturer = "AntiBio Inc", DosageForm = "Capsule", UnitPrice = 2.00m, StockQuantity = 2500, Description = "Antibiotic" },
        new Medicine { Name = "Paracetamol", GenericName = "Acetaminophen", Manufacturer = "PharmaCo", DosageForm = "Tablet", UnitPrice = 0.30m, StockQuantity = 8000, Description = "Pain reliever and fever reducer" },
        new Medicine { Name = "Atorvastatin", GenericName = "Atorvastatin", Manufacturer = "CardioPharm", DosageForm = "Tablet", UnitPrice = 3.50m, StockQuantity = 2000, Description = "Cholesterol management" },
        new Medicine { Name = "Omeprazole", GenericName = "Omeprazole", Manufacturer = "GastroMed", DosageForm = "Capsule", UnitPrice = 1.80m, StockQuantity = 3500, Description = "Proton pump inhibitor" },
        new Medicine { Name = "Amlodipine", GenericName = "Amlodipine", Manufacturer = "MediGen", DosageForm = "Tablet", UnitPrice = 1.50m, StockQuantity = 2800, Description = "Calcium channel blocker" },
        new Medicine { Name = "Sertraline", GenericName = "Sertraline", Manufacturer = "MindCare", DosageForm = "Tablet", UnitPrice = 2.75m, StockQuantity = 1500, Description = "SSRI antidepressant" },
        new Medicine { Name = "Prednisone", GenericName = "Prednisone", Manufacturer = "ImmunoPharm", DosageForm = "Tablet", UnitPrice = 1.00m, StockQuantity = 2200, Description = "Corticosteroid anti-inflammatory" }
    ];

    private static User CreateUser(string email, string firstName, string lastName, string phone, string passwordHash) =>
        new()
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phone,
            PasswordHash = passwordHash
        };
}
