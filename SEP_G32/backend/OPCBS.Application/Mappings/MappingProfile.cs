using AutoMapper;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.DTOs.Auth;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;

namespace OPCBS.Application.Mappings;

/// <summary>
/// AutoMapper profile for all entity ↔ DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User → UserProfileDto
        CreateMap<User, UserProfileDto>()
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.Name));

        // DoctorProfile → DoctorProfileDto
        CreateMap<DoctorProfile, DoctorProfileDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.User.FullName))
            .ForMember(d => d.AvatarUrl, opt => opt.MapFrom(s => s.User.AvatarUrl))
            .ForMember(d => d.Specializations, opt => opt.MapFrom(s =>
                s.DoctorSpecializations != null
                    ? s.DoctorSpecializations.Select(ds => ds.Specialization.Name).ToList()
                    : new List<string>()));

        // Appointment → AppointmentDto
        CreateMap<Appointment, AppointmentDto>()
            .ForMember(d => d.DoctorName, opt => opt.MapFrom(s => s.Doctor.User.FullName))
            .ForMember(d => d.PatientName, opt => opt.MapFrom(s =>
                s.Patient != null ? s.Patient.User.FullName : s.GuestName))
            .ForMember(d => d.AppointmentDate, opt => opt.MapFrom(s => s.AppointmentSlot.SlotDate.ToString("yyyy-MM-dd")))
            .ForMember(d => d.StartTime, opt => opt.MapFrom(s => s.AppointmentSlot.StartTime.ToString("HH:mm")))
            .ForMember(d => d.EndTime, opt => opt.MapFrom(s => s.AppointmentSlot.EndTime.ToString("HH:mm")));

        // Appointment → AppointmentListItemDto
        CreateMap<Appointment, AppointmentListItemDto>()
            .ForMember(d => d.DoctorName, opt => opt.MapFrom(s => s.Doctor.User.FullName))
            .ForMember(d => d.AppointmentDate, opt => opt.MapFrom(s => s.AppointmentSlot.SlotDate.ToString("yyyy-MM-dd")))
            .ForMember(d => d.StartTime, opt => opt.MapFrom(s => s.AppointmentSlot.StartTime.ToString("HH:mm")));

        // AppointmentSlot → AppointmentSlotDto
        CreateMap<AppointmentSlot, AppointmentSlotDto>()
            .ForMember(d => d.Date, opt => opt.MapFrom(s => s.SlotDate.ToString("yyyy-MM-dd")))
            .ForMember(d => d.StartTime, opt => opt.MapFrom(s => s.StartTime.ToString("HH:mm")))
            .ForMember(d => d.EndTime, opt => opt.MapFrom(s => s.EndTime.ToString("HH:mm")));

        // Schedule → ScheduleDto
        CreateMap<Schedule, ScheduleDto>()
            .ForMember(d => d.StartTime, opt => opt.MapFrom(s => s.StartTime.ToString("HH:mm")))
            .ForMember(d => d.EndTime, opt => opt.MapFrom(s => s.EndTime.ToString("HH:mm")));

        // ConsultationRecord → ConsultationRecordDto
        CreateMap<ConsultationRecord, ConsultationRecordDto>()
            .ForMember(d => d.DoctorName, opt => opt.MapFrom(s => s.Doctor.User.FullName))
            .ForMember(d => d.PatientName, opt => opt.MapFrom(s => s.Patient.User.FullName));

        // BlogPost → BlogPostDto
        CreateMap<BlogPost, BlogPostDto>()
            .ForMember(d => d.AuthorName, opt => opt.MapFrom(s => s.Doctor.User.FullName))
            .ForMember(d => d.AuthorAvatarUrl, opt => opt.MapFrom(s => s.Doctor.User.AvatarUrl))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        // Review → ReviewDto
        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.PatientName, opt => opt.MapFrom(s => s.Patient.User.FullName));

        // VerificationRequest → VerificationRequestDto
        CreateMap<VerificationRequest, VerificationRequestDto>()
            .ForMember(d => d.DoctorName, opt => opt.MapFrom(s => s.DoctorProfile.User.FullName))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        // Notification → NotificationDto
        CreateMap<Notification, NotificationDto>()
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()));

        // ServicePackage → ServicePackageDto
        CreateMap<ServicePackage, ServicePackageDto>();

        // DoctorSubscription → SubscriptionDto
        CreateMap<DoctorSubscription, SubscriptionDto>()
            .ForMember(d => d.PackageName, opt => opt.MapFrom(s => s.ServicePackage.Name))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        // TreatmentPackage → TreatmentPackageDto
        CreateMap<TreatmentPackage, TreatmentPackageDto>()
            .ForMember(d => d.DoctorName, opt => opt.MapFrom(s => s.Doctor.User.FullName))
            .ForMember(d => d.PatientName, opt => opt.MapFrom(s => s.Patient.User.FullName))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        // Specialization → SpecializationDto
        CreateMap<Specialization, SpecializationDto>();

        // AuditLog → AuditLogDto
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(d => d.UserEmail, opt => opt.MapFrom(s => s.User != null ? s.User.Email : null))
            .ForMember(d => d.Action, opt => opt.MapFrom(s => s.Action.ToString()));

        // User → UserListDto
        CreateMap<User, UserListDto>()
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.Name))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
    }
}
