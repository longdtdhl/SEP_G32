using AutoMapper;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.DTOs.Auth;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public DoctorService(IRepository<DoctorProfile> doctorRepo, IRepository<User> userRepo, IMapper mapper, IUnitOfWork uow)
    {
        _doctorRepo = doctorRepo;
        _userRepo = userRepo;
        _mapper = mapper;
        _uow = uow;
    }

    public async Task<ApiResponse<List<DoctorProfileDto>>> GetDoctorsAsync(string? search, Guid? specializationId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var query = allDoctors.Where(d => d.VerificationStatus == VerificationStatus.Approved && d.IsVisible);

        // Note: For production, filtering should use IQueryable. This works for MVP.
        var list = query.ToList();
        var total = list.Count;
        var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = _mapper.Map<List<DoctorProfileDto>>(items);

        return ApiResponse<List<DoctorProfileDto>>.SuccessResponse(dtos, pagination: new PaginationMetadata
        {
            Page = page, PageSize = pageSize, TotalItems = total
        });
    }

    public async Task<ApiResponse<DoctorProfileDto>> GetDoctorByIdAsync(Guid doctorProfileId, CancellationToken ct = default)
    {
        var doctor = await _doctorRepo.GetByIdAsync(doctorProfileId, ct);
        if (doctor == null)
            return ApiResponse<DoctorProfileDto>.ErrorResponse("Doctor not found");

        var dto = _mapper.Map<DoctorProfileDto>(doctor);
        return ApiResponse<DoctorProfileDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<DoctorProfileDto>> GetDoctorProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == userId);
        if (doctor == null)
            return ApiResponse<DoctorProfileDto>.ErrorResponse("Doctor profile not found");

        var dto = _mapper.Map<DoctorProfileDto>(doctor);
        return ApiResponse<DoctorProfileDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<DoctorProfileDto>> UpdateDoctorProfileAsync(Guid userId, UpdateDoctorProfileDto dto, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == userId);
        if (doctor == null)
            return ApiResponse<DoctorProfileDto>.ErrorResponse("Doctor profile not found");

        if (!string.IsNullOrWhiteSpace(dto.ProfessionalTitle))
            doctor.ProfessionalTitle = dto.ProfessionalTitle;
        if (!string.IsNullOrWhiteSpace(dto.Biography))
            doctor.Biography = dto.Biography;
        if (dto.ExperienceYears.HasValue)
            doctor.ExperienceYears = dto.ExperienceYears.Value;
        if (dto.IsVisible.HasValue)
            doctor.IsVisible = dto.IsVisible.Value;

        doctor.UpdatedAt = DateTime.UtcNow;
        _doctorRepo.Update(doctor);
        await _uow.SaveChangesAsync(ct);

        var result = _mapper.Map<DoctorProfileDto>(doctor);
        return ApiResponse<DoctorProfileDto>.SuccessResponse(result, "Profile updated successfully");
    }
}

