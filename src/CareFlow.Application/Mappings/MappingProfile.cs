using AutoMapper;
using CareFlow.Application.DTOs.Appointment;
using CareFlow.Application.DTOs.Auth;
using CareFlow.Application.DTOs.Billing;
using CareFlow.Application.DTOs.Common;
using CareFlow.Application.DTOs.Doctor;
using CareFlow.Application.DTOs.MedicalRecord;
using CareFlow.Application.DTOs.Patient;
using CareFlow.Application.DTOs.Prescription;
using CareFlow.Domain.Entities;
using CareFlow.Domain.Enums;

namespace CareFlow.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserResponse>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Roles, o => o.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name)));

        CreateMap<Patient, PatientResponse>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.User.FirstName))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.User.LastName))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.User.PhoneNumber))
            .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender.ToString()));

        CreateMap<Patient, PatientDetailResponse>()
            .IncludeBase<Patient, PatientResponse>();

        CreateMap<Doctor, DoctorResponse>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.User.FirstName))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.User.LastName))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.User.PhoneNumber))
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department.Name));

        CreateMap<Appointment, AppointmentResponse>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s => s.Patient.User.FullName))
            .ForMember(d => d.PatientNumber, o => o.MapFrom(s => s.Patient.PatientNumber))
            .ForMember(d => d.DoctorName, o => o.MapFrom(s => s.Doctor.User.FullName))
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Doctor.Department.Name))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<MedicalRecord, MedicalRecordResponse>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s => s.Patient.User.FullName))
            .ForMember(d => d.DoctorName, o => o.MapFrom(s => s.Doctor.User.FullName));

        CreateMap<Prescription, PrescriptionResponse>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s => s.Patient.User.FullName))
            .ForMember(d => d.DoctorName, o => o.MapFrom(s => s.Doctor.User.FullName));

        CreateMap<PrescriptionItem, PrescriptionItemResponse>()
            .ForMember(d => d.MedicineName, o => o.MapFrom(s => s.Medicine.Name));

        CreateMap<Invoice, InvoiceResponse>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s => s.Patient.User.FullName))
            .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.PaymentStatus.ToString()));

        CreateMap<InvoiceItem, InvoiceItemResponse>();
        CreateMap<Payment, PaymentResponse>();

        CreateMap<Department, DepartmentResponse>();
        CreateMap<Medicine, MedicineResponse>();

        CreateMap<CreatePatientRequest, User>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.UserRoles, o => o.Ignore())
            .ForMember(d => d.Patient, o => o.Ignore())
            .ForMember(d => d.Doctor, o => o.Ignore())
            .ForMember(d => d.AuditLogs, o => o.Ignore())
            .ForMember(d => d.LastLoginAt, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore());

        CreateMap<CreatePatientRequest, Patient>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.PatientNumber, o => o.Ignore())
            .ForMember(d => d.Appointments, o => o.Ignore())
            .ForMember(d => d.MedicalRecords, o => o.Ignore())
            .ForMember(d => d.Prescriptions, o => o.Ignore())
            .ForMember(d => d.Invoices, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore());

        CreateMap<CreateDoctorRequest, User>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.UserRoles, o => o.Ignore())
            .ForMember(d => d.Patient, o => o.Ignore())
            .ForMember(d => d.Doctor, o => o.Ignore())
            .ForMember(d => d.AuditLogs, o => o.Ignore())
            .ForMember(d => d.LastLoginAt, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore());

        CreateMap<CreateDoctorRequest, Doctor>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.Department, o => o.Ignore())
            .ForMember(d => d.Appointments, o => o.Ignore())
            .ForMember(d => d.MedicalRecords, o => o.Ignore())
            .ForMember(d => d.Prescriptions, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore());
    }
}
