namespace OPCBS.Web.DTOs;

public record CreateAppointmentDto(Guid DoctorId, DateTimeOffset StartAt, string Notes);