public class AppointmentService : IAppointmentService
{
    private readonly IRepository<Appointment> _apptRepo;
    private readonly IRepository<AppointmentSlot> _slotRepo;
    private readonly IRepository<AppointmentHistory> _historyRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<DoctorSubscription> _subscriptionRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AppointmentService(
        IRepository<Appointment> apptRepo,
        IRepository<AppointmentSlot> slotRepo,
        IRepository<AppointmentHistory> historyRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<DoctorSubscription> subscriptionRepo,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _apptRepo = apptRepo;
        _slotRepo = slotRepo;
        _historyRepo = historyRepo;
        _doctorRepo = doctorRepo;
        _patientRepo = patientRepo;
        _subscriptionRepo = subscriptionRepo;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AppointmentDto>> CreateAppointmentAsync(CreateAppointmentDto dto, Guid? patientUserId, CancellationToken ct = default)
    {
        // Validate doctor exists and is verified (BOOK-04)
        var doctor = await _doctorRepo.GetByIdAsync(dto.DoctorId, ct);
        if (doctor == null || doctor.VerificationStatus != VerificationStatus.Approved)
            return ApiResponse<AppointmentDto>.ErrorResponse("Doctor not found or not verified");

        // BOOK-04 / DOC-12 / SP-01: Doctor must have active subscription
        var allSubs = await _subscriptionRepo.GetAllAsync(ct);
        var hasActiveSub = allSubs.Any(s =>
            s.DoctorProfileId == dto.DoctorId &&
            s.Status == SubscriptionStatus.Active &&
            s.ExpirationDate > DateTime.UtcNow);
        if (!hasActiveSub)
            return ApiResponse<AppointmentDto>.ErrorResponse("Doctor does not have an active service subscription");

        // Validate slot exists and is available
        var slot = await _slotRepo.GetByIdAsync(dto.AppointmentSlotId, ct);
        if (slot == null || slot.Status != AppointmentSlotStatus.Available)
            return ApiResponse<AppointmentDto>.ErrorResponse("Slot not available");

        // BOOK-03: No past booking
        var slotDateTime = slot.SlotDate.ToDateTime(slot.StartTime);
        if (slotDateTime < DateTime.UtcNow)
            return ApiResponse<AppointmentDto>.ErrorResponse("Cannot book an appointment in the past");

        // Resolve patient profile
        Guid? patientProfileId = null;
        if (patientUserId.HasValue)
        {
            var allPatients = await _patientRepo.GetAllAsync(ct);
            var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId.Value);
            patientProfileId = patient?.Id;
        }

        // BOOK-06 / BOOK-07 / BOOK-09: No double booking for the same patient on the same slot
        if (patientProfileId.HasValue)
        {
            var allAppts = await _apptRepo.GetAllAsync(ct);
            var hasOverlap = allAppts.Any(a =>
                a.PatientId == patientProfileId.Value &&
                a.AppointmentSlotId == dto.AppointmentSlotId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.Rejected);
            if (hasOverlap)
                return ApiResponse<AppointmentDto>.ErrorResponse("Patient already has an appointment for this slot");
        }

        await _uow.BeginTransactionAsync(ct);
        try
        {
            slot.Status = AppointmentSlotStatus.Booked;
            _slotRepo.Update(slot);

            var bookingCode = $"OPCBS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            var appointment = new Appointment
            {
                BookingCode = bookingCode,
                AppointmentSlotId = slot.Id,
                DoctorId = dto.DoctorId,
                PatientId = patientProfileId,
                GuestName = dto.GuestName,
                GuestEmail = dto.GuestEmail,
                GuestPhoneNumber = dto.GuestPhoneNumber,
                Notes = dto.Notes,
                TreatmentPackageId = dto.TreatmentPackageId,
                Status = AppointmentStatus.Pending,
                AppointmentDate = slotDateTime,
                AppointmentSlot = slot,
                Doctor = doctor
            };
            await _apptRepo.AddAsync(appointment, ct);

            await _historyRepo.AddAsync(new AppointmentHistory
            {
                AppointmentId = appointment.Id,
                NewStatus = AppointmentStatus.Pending,
                Reason = "Appointment created",
                Appointment = appointment
            }, ct);

            await _uow.CommitTransactionAsync(ct);

            var result = _mapper.Map<AppointmentDto>(appointment);
            return ApiResponse<AppointmentDto>.SuccessResponse(result, "Appointment booked successfully");
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<ApiResponse<List<AppointmentListItemDto>>> GetMyAppointmentsAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == userId);
        if (patient == null)
            return ApiResponse<List<AppointmentListItemDto>>.ErrorResponse("Patient profile not found");

        var allAppts = await _apptRepo.GetAllAsync(ct);
        var myAppts = allAppts.Where(a => a.PatientId == patient.Id && !a.IsDeleted).ToList();
        var total = myAppts.Count;
        var items = myAppts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = _mapper.Map<List<AppointmentListItemDto>>(items);

        return ApiResponse<List<AppointmentListItemDto>>.SuccessResponse(dtos, pagination: new PaginationMetadata
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        });
    }

    public async Task<ApiResponse<AppointmentDto>> GetAppointmentByIdAsync(Guid appointmentId, Guid userId, CancellationToken ct = default)
    {
        var appointment = await _apptRepo.GetByIdAsync(appointmentId, ct);
        if (appointment == null)
            return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");

        var dto = _mapper.Map<AppointmentDto>(appointment);
        return ApiResponse<AppointmentDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<AppointmentDto>> TrackAppointmentAsync(TrackAppointmentDto dto, CancellationToken ct = default)
    {
        var allAppts = await _apptRepo.GetAllAsync(ct);
        var appointment = allAppts.FirstOrDefault(a =>
            a.BookingCode == dto.BookingCode &&
            (a.GuestEmail == dto.Email || (a.Patient != null && a.Patient.User.Email == dto.Email)));

        if (appointment == null)
            return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");

        var result = _mapper.Map<AppointmentDto>(appointment);
        return ApiResponse<AppointmentDto>.SuccessResponse(result);
    }

    public async Task<ApiResponse> CancelAppointmentAsync(Guid appointmentId, Guid userId, CancelAppointmentDto dto, CancellationToken ct = default)
    {
        var appointment = await _apptRepo.GetByIdAsync(appointmentId, ct);
        if (appointment == null)
            return ApiResponse.ErrorResponse("Appointment not found");

        if (appointment.Status == AppointmentStatus.Completed)
            return ApiResponse.ErrorResponse("Completed appointments cannot be modified");

        if (appointment.Status == AppointmentStatus.Cancelled)
            return ApiResponse.ErrorResponse("Appointment already cancelled");

        // APPT-05: 24-hour cancellation policy
        var slot = await _slotRepo.GetByIdAsync(appointment.AppointmentSlotId, ct);
        if (slot != null)
        {
            var slotDateTime = slot.SlotDate.ToDateTime(slot.StartTime);
            if (slotDateTime - DateTime.UtcNow < TimeSpan.FromHours(24))
                return ApiResponse.ErrorResponse("Cannot cancel an appointment less than 24 hours before the scheduled time");
        }

        var prevStatus = appointment.Status;
        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.CancellationReason = dto.Reason;
        appointment.UpdatedAt = DateTime.UtcNow;

        // Release the slot
        if (slot != null)
        {
            slot.Status = AppointmentSlotStatus.Available;
            _slotRepo.Update(slot);
        }

        _apptRepo.Update(appointment);
        await _historyRepo.AddAsync(new AppointmentHistory
        {
            AppointmentId = appointmentId,
            PreviousStatus = prevStatus,
            NewStatus = AppointmentStatus.Cancelled,
            Reason = dto.Reason ?? "Cancelled by user",
            Appointment = appointment
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Appointment cancelled successfully");
    }

    public async Task<ApiResponse<List<AppointmentListItemDto>>> GetDoctorAppointmentsAsync(Guid doctorUserId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null)
            return ApiResponse<List<AppointmentListItemDto>>.ErrorResponse("Doctor profile not found");

        var allAppts = await _apptRepo.GetAllAsync(ct);
        var doctorAppts = allAppts.Where(a => a.DoctorId == doctor.Id && !a.IsDeleted).ToList();
        var total = doctorAppts.Count;
        var items = doctorAppts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = _mapper.Map<List<AppointmentListItemDto>>(items);

        return ApiResponse<List<AppointmentListItemDto>>.SuccessResponse(dtos, pagination: new PaginationMetadata
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        });
    }

    public async Task<ApiResponse> ApproveAppointmentAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct = default)
    {
        var appointment = await _apptRepo.GetByIdAsync(appointmentId, ct);
        if (appointment == null)
            return ApiResponse.ErrorResponse("Appointment not found");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || appointment.DoctorId != doctor.Id)
            return ApiResponse.ErrorResponse("Not authorized to approve this appointment");

        if (appointment.Status != AppointmentStatus.Pending)
            return ApiResponse.ErrorResponse("Only pending appointments can be approved");

        appointment.Status = AppointmentStatus.Approved;
        appointment.ApprovedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;
        _apptRepo.Update(appointment);

        await _historyRepo.AddAsync(new AppointmentHistory
        {
            AppointmentId = appointmentId,
            PreviousStatus = AppointmentStatus.Pending,
            NewStatus = AppointmentStatus.Approved,
            Reason = "Approved by doctor",
            Appointment = appointment
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Appointment approved");
    }

    public async Task<ApiResponse> RejectAppointmentAsync(Guid appointmentId, Guid doctorUserId, RejectAppointmentDto dto, CancellationToken ct = default)
    {
        var appointment = await _apptRepo.GetByIdAsync(appointmentId, ct);
        if (appointment == null)
            return ApiResponse.ErrorResponse("Appointment not found");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || appointment.DoctorId != doctor.Id)
            return ApiResponse.ErrorResponse("Not authorized to reject this appointment");

        if (appointment.Status != AppointmentStatus.Pending)
            return ApiResponse.ErrorResponse("Only pending appointments can be rejected");

        appointment.Status = AppointmentStatus.Rejected;
        appointment.RejectionReason = dto.Reason;
        appointment.UpdatedAt = DateTime.UtcNow;
        _apptRepo.Update(appointment);

        // Release the slot
        var slot = await _slotRepo.GetByIdAsync(appointment.AppointmentSlotId, ct);
        if (slot != null)
        {
            slot.Status = AppointmentSlotStatus.Available;
            _slotRepo.Update(slot);
        }

        await _historyRepo.AddAsync(new AppointmentHistory
        {
            AppointmentId = appointmentId,
            PreviousStatus = AppointmentStatus.Pending,
            NewStatus = AppointmentStatus.Rejected,
            Reason = dto.Reason ?? "Rejected by doctor",
            Appointment = appointment
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Appointment rejected");
    }

    public async Task<ApiResponse> CompleteAppointmentAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct = default)
    {
        var appointment = await _apptRepo.GetByIdAsync(appointmentId, ct);
        if (appointment == null)
            return ApiResponse.ErrorResponse("Appointment not found");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || appointment.DoctorId != doctor.Id)
            return ApiResponse.ErrorResponse("Not authorized");

        if (appointment.Status != AppointmentStatus.Approved && appointment.Status != AppointmentStatus.InProgress)
            return ApiResponse.ErrorResponse("Only approved/in-progress appointments can be completed");

        var prevStatus = appointment.Status;
        appointment.Status = AppointmentStatus.Completed;
        appointment.CompletedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;
        _apptRepo.Update(appointment);

        var slot = await _slotRepo.GetByIdAsync(appointment.AppointmentSlotId, ct);
        if (slot != null)
        {
            slot.Status = AppointmentSlotStatus.Completed;
            _slotRepo.Update(slot);
        }

        await _historyRepo.AddAsync(new AppointmentHistory
        {
            AppointmentId = appointmentId,
            PreviousStatus = prevStatus,
            NewStatus = AppointmentStatus.Completed,
            Reason = "Completed by doctor",
            Appointment = appointment
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Appointment completed");
    }

    public async Task<ApiResponse> RescheduleAppointmentAsync(Guid appointmentId, Guid userId, RescheduleAppointmentDto dto, CancellationToken ct = default)
    {
        var appointment = await _apptRepo.GetByIdAsync(appointmentId, ct);
        if (appointment == null)
            return ApiResponse.ErrorResponse("Appointment not found");

        // Only pending or approved appointments can be rescheduled (APPT-06, APPT-08)
        if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Approved)
            return ApiResponse.ErrorResponse("Only pending or approved appointments can be rescheduled");

        // Verify the new slot is available
        var newSlot = await _slotRepo.GetByIdAsync(dto.NewSlotId, ct);
        if (newSlot == null || newSlot.Status != AppointmentSlotStatus.Available)
            return ApiResponse.ErrorResponse("New slot is not available");

        // New slot must belong to the same doctor
        if (newSlot.DoctorProfileId != appointment.DoctorId)
            return ApiResponse.ErrorResponse("New slot must belong to the same doctor");

        // BOOK-03: Cannot reschedule to a past slot
        var newSlotDateTime = newSlot.SlotDate.ToDateTime(newSlot.StartTime);
        if (newSlotDateTime < DateTime.UtcNow)
            return ApiResponse.ErrorResponse("Cannot reschedule to a past time slot");

        var prevStatus = appointment.Status;

        // Release old slot
        var oldSlot = await _slotRepo.GetByIdAsync(appointment.AppointmentSlotId, ct);
        if (oldSlot != null)
        {
            oldSlot.Status = AppointmentSlotStatus.Available;
            _slotRepo.Update(oldSlot);
        }

        // Book new slot
        newSlot.Status = AppointmentSlotStatus.Booked;
        _slotRepo.Update(newSlot);

        appointment.AppointmentSlotId = dto.NewSlotId;
        appointment.AppointmentDate = newSlotDateTime;
        appointment.UpdatedAt = DateTime.UtcNow;
        _apptRepo.Update(appointment);

        await _historyRepo.AddAsync(new AppointmentHistory
        {
            AppointmentId = appointmentId,
            PreviousStatus = prevStatus,
            NewStatus = prevStatus, // Status unchanged, only slot changed
            Reason = dto.Reason ?? "Rescheduled by patient",
            Appointment = appointment
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Appointment rescheduled successfully");
    }
}


public class ScheduleService : IScheduleService
{
    private readonly IRepository<Schedule> _scheduleRepo;
    private readonly IRepository<AppointmentSlot> _slotRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<DoctorDayOff> _dayOffRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ScheduleService(
        IRepository<Schedule> scheduleRepo,
        IRepository<AppointmentSlot> slotRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<DoctorDayOff> dayOffRepo,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _scheduleRepo = scheduleRepo;
        _slotRepo = slotRepo;
        _doctorRepo = doctorRepo;
        _dayOffRepo = dayOffRepo;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ScheduleDto>> CreateScheduleAsync(Guid doctorUserId, CreateScheduleDto dto, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null)
            return ApiResponse<ScheduleDto>.ErrorResponse("Doctor profile not found");

        var startTime = TimeOnly.Parse(dto.StartTime);
        var endTime = TimeOnly.Parse(dto.EndTime);
        if (startTime >= endTime)
            return ApiResponse<ScheduleDto>.ErrorResponse("Start time must be before end time");

        var totalMinutes = (endTime - startTime).TotalMinutes;
        var slotsPerDay = (int)(totalMinutes / (int)dto.SlotDuration);

        var schedule = new Schedule
        {
            DoctorProfileId = doctor.Id,
            WorkingDays = dto.WorkingDays,
            StartTime = startTime,
            EndTime = endTime,
            SlotDuration = dto.SlotDuration,
            SlotsPerDay = slotsPerDay,
            DoctorProfile = doctor
        };

        await _scheduleRepo.AddAsync(schedule, ct);
        await _uow.SaveChangesAsync(ct);

        var result = _mapper.Map<ScheduleDto>(schedule);
        return ApiResponse<ScheduleDto>.SuccessResponse(result, "Schedule created successfully");
    }

    public async Task<ApiResponse<ScheduleDto>> UpdateScheduleAsync(Guid scheduleId, Guid doctorUserId, UpdateScheduleDto dto, CancellationToken ct = default)
    {
        var schedule = await _scheduleRepo.GetByIdAsync(scheduleId, ct);
        if (schedule == null)
            return ApiResponse<ScheduleDto>.ErrorResponse("Schedule not found");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || schedule.DoctorProfileId != doctor.Id)
            return ApiResponse<ScheduleDto>.ErrorResponse("Not authorized");

        if (dto.WorkingDays.HasValue) schedule.WorkingDays = dto.WorkingDays.Value;
        if (!string.IsNullOrWhiteSpace(dto.StartTime)) schedule.StartTime = TimeOnly.Parse(dto.StartTime);
        if (!string.IsNullOrWhiteSpace(dto.EndTime)) schedule.EndTime = TimeOnly.Parse(dto.EndTime);
        if (dto.SlotDuration.HasValue) schedule.SlotDuration = dto.SlotDuration.Value;

        if (schedule.StartTime >= schedule.EndTime)
            return ApiResponse<ScheduleDto>.ErrorResponse("Start time must be before end time");

        var totalMinutes = (schedule.EndTime - schedule.StartTime).TotalMinutes;
        schedule.SlotsPerDay = (int)(totalMinutes / (int)schedule.SlotDuration);
        schedule.UpdatedAt = DateTime.UtcNow;
        _scheduleRepo.Update(schedule);
        await _uow.SaveChangesAsync(ct);

        var result = _mapper.Map<ScheduleDto>(schedule);
        return ApiResponse<ScheduleDto>.SuccessResponse(result, "Schedule updated successfully");
    }

    public async Task<ApiResponse<List<ScheduleDto>>> GetDoctorSchedulesAsync(Guid doctorUserId, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null)
            return ApiResponse<List<ScheduleDto>>.ErrorResponse("Doctor not found");

        var allSchedules = await _scheduleRepo.GetAllAsync(ct);
        var schedules = allSchedules.Where(s => s.DoctorProfileId == doctor.Id && s.IsActive).ToList();
        var dtos = _mapper.Map<List<ScheduleDto>>(schedules);

        return ApiResponse<List<ScheduleDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse> DeleteScheduleAsync(Guid scheduleId, Guid doctorUserId, CancellationToken ct = default)
    {
        var schedule = await _scheduleRepo.GetByIdAsync(scheduleId, ct);
        if (schedule == null)
            return ApiResponse.ErrorResponse("Schedule not found");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || schedule.DoctorProfileId != doctor.Id)
            return ApiResponse.ErrorResponse("Not authorized");

        schedule.IsActive = false;
        schedule.UpdatedAt = DateTime.UtcNow;
        _scheduleRepo.Update(schedule);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Schedule deleted");
    }

    public async Task<ApiResponse<AvailableSlotsDto>> GetAvailableSlotsAsync(Guid doctorProfileId, DateOnly? date, CancellationToken ct = default)
    {
        var doctor = await _doctorRepo.GetByIdAsync(doctorProfileId, ct);
        if (doctor == null)
            return ApiResponse<AvailableSlotsDto>.ErrorResponse("Doctor not found");

        var allSlots = await _slotRepo.GetAllAsync(ct);
        var availableSlots = allSlots
            .Where(s => s.DoctorProfileId == doctorProfileId && s.Status == AppointmentSlotStatus.Available)
            .ToList();

        if (date.HasValue)
            availableSlots = availableSlots.Where(s => s.SlotDate == date.Value).ToList();

        var slotDtos = _mapper.Map<List<AppointmentSlotDto>>(availableSlots);
        var result = new AvailableSlotsDto
        {
            DoctorId = doctorProfileId,
            DoctorName = doctor.User?.FullName ?? "Unknown",
            Slots = slotDtos
        };

        return ApiResponse<AvailableSlotsDto>.SuccessResponse(result);
    }

    public async Task<ApiResponse> AddDayOffAsync(Guid doctorUserId, CreateDayOffDto dto, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null)
            return ApiResponse.ErrorResponse("Doctor not found");

        var dayOff = new DoctorDayOff
        {
            DoctorProfileId = doctor.Id,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            DoctorProfile = doctor
        };

        await _dayOffRepo.AddAsync(dayOff, ct);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Day off added");
    }
}
