using CareFlow.Application.DTOs.Appointment;
using FluentValidation;

namespace CareFlow.Application.Validators;

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("Patient is required.");

        RuleFor(x => x.DoctorId)
            .GreaterThan(0).WithMessage("Doctor is required.");

        RuleFor(x => x.AppointmentDate)
            .Must(date => date.Date >= DateTime.UtcNow.Date)
            .WithMessage("Appointment date must be today or in the future.");

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime).WithMessage("Start time must be before end time.");

        RuleFor(x => x)
            .Must(x => x.AppointmentDate.Date > DateTime.UtcNow.Date ||
                       x.StartTime > DateTime.UtcNow.TimeOfDay)
            .WithMessage("Appointment date must be in the future.");
    }
}
