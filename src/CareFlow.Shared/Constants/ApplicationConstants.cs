namespace CareFlow.Shared.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Doctor = "Doctor";
    public const string Receptionist = "Receptionist";
    public const string Patient = "Patient";
}

public static class CacheKeys
{
    public const string Departments = "departments_all";
    public const string DoctorsLookup = "doctors_lookup";
    public const string MedicinesLookup = "medicines_lookup";
    public const string AdminDashboard = "dashboard_admin";
    public const string DoctorDashboardPrefix = "dashboard_doctor_";
    public const string ReceptionistDashboard = "dashboard_receptionist";
}

public static class AuditActions
{
    public const string PatientCreated = "PatientCreated";
    public const string PatientUpdated = "PatientUpdated";
    public const string PatientDeactivated = "PatientDeactivated";
    public const string DoctorCreated = "DoctorCreated";
    public const string DoctorUpdated = "DoctorUpdated";
    public const string AppointmentBooked = "AppointmentBooked";
    public const string AppointmentCancelled = "AppointmentCancelled";
    public const string AppointmentCompleted = "AppointmentCompleted";
    public const string InvoiceCreated = "InvoiceCreated";
    public const string PaymentRecorded = "PaymentRecorded";
    public const string UserLogin = "UserLogin";
}
