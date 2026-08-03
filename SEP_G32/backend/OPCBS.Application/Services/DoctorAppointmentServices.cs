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
    private readonly IRepository<DoctorSpecialization> _doctorSpecRepo;
    private readonly IRepository<Specialization> _specRepo;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public DoctorService(
        IRepository<DoctorProfile> doctorRepo,
        IRepository<User> userRepo,
        IRepository<DoctorSpecialization> doctorSpecRepo,
        IRepository<Specialization> specRepo,
        IMapper mapper,
        IUnitOfWork uow)
    {
        _doctorRepo = doctorRepo;
        _userRepo = userRepo;
        _doctorSpecRepo = doctorSpecRepo;
        _specRepo = specRepo;
        _mapper = mapper;
        _uow = uow;
    }

    public async Task<ApiResponse<List<DoctorProfileDto>>> GetDoctorsAsync(string? search, Guid? specializationId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var query = allDoctors.Where(d => d.VerificationStatus == VerificationStatus.Approved && d.IsVisible);

        var list = query.ToList();

        // Manually populate User data since repository doesn't eager-load
        var allUsers = await _userRepo.GetAllAsync(ct);
        var userMap = allUsers.ToDictionary(u => u.Id);

        // Load specialization mapping
        var allDoctorSpecs = (await _doctorSpecRepo.GetAllAsync(ct)).ToList();
        var allSpecs = (await _specRepo.GetAllAsync(ct)).ToList();
        var specMap = allSpecs.ToDictionary(s => s.Id, s => s.Name);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.ToLower();
            list = list.Where(d =>
            {
                var userName = userMap.GetValueOrDefault(d.UserId)?.FullName?.ToLower();
                return (userName != null && userName.Contains(kw))
                    || (d.ProfessionalTitle?.ToLower().Contains(kw) == true)
                    || (d.Biography?.ToLower().Contains(kw) == true);
            }).ToList();
        }

        var total = list.Count;
        var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var dtos = items.Select(d =>
        {
            var user = userMap.GetValueOrDefault(d.UserId);
            var specNames = allDoctorSpecs
                .Where(ds => ds.DoctorProfileId == d.Id)
                .Select(ds => specMap.GetValueOrDefault(ds.SpecializationId))
                .Where(n => !string.IsNullOrEmpty(n))
                .Select(n => n!)
                .ToList();
            return new DoctorProfileDto
            {
                Id = d.Id,
                FullName = user?.FullName ?? "Unknown",
                AvatarUrl = user?.AvatarUrl,
                ProfessionalTitle = d.ProfessionalTitle,
                Biography = d.Biography,
                ExperienceYears = d.ExperienceYears,
                VerificationStatus = d.VerificationStatus,
                IsVisible = d.IsVisible,
                AverageRating = d.AverageRating,
                ReviewCount = d.ReviewCount,
                Specializations = specNames,
                Gender = d.Gender?.ToString(),
                DateOfBirth = d.DateOfBirth,
                Address = d.Address,
                Education = d.Education,
                CareerBackground = d.CareerBackground,
                ConsultationFee = d.ConsultationFee,
                CareApproach = d.CareApproach,
                Languages = d.Languages,
                ConsultationTypes = d.ConsultationTypes,
                LicenseNumber = d.LicenseNumber,
                Email = user?.Email,
                PhoneNumber = user?.PhoneNumber
            };
        }).ToList();

        return ApiResponse<List<DoctorProfileDto>>.SuccessResponse(dtos, pagination: new PaginationMetadata
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        });
    }

    public async Task<ApiResponse<DoctorProfileDto>> GetDoctorByIdAsync(Guid doctorProfileId, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.Id == doctorProfileId || d.UserId == doctorProfileId);
        if (doctor == null)
            return ApiResponse<DoctorProfileDto>.ErrorResponse("Doctor not found");

        var allUsers = await _userRepo.GetAllAsync(ct);
        var user = allUsers.FirstOrDefault(u => u.Id == doctor.UserId);
        var specNames = await GetSpecNamesForDoctor(doctor.Id, ct);
        var dto = BuildDoctorDto(doctor, user, specNames);
        return ApiResponse<DoctorProfileDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<DoctorProfileDto>> GetDoctorProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == userId);
        if (doctor == null)
            return ApiResponse<DoctorProfileDto>.ErrorResponse("Doctor profile not found");

        var allUsers = await _userRepo.GetAllAsync(ct);
        var user = allUsers.FirstOrDefault(u => u.Id == userId);
        var specNames = await GetSpecNamesForDoctor(doctor.Id, ct);
        var dto = BuildDoctorDto(doctor, user, specNames);
        return ApiResponse<DoctorProfileDto>.SuccessResponse(dto);
    }

    private async Task<List<string>> GetSpecNamesForDoctor(Guid doctorProfileId, CancellationToken ct)
    {
        var allDoctorSpecs = (await _doctorSpecRepo.GetAllAsync(ct)).ToList();
        var allSpecs = (await _specRepo.GetAllAsync(ct)).ToList();
        var specMap = allSpecs.ToDictionary(s => s.Id, s => s.Name);
        return allDoctorSpecs
            .Where(ds => ds.DoctorProfileId == doctorProfileId)
            .Select(ds => specMap.GetValueOrDefault(ds.SpecializationId))
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n!)
            .ToList();
    }

    private static DoctorProfileDto BuildDoctorDto(DoctorProfile d, User? user, List<string>? specializations = null)
    {
        return new DoctorProfileDto
        {
            Id = d.Id,
            FullName = user?.FullName ?? "Unknown",
            AvatarUrl = user?.AvatarUrl,
            ProfessionalTitle = d.ProfessionalTitle,
            Biography = d.Biography,
            ExperienceYears = d.ExperienceYears,
            VerificationStatus = d.VerificationStatus,
            IsVisible = d.IsVisible,
            AverageRating = d.AverageRating,
            ReviewCount = d.ReviewCount,
            Specializations = specializations ?? new List<string>(),
            Gender = d.Gender?.ToString(),
            DateOfBirth = d.DateOfBirth,
            Address = d.Address,
            Education = d.Education,
            CareerBackground = d.CareerBackground,
            ConsultationFee = d.ConsultationFee,
            CareApproach = d.CareApproach,
            Languages = d.Languages,
            ConsultationTypes = d.ConsultationTypes,
            LicenseNumber = d.LicenseNumber,
            Email = user?.Email,
            PhoneNumber = user?.PhoneNumber
        };
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
        if (!string.IsNullOrEmpty(dto.Gender))
        {
            if (Enum.TryParse<Gender>(dto.Gender, true, out var g))
                doctor.Gender = g;
        }
        else
        {
            doctor.Gender = null;
        }
        if (dto.DateOfBirth.HasValue)
            doctor.DateOfBirth = dto.DateOfBirth.Value;
        if (dto.Address != null)
            doctor.Address = dto.Address;
        if (dto.Education != null)
            doctor.Education = dto.Education;
        if (dto.CareerBackground != null)
            doctor.CareerBackground = dto.CareerBackground;
        if (dto.ConsultationFee.HasValue)
            doctor.ConsultationFee = dto.ConsultationFee.Value;
        if (dto.CareApproach != null)
            doctor.CareApproach = dto.CareApproach;
        if (dto.Languages != null)
            doctor.Languages = dto.Languages;
        if (dto.ConsultationTypes != null)
            doctor.ConsultationTypes = dto.ConsultationTypes;
        if (dto.LicenseNumber != null)
            doctor.LicenseNumber = dto.LicenseNumber;

        // Update specializations
        if (dto.SpecializationIds != null)
        {
            var existingSpecs = (await _doctorSpecRepo.GetAllAsync(ct))
                .Where(ds => ds.DoctorProfileId == doctor.Id)
                .ToList();

            // Remove specializations that are no longer selected
            foreach (var spec in existingSpecs)
            {
                if (!dto.SpecializationIds.Contains(spec.SpecializationId))
                {
                    _doctorSpecRepo.Delete(spec);
                }
            }

            // Add new specializations
            var existingIds = existingSpecs.Select(ds => ds.SpecializationId).ToHashSet();
            foreach (var specId in dto.SpecializationIds)
            {
                if (!existingIds.Contains(specId))
                {
                    await _doctorSpecRepo.AddAsync(new DoctorSpecialization
                    {
                        DoctorProfileId = doctor.Id,
                        SpecializationId = specId,
                        DoctorProfile = null!,
                        Specialization = null!
                    }, ct);
                }
            }
        }

        doctor.UpdatedAt = DateTime.UtcNow;
        _doctorRepo.Update(doctor);
        await _uow.SaveChangesAsync(ct);

        var allUsers = await _userRepo.GetAllAsync(ct);
        var user = allUsers.FirstOrDefault(u => u.Id == userId);
        var specNames = await GetSpecNamesForDoctor(doctor.Id, ct);
        var result = BuildDoctorDto(doctor, user, specNames);
        return ApiResponse<DoctorProfileDto>.SuccessResponse(result, "Profile updated successfully");
    }
}

public class AppointmentService : IAppointmentService
{
    private readonly IRepository<Appointment> _apptRepo;
    private readonly IRepository<AppointmentSlot> _slotRepo;
    private readonly IRepository<AppointmentHistory> _historyRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<DoctorSubscription> _subscriptionRepo;
    private readonly IRepository<TreatmentPackage> _packageRepo;
    private readonly IRepository<ConsultationNote> _consultationNoteRepo;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IRepository<PsychometricSubmission>? _psychRepo;
    private readonly IRepository<TreatmentCase>? _treatmentCaseRepo;
    private readonly IRepository<PatientRecord>? _patientRecordRepo;

    public AppointmentService(
        IRepository<Appointment> apptRepo,
        IRepository<AppointmentSlot> slotRepo,
        IRepository<AppointmentHistory> historyRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<User> userRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<DoctorSubscription> subscriptionRepo,
        IRepository<TreatmentPackage> packageRepo,
        IRepository<ConsultationNote> consultationNoteRepo,
        INotificationService notificationService,
        IEmailService emailService,
        IUnitOfWork uow,
        IMapper mapper,
        IRepository<PsychometricSubmission>? psychRepo = null,
        IRepository<TreatmentCase>? treatmentCaseRepo = null,
        IRepository<PatientRecord>? patientRecordRepo = null)
    {
        _apptRepo = apptRepo;
        _slotRepo = slotRepo;
        _historyRepo = historyRepo;
        _doctorRepo = doctorRepo;
        _userRepo = userRepo;
        _patientRepo = patientRepo;
        _subscriptionRepo = subscriptionRepo;
        _packageRepo = packageRepo;
        _consultationNoteRepo = consultationNoteRepo;
        _notificationService = notificationService;
        _emailService = emailService;
        _uow = uow;
        _mapper = mapper;
        _psychRepo = psychRepo;
        _treatmentCaseRepo = treatmentCaseRepo;
        _patientRecordRepo = patientRecordRepo;
    }

    public async Task<ApiResponse<AppointmentDto>> CreateAppointmentAsync(CreateAppointmentDto dto, Guid? patientUserId, CancellationToken ct = default)
    {
        // Validate doctor exists and is verified (BOOK-04)
        // DoctorId may be either DoctorProfile.Id (PK) or User.Id (from enriched DTOs)
        var doctor = await _doctorRepo.GetByIdAsync(dto.DoctorId, ct);
        if (doctor == null)
        {
            // Fallback: DoctorId might be User.Id (EnrichNamesAsync swaps DoctorId to UserId)
            var allDoctors = await _doctorRepo.GetAllAsync(ct);
            doctor = allDoctors.FirstOrDefault(d => d.UserId == dto.DoctorId);
        }
        if (doctor == null || doctor.VerificationStatus != VerificationStatus.Approved)
            return ApiResponse<AppointmentDto>.ErrorResponse("Doctor not found or not verified");

        // Normalize: ensure we use DoctorProfile.Id for all downstream operations
        dto.DoctorId = doctor.Id;

        // BOOK-04 / DOC-12 / SP-01: Doctor must have active subscription
        var allSubs = await _subscriptionRepo.GetAllAsync(ct);
        var hasActiveSub = allSubs.Any(s =>
            s.DoctorProfileId == doctor.Id &&
            s.Status == SubscriptionStatus.Active &&
            s.ExpirationDate > DateTime.UtcNow);
        if (!hasActiveSub)
            return ApiResponse<AppointmentDto>.ErrorResponse("Doctor does not have an active service subscription");

        // Validate slot exists and is available
        var slot = await _slotRepo.GetByIdAsync(dto.AppointmentSlotId, ct);
        if (slot == null || (slot.Status != AppointmentSlotStatus.Available && slot.Status != AppointmentSlotStatus.Booked))
            return ApiResponse<AppointmentDto>.ErrorResponse("Slot is not available for booking.");
        // Check capacity
        if (slot.CurrentBookings >= slot.MaxPatients)
            return ApiResponse<AppointmentDto>.ErrorResponse("Khung giờ này đã đạt giới hạn số lượng bệnh nhân.");

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
        else
        {
            if (string.IsNullOrWhiteSpace(dto.GuestName))
                return ApiResponse<AppointmentDto>.ErrorResponse("Guest name is required.");
            if (string.IsNullOrWhiteSpace(dto.GuestEmail))
                return ApiResponse<AppointmentDto>.ErrorResponse("Guest email is required.");
            if (string.IsNullOrWhiteSpace(dto.GuestPhoneNumber))
                return ApiResponse<AppointmentDto>.ErrorResponse("Guest phone number is required.");
        }

        var allAppts = await _apptRepo.GetAllAsync(ct);
        var allSlots = await _slotRepo.GetAllAsync(ct);
        var slotDict = allSlots.GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.First());

        // 1. Ensure the patient hasn't already booked this specific slot
        var isSlotBookedByPatient = allAppts.Any(a =>
            a.AppointmentSlotId == dto.AppointmentSlotId &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Rejected &&
            (patientProfileId.HasValue ? a.PatientId == patientProfileId : a.GuestEmail == dto.GuestEmail));
        if (isSlotBookedByPatient)
            return ApiResponse<AppointmentDto>.ErrorResponse("Bạn đã đặt khung giờ này rồi.");

        // 2. Ensure patient does not have another appointment in the same time slot
        if (patientProfileId.HasValue)
        {
            var hasOverlapTime = allAppts.Any(a =>
                a.PatientId == patientProfileId.Value &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.Rejected &&
                slotDict.TryGetValue(a.AppointmentSlotId, out var s) &&
                s.SlotDate == slot.SlotDate &&
                s.StartTime == slot.StartTime);

            if (hasOverlapTime)
                return ApiResponse<AppointmentDto>.ErrorResponse("Bạn đã có một lịch hẹn khác trong khung giờ này.");
        }

        TreatmentPackage? treatmentPackage = null;
        if (dto.TreatmentPackageId.HasValue && dto.TreatmentPackageId.Value != Guid.Empty)
        {
            treatmentPackage = await _packageRepo.GetByIdAsync(dto.TreatmentPackageId.Value, ct);
            if (treatmentPackage == null)
                return ApiResponse<AppointmentDto>.ErrorResponse("Không tìm thấy gói điều trị.");
            if (treatmentPackage.RemainingSessions <= 0)
                return ApiResponse<AppointmentDto>.ErrorResponse("Gói điều trị đã hết phiên tư vấn.");
            if (treatmentPackage.Status != TreatmentPackageStatus.Active && treatmentPackage.Status != TreatmentPackageStatus.Accepted)
                return ApiResponse<AppointmentDto>.ErrorResponse("Gói điều trị chưa được kích hoạt hoặc đã bị hủy.");
            if (treatmentPackage.ExpirationDate < DateTime.UtcNow)
                return ApiResponse<AppointmentDto>.ErrorResponse("Gói điều trị đã hết hạn sử dụng.");
        }
        else if (patientProfileId.HasValue)
        {
            var allPackages = await _packageRepo.GetAllAsync(ct);
            treatmentPackage = allPackages.FirstOrDefault(p =>
                p.PatientId == patientProfileId.Value &&
                p.DoctorId == dto.DoctorId &&
                !p.IsDeleted &&
                (p.Status == TreatmentPackageStatus.Active || p.Status == TreatmentPackageStatus.Accepted) &&
                p.ExpirationDate > DateTime.UtcNow &&
                p.RemainingSessions > 0);

            if (treatmentPackage != null)
            {
                dto.TreatmentPackageId = treatmentPackage.Id;
            }
        }

        await _uow.BeginTransactionAsync(ct);
        try
        {
            if (treatmentPackage != null)
            {
                treatmentPackage.RemainingSessions--;
                _packageRepo.Update(treatmentPackage);
            }

            slot.CurrentBookings++;
            if (slot.CurrentBookings >= slot.MaxPatients)
                slot.Status = AppointmentSlotStatus.Booked;
            _slotRepo.Update(slot);

            var bookingCode = $"OPCBS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            // If pre-evaluation fields are empty (e.g. for returning patients), auto-inherit from previous appointment
            if (patientProfileId.HasValue)
            {
                var prevAppt = allAppts
                    .Where(a => a.PatientId == patientProfileId.Value && (!string.IsNullOrEmpty(a.Symptoms) || !string.IsNullOrEmpty(a.MedicalHistory)))
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefault();

                if (prevAppt != null)
                {
                    if (string.IsNullOrWhiteSpace(dto.Symptoms)) dto.Symptoms = prevAppt.Symptoms;
                    if (string.IsNullOrWhiteSpace(dto.MedicalHistory)) dto.MedicalHistory = prevAppt.MedicalHistory;
                    if (string.IsNullOrWhiteSpace(dto.Expectations)) dto.Expectations = prevAppt.Expectations;
                }
            }

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
                Symptoms = dto.Symptoms,
                MedicalHistory = dto.MedicalHistory,
                Expectations = dto.Expectations,
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

            // Manually build DTO to avoid null navigation properties
            var allUsers = await _userRepo.GetAllAsync(ct);
            var doctorUser = allUsers.FirstOrDefault(u => u.Id == doctor.UserId);
            var patientName = appointment.GuestName ?? "Patient";
            if (patientProfileId.HasValue)
            {
                var allPatients2 = await _patientRepo.GetAllAsync(ct);
                var pat = allPatients2.FirstOrDefault(p => p.Id == patientProfileId.Value);
                if (pat != null)
                {
                    var patUser = allUsers.FirstOrDefault(u => u.Id == pat.UserId);
                    patientName = patUser?.FullName ?? patientName;
                }
            }

            var result = new AppointmentDto
            {
                Id = appointment.Id,
                BookingCode = appointment.BookingCode,
                DoctorName = doctorUser?.FullName ?? "Unknown",
                PatientName = patientName,
                AppointmentDate = slot.SlotDate.ToString("yyyy-MM-dd"),
                StartTime = slot.StartTime.ToString("HH:mm"),
                EndTime = slot.EndTime.ToString("HH:mm"),
                Status = appointment.Status,
                Notes = appointment.Notes,
                Symptoms = appointment.Symptoms,
                MedicalHistory = appointment.MedicalHistory,
                Expectations = appointment.Expectations
            };

            // Notify doctor about new booking
            try
            {
                await _notificationService.CreateNotificationAsync(
                    doctor.UserId,
                    "📅 New Appointment",
                    $"You have a new appointment from {patientName} on {slot.SlotDate:MM/dd/yyyy} at {slot.StartTime.ToString("HH':'mm")}.",
                    NotificationType.Appointment,
                    appointment.Id,
                    "Appointment",
                    ct);
            }
            catch { /* Non-critical — don't fail the booking */ }

            return ApiResponse<AppointmentDto>.SuccessResponse(result, "Appointment booked successfully");
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task EnrichAppointmentListDtosAsync(List<AppointmentListItemDto> dtos, List<Appointment> appointments, CancellationToken ct)
    {
        if (!dtos.Any()) return;
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var allUsers = await _userRepo.GetAllAsync(ct);
        var allSlots = await _slotRepo.GetAllAsync(ct);

        var userDict = allUsers.GroupBy(u => u.Id).ToDictionary(g => g.Key, g => g.First().FullName);
        var doctorUserMap = allDoctors.GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.First().UserId);
        var patientUserMap = allPatients.GroupBy(p => p.Id).ToDictionary(g => g.Key, g => g.First().UserId);
        var slotDict = allSlots.GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.First());

        foreach (var dto in dtos)
        {
            var appt = appointments.FirstOrDefault(a => a.Id == dto.Id);
            if (appt == null) continue;

            // Set DoctorId (UserId, not ProfileId)
            if (doctorUserMap.TryGetValue(appt.DoctorId, out var docUserId))
            {
                dto.DoctorProfileId = appt.DoctorId;
                dto.DoctorId = docUserId;
                if (userDict.TryGetValue(docUserId, out var docName))
                    dto.DoctorName = docName;
                else
                    dto.DoctorName = "Doctor";
            }
            else
            {
                dto.DoctorName = "Doctor";
            }

            // Set PatientId & PatientName
            if (appt.PatientId.HasValue && patientUserMap.TryGetValue(appt.PatientId.Value, out var patUserId))
            {
                dto.PatientId = patUserId;
            }

            if (!string.IsNullOrWhiteSpace(appt.GuestName))
            {
                dto.PatientName = appt.GuestName;
            }
            else if (dto.PatientId.HasValue && userDict.TryGetValue(dto.PatientId.Value, out var patName) && !string.IsNullOrWhiteSpace(patName))
            {
                dto.PatientName = patName;
            }
            else
            {
                dto.PatientName = "Guest";
            }

            if (slotDict.TryGetValue(appt.AppointmentSlotId, out var slot))
            {
                dto.AppointmentDate = slot.SlotDate.ToString("yyyy-MM-dd");
                dto.StartTime = slot.StartTime.ToString("HH:mm");
                dto.EndTime = slot.EndTime.ToString("HH:mm");
                dto.Fee = slot.Price;

                var slotDateTime = slot.SlotDate.ToDateTime(slot.StartTime);
                dto.CanReschedule = appt.Status == AppointmentStatus.Approved &&
                                    slotDateTime > DateTime.Now;
            }

            dto.ProposedSlotId = appt.ProposedSlotId;
            if (appt.ProposedSlotId.HasValue && slotDict.TryGetValue(appt.ProposedSlotId.Value, out var propSlot))
            {
                dto.ProposedSlotDate = propSlot.SlotDate.ToString("yyyy-MM-dd");
                dto.ProposedSlotStartTime = propSlot.StartTime.ToString("HH:mm");
                dto.ProposedSlotEndTime = propSlot.EndTime.ToString("HH:mm");
            }

            dto.TreatmentPackageId = appt.TreatmentPackageId;
        }
    }

    private async Task EnrichAppointmentDtoAsync(AppointmentDto dto, Appointment appt, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var allUsers = await _userRepo.GetAllAsync(ct);
        var allSlots = await _slotRepo.GetAllAsync(ct);

        var userDict = allUsers.GroupBy(u => u.Id).ToDictionary(g => g.Key, g => g.First().FullName);
        var doctorUserMap = allDoctors.GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.First().UserId);
        var patientUserMap = allPatients.GroupBy(p => p.Id).ToDictionary(g => g.Key, g => g.First().UserId);
        var slotDict = allSlots.GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.First());

        if (doctorUserMap.TryGetValue(appt.DoctorId, out var docUserId))
        {
            dto.DoctorProfileId = appt.DoctorId;
            dto.DoctorId = docUserId;
            if (userDict.TryGetValue(docUserId, out var docName))
                dto.DoctorName = docName;
            else
                dto.DoctorName = "Doctor";
        }
        else
        {
            dto.DoctorName = "Doctor";
        }

        // Set PatientId & PatientName
        if (appt.PatientId.HasValue && patientUserMap.TryGetValue(appt.PatientId.Value, out var patUserId2))
        {
            dto.PatientId = patUserId2;
        }

        if (!string.IsNullOrWhiteSpace(appt.GuestName))
        {
            dto.PatientName = appt.GuestName;
        }
        else if (dto.PatientId.HasValue && userDict.TryGetValue(dto.PatientId.Value, out var patName2) && !string.IsNullOrWhiteSpace(patName2))
        {
            dto.PatientName = patName2;
        }
        else
        {
            dto.PatientName = "Guest";
        }

        if (slotDict.TryGetValue(appt.AppointmentSlotId, out var slot))
        {
            dto.AppointmentDate = slot.SlotDate.ToString("yyyy-MM-dd");
            dto.StartTime = slot.StartTime.ToString("HH:mm");
            dto.EndTime = slot.EndTime.ToString("HH:mm");
            dto.Fee = slot.Price;

            var slotDateTime = slot.SlotDate.ToDateTime(slot.StartTime);
            dto.CanReschedule = appt.Status == AppointmentStatus.Approved &&
                                slotDateTime > DateTime.Now;
        }

        dto.ProposedSlotId = appt.ProposedSlotId;
        dto.RescheduleReason = appt.RescheduleReason;
        if (appt.ProposedSlotId.HasValue && slotDict.TryGetValue(appt.ProposedSlotId.Value, out var proposedSlot))
        {
            dto.ProposedSlotDate = proposedSlot.SlotDate.ToString("yyyy-MM-dd");
            dto.ProposedSlotStartTime = proposedSlot.StartTime.ToString("HH:mm");
            dto.ProposedSlotEndTime = proposedSlot.EndTime.ToString("HH:mm");
        }

        // Enrich with TreatmentPackage info
        dto.TreatmentPackageId = appt.TreatmentPackageId;
        if (appt.TreatmentPackageId.HasValue)
        {
            var pkg = await _packageRepo.GetByIdAsync(appt.TreatmentPackageId.Value, ct);
            dto.TreatmentPackageName = pkg?.Name;
        }

        // Enrich with visit count (all non-cancelled appointments with same doctor, excluding current)
        if (appt.PatientId.HasValue)
        {
            var allAppts = await _apptRepo.GetAllAsync(ct);
            dto.VisitCount = allAppts.Count(a =>
                a.PatientId == appt.PatientId &&
                a.DoctorId == appt.DoctorId &&
                a.Id != appt.Id &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.Rejected &&
                !a.IsDeleted);
        }

        // Enrich with cancellation reason
        dto.CancellationReason = appt.CancellationReason;
    }

    public async Task<ApiResponse<List<AppointmentListItemDto>>> GetMyAppointmentsAsync(Guid userId, int page = 1, int pageSize = 10, string? status = null, string? search = null, string? view = null, CancellationToken ct = default)
    {
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == userId || p.Id == userId);
        if (patient == null)
            return ApiResponse<List<AppointmentListItemDto>>.ErrorResponse("Patient profile not found");

        var allAppts = await _apptRepo.GetAllAsync(ct);
        var myAppts = allAppts.Where(a => (a.PatientId == patient.Id || a.PatientId == patient.UserId || a.PatientId == userId) && !a.IsDeleted).ToList();

        if (!string.IsNullOrEmpty(view))
        {
            if (view.Equals("active", StringComparison.OrdinalIgnoreCase))
            {
                myAppts = myAppts.Where(a => AppointmentStatusHelper.IsActive(a.Status)).ToList();
            }
            else if (view.Equals("history", StringComparison.OrdinalIgnoreCase))
            {
                myAppts = myAppts.Where(a => AppointmentStatusHelper.IsHistory(a.Status)).ToList();
            }
        }
        else if (string.IsNullOrEmpty(status))
        {
            myAppts = myAppts.Where(a => AppointmentStatusHelper.IsActive(a.Status)).ToList();
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            {
                myAppts = myAppts.Where(a => a.Status == statusEnum).ToList();
            }
            else if (int.TryParse(status, out var statusVal) && Enum.IsDefined(typeof(AppointmentStatus), statusVal))
            {
                myAppts = myAppts.Where(a => a.Status == (AppointmentStatus)statusVal).ToList();
            }
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            var allDoctors = await _doctorRepo.GetAllAsync(ct);
            var allUsers = await _userRepo.GetAllAsync(ct);
            var docUsers = allDoctors.Join(allUsers, d => d.UserId, u => u.Id, (d, u) => new { DoctorId = d.Id, FullName = u.FullName }).ToList();

            myAppts = myAppts.Where(a =>
                (a.BookingCode != null && a.BookingCode.ToLower().Contains(searchLower)) ||
                (a.Notes != null && a.Notes.ToLower().Contains(searchLower)) ||
                docUsers.Any(d => d.DoctorId == a.DoctorId && d.FullName.ToLower().Contains(searchLower))
            ).ToList();
        }

        var total = myAppts.Count;
        var items = myAppts.OrderByDescending(a => a.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = _mapper.Map<List<AppointmentListItemDto>>(items);
        await EnrichAppointmentListDtosAsync(dtos, items, ct);

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
        await EnrichAppointmentDtoAsync(dto, appointment, ct);
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
        await EnrichAppointmentDtoAsync(result, appointment, ct);
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

        if (appointment.Status == AppointmentStatus.Rejected)
            return ApiResponse.ErrorResponse("Rejected appointments cannot be cancelled");

        var slot = await _slotRepo.GetByIdAsync(appointment.AppointmentSlotId, ct);

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.Id == appointment.DoctorId);
        var doctorUserId = doctor?.UserId;

        Guid? patientUserId = null;
        if (appointment.PatientId.HasValue)
        {
            var allPatients = await _patientRepo.GetAllAsync(ct);
            var patient = allPatients.FirstOrDefault(p => p.Id == appointment.PatientId.Value);
            patientUserId = patient?.UserId;
        }

        if (appointment.PatientId.HasValue && userId != doctorUserId && userId != patientUserId)
        {
            return ApiResponse.ErrorResponse("You are not authorized to cancel this appointment");
        }

        // APPT-05: 24-hour cancellation policy (only applies to patient)
        var isPatientCancellation = appointment.PatientId.HasValue && patientUserId == userId;
        if (isPatientCancellation)
        {
            if (slot != null)
            {
                var slotDateTime = slot.SlotDate.ToDateTime(slot.StartTime);
                if (slotDateTime - DateTime.UtcNow < TimeSpan.FromHours(24))
                    return ApiResponse.ErrorResponse("Cannot cancel an appointment less than 24 hours before the scheduled time");
            }
        }

        var prevStatus = appointment.Status;
        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.CancellationReason = dto.Reason;
        appointment.UpdatedAt = DateTime.UtcNow;

        // Release the slot
        if (slot != null)
        {
            slot.CurrentBookings = Math.Max(0, slot.CurrentBookings - 1);
            if (slot.CurrentBookings < slot.MaxPatients)
                slot.Status = AppointmentSlotStatus.Available;
            _slotRepo.Update(slot);
        }

        if (appointment.TreatmentPackageId.HasValue && appointment.TreatmentPackageId.Value != Guid.Empty)
        {
            var package = await _packageRepo.GetByIdAsync(appointment.TreatmentPackageId.Value, ct);
            if (package != null)
            {
                package.RemainingSessions++;
                if (package.RemainingSessions > package.SessionQuantity)
                    package.RemainingSessions = package.SessionQuantity;
                _packageRepo.Update(package);
            }
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

        // Notify the other party about cancellation + send email
        try
        {
            var allUsers = await _userRepo.GetAllAsync(ct);
            var allPatients = await _patientRepo.GetAllAsync(ct);
            var cancelDoctor = allDoctors.FirstOrDefault(d => d.Id == appointment.DoctorId);
            var cancelPatient = appointment.PatientId.HasValue ? allPatients.FirstOrDefault(p => p.Id == appointment.PatientId.Value) : null;
            var cancelSlot = await _slotRepo.GetByIdAsync(appointment.AppointmentSlotId, ct);
            var cancelDateStr = cancelSlot?.SlotDate.ToString("MM/dd/yyyy") ?? "";
            var cancelTimeStr = cancelSlot?.StartTime.ToString("HH:mm") ?? "";

            // Check if canceller is the patient → notify doctor; else notify patient
            var isPatientCancel = cancelPatient != null && cancelPatient.UserId == userId;
            if (isPatientCancel && cancelDoctor != null)
            {
                var patUser = allUsers.FirstOrDefault(u => u.Id == userId);
                await _notificationService.CreateNotificationAsync(
                    cancelDoctor.UserId,
                    "🚫 Appointment Cancelled",
                    $"{patUser?.FullName ?? "Patient"} has cancelled the appointment on {cancelDateStr} at {cancelTimeStr}.",
                    NotificationType.Appointment, appointmentId, "Appointment", ct);

                // Email to doctor
                var docUser = allUsers.FirstOrDefault(u => u.Id == cancelDoctor.UserId);
                if (docUser?.Email != null)
                {
                    await _emailService.SendAppointmentCancelledEmailAsync(
                        docUser.Email, docUser.FullName ?? "Doctor",
                        patUser?.FullName ?? "Patient", cancelDateStr,
                        dto.Reason ?? "No reason provided", ct);
                }
            }
            else if (!isPatientCancel && cancelPatient != null)
            {
                var docUser = cancelDoctor != null ? allUsers.FirstOrDefault(u => u.Id == cancelDoctor.UserId) : null;
                await _notificationService.CreateNotificationAsync(
                    cancelPatient.UserId,
                    "🚫 Appointment Cancelled",
                    $"Dr. {docUser?.FullName ?? "your doctor"} has cancelled the appointment on {cancelDateStr} at {cancelTimeStr}.",
                    NotificationType.Appointment, appointmentId, "Appointment", ct);

                // Email to patient
                var patUser = allUsers.FirstOrDefault(u => u.Id == cancelPatient.UserId);
                if (patUser?.Email != null)
                {
                    await _emailService.SendAppointmentCancelledEmailAsync(
                        patUser.Email, patUser.FullName ?? "Patient",
                        $"Dr. {docUser?.FullName ?? "your doctor"}", cancelDateStr,
                        dto.Reason ?? "No reason provided", ct);
                }
            }
        }
        catch { }

        return ApiResponse.SuccessResponse("Appointment cancelled successfully");
    }

    public async Task<ApiResponse> RescheduleAppointmentAsync(Guid appointmentId, Guid userId, RescheduleAppointmentDto dto, CancellationToken ct = default)
    {
        return await RequestRescheduleAsync(appointmentId, userId, dto, ct);
    }

    public async Task<ApiResponse> RequestRescheduleAsync(Guid appointmentId, Guid patientUserId, RescheduleAppointmentDto dto, CancellationToken ct = default)
    {
        var appointment = await _apptRepo.GetByIdAsync(appointmentId, ct);
        if (appointment == null)
            return ApiResponse.ErrorResponse("Appointment not found");

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        if (patient == null || appointment.PatientId != patient.Id)
            return ApiResponse.ErrorResponse("You are not authorized to request a reschedule for this appointment.");

        if (appointment.Status != AppointmentStatus.Approved)
            return ApiResponse.ErrorResponse("Only approved appointments can be rescheduled.");

        if (appointment.Status == AppointmentStatus.RescheduleRequested)
            return ApiResponse.ErrorResponse("A reschedule request is already pending for this appointment.");

        var currentSlot = await _slotRepo.GetByIdAsync(appointment.AppointmentSlotId, ct);
        if (currentSlot != null)
        {
            var slotStartDateTime = currentSlot.SlotDate.ToDateTime(currentSlot.StartTime);
            if ((slotStartDateTime - DateTime.UtcNow).TotalHours < 24)
            {
                return ApiResponse.ErrorResponse("Reschedule requests must be made at least 24 hours prior to appointment start time.");
            }
        }

        var newSlot = await _slotRepo.GetByIdAsync(dto.NewSlotId, ct);
        if (newSlot == null)
            return ApiResponse.ErrorResponse("The selected time slot does not exist.");

        if (newSlot.Status != AppointmentSlotStatus.Available)
            return ApiResponse.ErrorResponse("The selected time slot is no longer available.");

        if (newSlot.DoctorProfileId != appointment.DoctorId)
            return ApiResponse.ErrorResponse("The selected time slot belongs to a different doctor.");

        var prevStatus = appointment.Status;
        appointment.Status = AppointmentStatus.RescheduleRequested;
        appointment.ProposedSlotId = dto.NewSlotId;
        appointment.RescheduleReason = dto.Reason;
        appointment.UpdatedAt = DateTime.UtcNow;

        _apptRepo.Update(appointment);
        await _historyRepo.AddAsync(new AppointmentHistory
        {
            AppointmentId = appointmentId,
            PreviousStatus = prevStatus,
            NewStatus = AppointmentStatus.RescheduleRequested,
            Reason = dto.Reason ?? "Reschedule requested by patient",
            Appointment = appointment
        }, ct);

        await _uow.SaveChangesAsync(ct);

        // Notify Doctor
        try
        {
            var allDoctors = await _doctorRepo.GetAllAsync(ct);
            var doctor = allDoctors.FirstOrDefault(d => d.Id == appointment.DoctorId);
            if (doctor != null)
            {
                var allUsers = await _userRepo.GetAllAsync(ct);
                var patientUser = allUsers.FirstOrDefault(u => u.Id == patientUserId);
                var patName = patientUser?.FullName ?? "Patient";
                await _notificationService.CreateNotificationAsync(
                    doctor.UserId,
                    "🔄 Reschedule Requested",
                    $"{patName} requested to reschedule appointment {appointment.BookingCode} to {newSlot.SlotDate:MM/dd/yyyy} at {newSlot.StartTime:HH:mm}.",
                    NotificationType.Appointment,
                    appointmentId,
                    "Appointment",
                    ct);
            }
        }
        catch { }

        return ApiResponse.SuccessResponse("Reschedule request submitted successfully. Awaiting doctor approval.");
    }

    public async Task<ApiResponse> ApproveRescheduleAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct = default)
    {
        var appointment = await _apptRepo.GetByIdAsync(appointmentId, ct);
        if (appointment == null)
            return ApiResponse.ErrorResponse("Appointment not found");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || appointment.DoctorId != doctor.Id)
            return ApiResponse.ErrorResponse("You are not authorized to approve this reschedule request.");

        if (appointment.Status != AppointmentStatus.RescheduleRequested)
            return ApiResponse.ErrorResponse("No pending reschedule request found for this appointment.");

        if (!appointment.ProposedSlotId.HasValue)
            return ApiResponse.ErrorResponse("No proposed slot specified for reschedule.");

        var newSlot = await _slotRepo.GetByIdAsync(appointment.ProposedSlotId.Value, ct);
        if (newSlot == null)
            return ApiResponse.ErrorResponse("Proposed time slot no longer exists.");

        if (newSlot.Status != AppointmentSlotStatus.Available)
            return ApiResponse.ErrorResponse("Proposed time slot is no longer available.");

        // Release old slot
        var oldSlot = await _slotRepo.GetByIdAsync(appointment.AppointmentSlotId, ct);
        if (oldSlot != null)
        {
            oldSlot.CurrentBookings = Math.Max(0, oldSlot.CurrentBookings - 1);
            if (oldSlot.CurrentBookings < oldSlot.MaxPatients)
                oldSlot.Status = AppointmentSlotStatus.Available;
            _slotRepo.Update(oldSlot);
        }

        // Lock new slot
        newSlot.CurrentBookings++;
        newSlot.Status = AppointmentSlotStatus.Booked;
        _slotRepo.Update(newSlot);

        var prevStatus = appointment.Status;
        appointment.AppointmentSlotId = newSlot.Id;
        appointment.ProposedSlotId = null;
        appointment.AppointmentDate = newSlot.SlotDate.ToDateTime(newSlot.StartTime);
        appointment.Status = AppointmentStatus.Approved;
        appointment.ApprovedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        _apptRepo.Update(appointment);
        await _historyRepo.AddAsync(new AppointmentHistory
        {
            AppointmentId = appointmentId,
            PreviousStatus = prevStatus,
            NewStatus = AppointmentStatus.Approved,
            Reason = "Reschedule request approved by doctor",
            Appointment = appointment
        }, ct);

        await _uow.SaveChangesAsync(ct);

        // Notify Patient
        try
        {
            if (appointment.PatientId.HasValue)
            {
                var allPatients = await _patientRepo.GetAllAsync(ct);
                var patient = allPatients.FirstOrDefault(p => p.Id == appointment.PatientId.Value);
                if (patient != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        patient.UserId,
                        "✅ Reschedule Approved",
                        $"Your reschedule request for appointment {appointment.BookingCode} has been approved. New time: {newSlot.SlotDate:MM/dd/yyyy} at {newSlot.StartTime:HH:mm}.",
                        NotificationType.Appointment,
                        appointmentId,
                        "Appointment",
                        ct);
                }
            }
        }
        catch { }

        return ApiResponse.SuccessResponse("Reschedule request approved successfully.");
    }

    public async Task<ApiResponse> RejectRescheduleAsync(Guid appointmentId, Guid doctorUserId, RejectAppointmentDto? dto = null, CancellationToken ct = default)
    {
        var appointment = await _apptRepo.GetByIdAsync(appointmentId, ct);
        if (appointment == null)
            return ApiResponse.ErrorResponse("Appointment not found");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || appointment.DoctorId != doctor.Id)
            return ApiResponse.ErrorResponse("You are not authorized to reject this reschedule request.");

        if (appointment.Status != AppointmentStatus.RescheduleRequested)
            return ApiResponse.ErrorResponse("No pending reschedule request found for this appointment.");

        var prevStatus = appointment.Status;
        appointment.ProposedSlotId = null;
        appointment.Status = AppointmentStatus.Approved;
        appointment.UpdatedAt = DateTime.UtcNow;

        _apptRepo.Update(appointment);
        await _historyRepo.AddAsync(new AppointmentHistory
        {
            AppointmentId = appointmentId,
            PreviousStatus = prevStatus,
            NewStatus = AppointmentStatus.Approved,
            Reason = dto?.Reason ?? "Reschedule request declined by doctor",
            Appointment = appointment
        }, ct);

        await _uow.SaveChangesAsync(ct);

        // Notify Patient
        try
        {
            if (appointment.PatientId.HasValue)
            {
                var allPatients = await _patientRepo.GetAllAsync(ct);
                var patient = allPatients.FirstOrDefault(p => p.Id == appointment.PatientId.Value);
                if (patient != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        patient.UserId,
                        "❌ Reschedule Request Declined",
                        $"Your reschedule request for appointment {appointment.BookingCode} was declined. Your original appointment time remains unchanged.",
                        NotificationType.Appointment,
                        appointmentId,
                        "Appointment",
                        ct);
                }
            }
        }
        catch { }

        return ApiResponse.SuccessResponse("Reschedule request declined. Original schedule retained.");
    }


    public async Task<ApiResponse<List<AppointmentListItemDto>>> GetDoctorAppointmentsAsync(
        Guid doctorUserId,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? search = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? view = null,
        CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId || d.Id == doctorUserId);
        if (doctor == null)
            return ApiResponse<List<AppointmentListItemDto>>.ErrorResponse("Doctor profile not found");

        var allAppts = await _apptRepo.GetAllAsync(ct);
        var doctorAppts = allAppts.Where(a => (a.DoctorId == doctor.Id || a.DoctorId == doctor.UserId || a.DoctorId == doctorUserId) && !a.IsDeleted).ToList();

        if (!string.IsNullOrEmpty(view))
        {
            if (view.Equals("active", StringComparison.OrdinalIgnoreCase))
            {
                doctorAppts = doctorAppts.Where(a => AppointmentStatusHelper.IsActive(a.Status)).ToList();
            }
            else if (view.Equals("history", StringComparison.OrdinalIgnoreCase))
            {
                doctorAppts = doctorAppts.Where(a => AppointmentStatusHelper.IsHistory(a.Status)).ToList();
            }
        }
        else if (string.IsNullOrEmpty(status))
        {
            doctorAppts = doctorAppts.Where(a => AppointmentStatusHelper.IsActive(a.Status)).ToList();
        }

        // 1. Status Filter
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            {
                doctorAppts = doctorAppts.Where(a => a.Status == statusEnum).ToList();
            }
            else if (int.TryParse(status, out var statusVal) && Enum.IsDefined(typeof(AppointmentStatus), statusVal))
            {
                doctorAppts = doctorAppts.Where(a => a.Status == (AppointmentStatus)statusVal).ToList();
            }
        }

        // 2. Search Filter (by patient full name, notes, guest name, or booking code)
        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            var allPatients = await _patientRepo.GetAllAsync(ct);
            var allUsers = await _userRepo.GetAllAsync(ct);
            var patientUsers = allPatients.Join(allUsers, p => p.UserId, u => u.Id, (p, u) => new { PatientId = p.Id, FullName = u.FullName }).ToList();

            doctorAppts = doctorAppts.Where(a =>
                (a.BookingCode != null && a.BookingCode.ToLower().Contains(searchLower)) ||
                (a.Notes != null && a.Notes.ToLower().Contains(searchLower)) ||
                (a.GuestName != null && a.GuestName.ToLower().Contains(searchLower)) ||
                (a.PatientId.HasValue && patientUsers.Any(p => p.PatientId == a.PatientId && p.FullName.ToLower().Contains(searchLower)))
            ).ToList();
        }

        // 3. Date Range Filter (by SlotDate)
        if (fromDate.HasValue || toDate.HasValue)
        {
            var allSlots = await _slotRepo.GetAllAsync(ct);
            var slotDict = allSlots.GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.First());

            if (fromDate.HasValue)
            {
                var fromDateOnly = DateOnly.FromDateTime(fromDate.Value);
                doctorAppts = doctorAppts.Where(a => slotDict.TryGetValue(a.AppointmentSlotId, out var slot) && slot.SlotDate >= fromDateOnly).ToList();
            }
            if (toDate.HasValue)
            {
                var toDateOnly = DateOnly.FromDateTime(toDate.Value);
                doctorAppts = doctorAppts.Where(a => slotDict.TryGetValue(a.AppointmentSlotId, out var slot) && slot.SlotDate <= toDateOnly).ToList();
            }
        }

        var total = doctorAppts.Count;

        // Sort by CreatedAt descending to put the latest booked appointments first
        var items = doctorAppts.OrderByDescending(a => a.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = _mapper.Map<List<AppointmentListItemDto>>(items);
        await EnrichAppointmentListDtosAsync(dtos, items, ct);

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

        // Notify patient about approval + send email
        try
        {
            if (appointment.PatientId.HasValue)
            {
                var allUsers = await _userRepo.GetAllAsync(ct);
                var allPatients = await _patientRepo.GetAllAsync(ct);
                var pat = allPatients.FirstOrDefault(p => p.Id == appointment.PatientId.Value);
                var doctorUser = allUsers.FirstOrDefault(u => u.Id == doctorUserId);
                var slot = await _slotRepo.GetByIdAsync(appointment.AppointmentSlotId, ct);
                if (pat != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        pat.UserId,
                        "✅ Appointment Confirmed",
                        $"Your appointment on {slot?.SlotDate:MM/dd/yyyy} at {slot?.StartTime.ToString("HH':'mm")} has been confirmed by Dr. {doctorUser?.FullName ?? "your doctor"}.",
                        NotificationType.Appointment,
                        appointmentId,
                        "Appointment",
                        ct);

                    // Send confirmation email
                    var patUser = allUsers.FirstOrDefault(u => u.Id == pat.UserId);
                    if (patUser?.Email != null)
                    {
                        await _emailService.SendAppointmentConfirmedEmailAsync(
                            patUser.Email,
                            patUser.FullName ?? "Patient",
                            doctorUser?.FullName ?? "your doctor",
                            slot?.SlotDate.ToString("MM/dd/yyyy") ?? "",
                            slot?.StartTime.ToString("HH:mm") ?? "",
                            ct);
                    }
                }
            }
        }
        catch { }

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

        if (appointment.TreatmentPackageId.HasValue && appointment.TreatmentPackageId.Value != Guid.Empty)
        {
            var package = await _packageRepo.GetByIdAsync(appointment.TreatmentPackageId.Value, ct);
            if (package != null)
            {
                package.RemainingSessions++;
                if (package.RemainingSessions > package.SessionQuantity)
                    package.RemainingSessions = package.SessionQuantity;
                _packageRepo.Update(package);
            }
        }

        appointment.Status = AppointmentStatus.Rejected;
        appointment.RejectionReason = dto.Reason;
        appointment.UpdatedAt = DateTime.UtcNow;
        _apptRepo.Update(appointment);

        // Release the slot
        var slot = await _slotRepo.GetByIdAsync(appointment.AppointmentSlotId, ct);
        if (slot != null)
        {
            slot.CurrentBookings = Math.Max(0, slot.CurrentBookings - 1);
            if (slot.CurrentBookings < slot.MaxPatients)
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

        // Notify patient about rejection
        try
        {
            if (appointment.PatientId.HasValue)
            {
                var allUsers = await _userRepo.GetAllAsync(ct);
                var allPatients = await _patientRepo.GetAllAsync(ct);
                var pat = allPatients.FirstOrDefault(p => p.Id == appointment.PatientId.Value);
                var doctorUser = allUsers.FirstOrDefault(u => u.Id == doctorUserId);
                var slot2 = await _slotRepo.GetByIdAsync(appointment.AppointmentSlotId, ct);
                if (pat != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        pat.UserId,
                        "❌ Appointment Rejected",
                        $"Your appointment on {slot2?.SlotDate:MM/dd/yyyy} has been rejected by Dr. {doctorUser?.FullName ?? "your doctor"}. Reason: {dto.Reason ?? "No reason provided"}.",
                        NotificationType.Appointment,
                        appointmentId,
                        "Appointment",
                        ct);
                }
            }
        }
        catch { }

        return ApiResponse.SuccessResponse("Appointment rejected");
    }

    public async Task<ApiResponse> StartAppointmentAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct = default)
    {
        var appointment = await _apptRepo.GetByIdAsync(appointmentId, ct);
        if (appointment == null)
            return ApiResponse.ErrorResponse("Appointment not found");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || appointment.DoctorId != doctor.Id)
            return ApiResponse.ErrorResponse("Not authorized to start this appointment");

        if (appointment.Status != AppointmentStatus.Approved)
            return ApiResponse.ErrorResponse("Only approved appointments can be started");

        var prevStatus = appointment.Status;
        appointment.Status = AppointmentStatus.InProgress;
        appointment.UpdatedAt = DateTime.UtcNow;
        _apptRepo.Update(appointment);

        await _historyRepo.AddAsync(new AppointmentHistory
        {
            AppointmentId = appointmentId,
            PreviousStatus = prevStatus,
            NewStatus = AppointmentStatus.InProgress,
            Reason = "Appointment started by doctor",
            Appointment = appointment
        }, ct);

        await _uow.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Appointment started successfully");
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

        // Check if consultation note exists before allowing completion
        var allNotes = await _consultationNoteRepo.GetAllAsync(ct);
        var hasNote = allNotes.Any(n => n.AppointmentId == appointmentId && !n.IsDeleted);
        if (!hasNote)
            return ApiResponse.ErrorResponse("Please create a consultation note before completing this appointment.");

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

        // Notify patient about completion + send email
        try
        {
            if (appointment.PatientId.HasValue)
            {
                var allPatients = await _patientRepo.GetAllAsync(ct);
                var pat = allPatients.FirstOrDefault(p => p.Id == appointment.PatientId.Value);
                var allUsers = await _userRepo.GetAllAsync(ct);
                var doctorUser = allUsers.FirstOrDefault(u => u.Id == doctorUserId);
                if (pat != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        pat.UserId,
                        "🎉 Consultation Completed",
                        $"Your consultation with Dr. {doctorUser?.FullName ?? "your doctor"} has been completed. Please check your consultation records.",
                        NotificationType.Appointment,
                        appointmentId,
                        "Appointment",
                        ct);

                    // Send completion email
                    var patUser = allUsers.FirstOrDefault(u => u.Id == pat.UserId);
                    if (patUser?.Email != null)
                    {
                        await _emailService.SendAppointmentCompletedEmailAsync(
                            patUser.Email,
                            patUser.FullName ?? "Patient",
                            doctorUser?.FullName ?? "your doctor",
                            ct);
                    }
                }
            }
        }
        catch { }

        return ApiResponse.SuccessResponse("Appointment completed");
    }

    public async Task<ApiResponse<int>> GetVisitCountAsync(Guid patientUserId, Guid doctorProfileId, CancellationToken ct = default)
    {
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        if (patient == null)
            return ApiResponse<int>.SuccessResponse(0);

        var allAppts = await _apptRepo.GetAllAsync(ct);
        var count = allAppts.Count(a =>
            a.PatientId == patient.Id &&
            a.DoctorId == doctorProfileId &&
            a.Status == AppointmentStatus.Completed &&
            !a.IsDeleted);

        return ApiResponse<int>.SuccessResponse(count);
    }

    public async Task<ApiResponse<bool>> IsReturningPatientAsync(Guid patientUserId, Guid doctorProfileId, CancellationToken ct = default)
    {
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        if (patient == null)
            return ApiResponse<bool>.SuccessResponse(false);

        // Resolve doctorProfileId: may be DoctorProfile.Id (PK) or User.Id (from enriched DTOs)
        var resolvedDoctorProfileId = doctorProfileId;
        var doctorById = await _doctorRepo.GetByIdAsync(doctorProfileId, ct);
        if (doctorById == null)
        {
            var allDoctors = await _doctorRepo.GetAllAsync(ct);
            var doctorByUserId = allDoctors.FirstOrDefault(d => d.UserId == doctorProfileId);
            if (doctorByUserId != null)
                resolvedDoctorProfileId = doctorByUserId.Id;
        }

        var allAppts = await _apptRepo.GetAllAsync(ct);
        var isReturning = allAppts.Any(a =>
            a.PatientId == patient.Id &&
            a.DoctorId == resolvedDoctorProfileId &&
            !a.IsDeleted);

        return ApiResponse<bool>.SuccessResponse(isReturning);
    }

    public async Task<ApiResponse<AppointmentClinicalContextDto>> GetClinicalContextAsync(Guid appointmentId, Guid requestingUserId, CancellationToken ct = default)
    {
        var appt = await _apptRepo.GetByIdAsync(appointmentId, ct);
        if (appt == null)
            return ApiResponse<AppointmentClinicalContextDto>.ErrorResponse("Appointment not found");

        var allUsers = await _userRepo.GetAllAsync(ct);
        var requestingUser = allUsers.FirstOrDefault(u => u.Id == requestingUserId);

        // Authorization check: doctor assigned to appt, patient assigned to appt, or staff/admin
        var isAssignedDoctor = false;
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.Id == appt.DoctorId || d.UserId == appt.DoctorId);
        if (doctor != null && (doctor.UserId == requestingUserId || doctor.Id == requestingUserId))
        {
            isAssignedDoctor = true;
        }

        var isPatient = false;
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = appt.PatientId.HasValue ? allPatients.FirstOrDefault(p => p.Id == appt.PatientId.Value || p.UserId == appt.PatientId.Value) : null;
        if (patient == null && appt.PatientId.HasValue)
        {
            patient = allPatients.FirstOrDefault(p => p.UserId == appt.PatientId.Value || p.Id == appt.PatientId.Value);
        }

        if (patient != null && patient.UserId == requestingUserId)
        {
            isPatient = true;
        }

        var isStaffOrAdmin = requestingUser != null && (requestingUser.Role?.Name == RoleConstants.SystemAdmin || requestingUser.Role?.Name == RoleConstants.CustomerSupport || requestingUser.Role?.Name == RoleConstants.BusinessManager);

        if (!isAssignedDoctor && !isPatient && !isStaffOrAdmin)
        {
            return ApiResponse<AppointmentClinicalContextDto>.ErrorResponse("Unauthorized access to patient clinical context.");
        }

        var result = new AppointmentClinicalContextDto();

        // 1. Fetch recent consultation records (excluding current appointment's note)
        if (patient != null)
        {
            var allNotes = await _consultationNoteRepo.GetAllAsync(ct);
            var allPatientRecords = _patientRecordRepo != null ? await _patientRecordRepo.GetAllAsync(ct) : new List<PatientRecord>();
            var pRecord = allPatientRecords.FirstOrDefault(pr => pr.PatientId == patient.Id);
            var pRecordId = pRecord?.Id;

            var patientNotes = allNotes.Where(n => !n.IsDeleted &&
                ((pRecordId.HasValue && n.PatientRecordId == pRecordId.Value) ||
                 (n.Appointment != null && n.Appointment.PatientId == patient.Id) ||
                 (n.AppointmentId.HasValue && n.AppointmentId != appointmentId)) &&
                n.AppointmentId != appointmentId)
                .OrderByDescending(n => n.ConsultationDate ?? n.CreatedAt)
                .Take(3)
                .ToList();

            var docUsers = allDoctors.Join(allUsers, d => d.UserId, u => u.Id, (d, u) => new { DoctorProfileId = d.Id, FullName = u.FullName }).ToList();

            result.RecentConsultations = patientNotes.Select(n => new RecentConsultationDto
            {
                Id = n.Id,
                AppointmentId = n.AppointmentId,
                ConsultationDate = n.ConsultationDate ?? n.CreatedAt,
                DoctorName = docUsers.FirstOrDefault(d => d.DoctorProfileId == n.DoctorId)?.FullName ?? "Doctor",
                Diagnosis = n.Diagnosis,
                ConsultationSummary = n.ConsultationSummary,
                Recommendation = n.Recommendation,
                TherapyPlan = n.TherapyPlan
            }).ToList();
        }

        // 2. Fetch Psychometric Assessments (Current Appointment + Top 3 recent history)
        if (_psychRepo != null && patient != null)
        {
            var allSubmissions = await _psychRepo.GetAllAsync(ct);
            var patientSubs = allSubmissions.Where(s => !s.IsDeleted && (s.PatientId == patient.Id || (s.Patient != null && s.Patient.UserId == patient.UserId))).ToList();

            var currentSub = patientSubs.FirstOrDefault(s => s.AppointmentId == appointmentId);
            if (currentSub != null)
            {
                result.CurrentAssessment = new RecentAssessmentResultDto
                {
                    Id = currentSub.Id,
                    AppointmentId = currentSub.AppointmentId,
                    TestTitle = currentSub.Test?.Title ?? "Psychometric Screening",
                    TestType = currentSub.Test?.TestType,
                    SubmittedAt = currentSub.CreatedAt,
                    TotalScore = currentSub.TotalScore,
                    Interpretation = currentSub.Interpretation,
                    ScoreDataJson = currentSub.ScoreDataJson
                };
            }

            var recentSubs = patientSubs
                .Where(s => currentSub == null || s.Id != currentSub.Id)
                .OrderByDescending(s => s.CreatedAt)
                .Take(3)
                .Select(s => new RecentAssessmentResultDto
                {
                    Id = s.Id,
                    AppointmentId = s.AppointmentId,
                    TestTitle = s.Test?.Title ?? "Psychometric Screening",
                    TestType = s.Test?.TestType,
                    SubmittedAt = s.CreatedAt,
                    TotalScore = s.TotalScore,
                    Interpretation = s.Interpretation,
                    ScoreDataJson = s.ScoreDataJson
                })
                .ToList();

            result.RecentAssessments = recentSubs;
        }

        // 3. Treatment Case Progress Context
        if (_treatmentCaseRepo != null && patient != null)
        {
            var allCases = await _treatmentCaseRepo.GetAllAsync(ct);
            TreatmentCase? tCase = null;

            if (appt.TreatmentCaseId.HasValue)
            {
                tCase = allCases.FirstOrDefault(c => c.Id == appt.TreatmentCaseId.Value && !c.IsDeleted);
            }

            if (tCase == null)
            {
                tCase = allCases.Where(c => !c.IsDeleted && (c.PatientId == patient.Id || c.PatientId == patient.UserId) && c.Status == TreatmentCaseStatus.Active)
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefault();
            }

            if (tCase != null)
            {
                var goals = tCase.Goals != null ? tCase.Goals.Where(g => !g.IsDeleted).ToList() : new List<TreatmentGoal>();
                var sessions = tCase.Sessions != null ? tCase.Sessions.Where(s => !s.IsDeleted).ToList() : new List<TreatmentSession>();
                var assignments = tCase.Assignments != null ? tCase.Assignments.Where(a => !a.IsDeleted).ToList() : new List<TherapyAssignment>();
                var moods = tCase.MoodEntries != null ? tCase.MoodEntries.Where(m => !m.IsDeleted).OrderByDescending(m => m.RecordedAt).ToList() : new List<MoodEntry>();

                var currentSession = sessions.FirstOrDefault(s => s.AppointmentId == appointmentId);
                var nextSession = sessions.Where(s => s.Status == TreatmentSessionStatus.Scheduled && s.PlannedStartTime > DateTime.Now).OrderBy(s => s.PlannedStartTime).FirstOrDefault();

                var latestMood = moods.FirstOrDefault();
                string? moodSummary = latestMood != null
                    ? $"Overall Mood: {latestMood.MoodScore}/10{(latestMood.AnxietyScore.HasValue ? $", Anxiety: {latestMood.AnxietyScore}/10" : "")}{(latestMood.StressScore.HasValue ? $", Stress: {latestMood.StressScore}/10" : "")}"
                    : null;

                var activeGoals = goals.Where(g => g.Status != GoalStatus.Achieved && g.Status != GoalStatus.Cancelled)
                    .OrderByDescending(g => g.Priority)
                    .ThenByDescending(g => g.UpdatedAt)
                    .Take(3)
                    .Select(g => new TreatmentGoalContextDto
                    {
                        Id = g.Id,
                        Title = g.Title,
                        Category = g.Category.ToString(),
                        ProgressPercent = g.ProgressPercent,
                        CurrentValue = g.CurrentValue,
                        TargetValue = g.TargetValue,
                        Unit = g.Unit,
                        Status = g.Status.ToString(),
                        UpdatedAt = g.UpdatedAt ?? g.CreatedAt
                    })
                    .ToList();

                var recentProgress = goals.SelectMany(g => g.ProgressHistory ?? new List<TreatmentGoalProgress>())
                    .Where(p => !p.IsDeleted)
                    .OrderByDescending(p => p.RecordedAt)
                    .Take(3)
                    .Select(p => new TreatmentGoalProgressContextDto
                    {
                        Id = p.Id,
                        GoalTitle = p.Goal?.Title ?? "Goal Progress",
                        SessionNumber = p.TreatmentSession?.SessionNumber,
                        ProgressPercent = p.ProgressPercent,
                        DoctorComment = p.DoctorComment,
                        RecordedDate = p.RecordedAt
                    })
                    .ToList();

                result.TreatmentCaseContext = new AppointmentTreatmentCaseContextDto
                {
                    TreatmentCaseId = tCase.Id,
                    CaseName = tCase.CaseName,
                    Status = tCase.Status.ToString(),
                    CompletedSessions = tCase.CompletedSessions,
                    TotalSessions = tCase.TotalSessions,
                    CurrentSessionNumber = currentSession?.SessionNumber ?? (tCase.CompletedSessions + 1),
                    NextPlannedSessionDate = nextSession?.PlannedStartTime,
                    OverallProgressPercent = tCase.TotalSessions > 0 ? Math.Round((double)tCase.CompletedSessions / tCase.TotalSessions * 100, 1) : tCase.OverallProgressPercent,
                    GoalsAchieved = goals.Count(g => g.Status == GoalStatus.Achieved),
                    TotalGoals = goals.Count,
                    HomeworkCompleted = assignments.Count(a => a.Status == HomeworkStatus.Submitted || a.Status == HomeworkStatus.Reviewed),
                    HomeworkAssigned = assignments.Count,
                    LatestMoodSummary = moodSummary,
                    ActiveGoals = activeGoals,
                    RecentGoalProgressHistory = recentProgress
                };
            }
        }

        return ApiResponse<AppointmentClinicalContextDto>.SuccessResponse(result);
    }

    public Task<ApiResponse<List<AppointmentListItemDto>>> GetMyAppointmentsAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<List<AppointmentListItemDto>>> GetDoctorAppointmentsAsync(Guid doctorUserId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}


public class ScheduleService : IScheduleService
{
    private readonly IRepository<Schedule> _scheduleRepo;
    private readonly IRepository<AppointmentSlot> _slotRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<DoctorDayOff> _dayOffRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ScheduleService(
        IRepository<Schedule> scheduleRepo,
        IRepository<AppointmentSlot> slotRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<User> userRepo,
        IRepository<DoctorDayOff> dayOffRepo,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _scheduleRepo = scheduleRepo;
        _slotRepo = slotRepo;
        _doctorRepo = doctorRepo;
        _userRepo = userRepo;
        _dayOffRepo = dayOffRepo;
        _uow = uow;
        _mapper = mapper;
    }

    /// <summary>
    /// Maps DayOfWeekEnum flags to System.DayOfWeek for date iteration
    /// </summary>
    private static DayOfWeekEnum ToDayFlag(DayOfWeek dow) => dow switch
    {
        DayOfWeek.Monday => DayOfWeekEnum.Monday,
        DayOfWeek.Tuesday => DayOfWeekEnum.Tuesday,
        DayOfWeek.Wednesday => DayOfWeekEnum.Wednesday,
        DayOfWeek.Thursday => DayOfWeekEnum.Thursday,
        DayOfWeek.Friday => DayOfWeekEnum.Friday,
        DayOfWeek.Saturday => DayOfWeekEnum.Saturday,
        DayOfWeek.Sunday => DayOfWeekEnum.Sunday,
        _ => 0
    };

    /// <summary>
    /// Auto-generate AppointmentSlot records from a Schedule config for the next N weeks.
    /// Skips past dates, skips days-off, skips slots that already exist.
    /// </summary>
    private async Task GenerateSlotsFromScheduleAsync(Schedule schedule, DoctorProfile doctor, int weeksAhead = 4, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = today.AddDays(weeksAhead * 7);

        // Load existing slots to avoid duplicates, and delete future available slots to sync with new schedule
        var allSlots = await _slotRepo.GetAllAsync(ct);

        // Remove existing available slots from today onwards to regenerate them
        var futureAvailableSlots = allSlots
            .Where(s => s.DoctorProfileId == doctor.Id && s.SlotDate >= today && s.Status == AppointmentSlotStatus.Available)
            .ToList();

        if (futureAvailableSlots.Count > 0)
        {
            _slotRepo.DeleteRange(futureAvailableSlots);
        }

        // Only booked/blocked slots remain as existing
        var remainingSlots = allSlots
            .Where(s => s.DoctorProfileId == doctor.Id && s.SlotDate >= today && s.Status != AppointmentSlotStatus.Available)
            .ToHashSet();
        var existingKeys = new HashSet<string>(remainingSlots.Select(s => $"{s.SlotDate}|{s.StartTime}"));

        // Load days off
        var allDaysOff = await _dayOffRepo.GetAllAsync(ct);
        var daysOff = allDaysOff.Where(d => d.DoctorProfileId == doctor.Id).ToList();

        var newSlots = new List<AppointmentSlot>();
        var slotDurationMinutes = (int)schedule.SlotDuration;

        for (var date = today; date <= endDate; date = date.AddDays(1))
        {
            // Check if this day of week is in the schedule's working days
            var dayFlag = ToDayFlag(date.DayOfWeek);
            if (!schedule.WorkingDays.HasFlag(dayFlag))
                continue;

            // Check if this date is a day off
            var dateTime = date.ToDateTime(TimeOnly.MinValue);
            if (daysOff.Any(d => dateTime >= d.StartDate && dateTime <= d.EndDate))
                continue;

            // Generate slots for this day
            var currentTime = schedule.StartTime;
            while (currentTime.AddMinutes(slotDurationMinutes) <= schedule.EndTime)
            {
                var slotEnd = currentTime.AddMinutes(slotDurationMinutes);
                var key = $"{date}|{currentTime}";

                if (!existingKeys.Contains(key))
                {
                    newSlots.Add(new AppointmentSlot
                    {
                        DoctorProfileId = doctor.Id,
                        SlotDate = date,
                        StartTime = currentTime,
                        EndTime = slotEnd,
                        Status = AppointmentSlotStatus.Available,
                        DoctorProfile = doctor
                    });
                    existingKeys.Add(key);
                }
                currentTime = slotEnd;
            }
        }

        if (newSlots.Count > 0)
        {
            await _slotRepo.AddRangeAsync(newSlots, ct);
            await _uow.SaveChangesAsync(ct);
        }
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

        // Auto-generate appointment slots for the next N weeks
        var weeks = dto.WeeksAhead ?? 4;
        await GenerateSlotsFromScheduleAsync(schedule, doctor, weeks, ct);

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

        // Re-generate slots for the updated schedule
        var weeks = dto.WeeksAhead ?? 4;
        await GenerateSlotsFromScheduleAsync(schedule, doctor, weeks, ct);

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
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.Id == doctorProfileId || d.UserId == doctorProfileId);
        if (doctor == null)
            return ApiResponse<AvailableSlotsDto>.ErrorResponse("Doctor not found");

        var docId = doctor.Id;

        // Get user name
        var allUsers = await _userRepo.GetAllAsync(ct);
        var user = allUsers.FirstOrDefault(u => u.Id == doctor.UserId);

        var allSlots = await _slotRepo.GetAllAsync(ct);
        // Return Available + Booked slots so calendar shows booked as locked; exclude Blocked
        var doctorSlots = allSlots
            .Where(s => s.DoctorProfileId == docId && s.Status != AppointmentSlotStatus.Blocked)
            .ToList();

        if (date.HasValue)
            doctorSlots = doctorSlots.Where(s => s.SlotDate == date.Value).ToList();

        var slotDtos = doctorSlots.Select(s => new AppointmentSlotDto
        {
            Id = s.Id,
            Date = s.SlotDate.ToString("yyyy-MM-dd"),
            StartTime = s.StartTime.ToString("HH:mm"),
            EndTime = s.EndTime.ToString("HH:mm"),
            Status = s.Status,
            Price = s.Price,
            Notes = s.Notes,
            MaxPatients = s.MaxPatients,
            CurrentBookings = s.CurrentBookings
        }).ToList();

        var result = new AvailableSlotsDto
        {
            DoctorId = doctorProfileId,
            DoctorName = user?.FullName ?? "Unknown",
            Slots = slotDtos
        };

        return ApiResponse<AvailableSlotsDto>.SuccessResponse(result);
    }

    public async Task<ApiResponse<AvailableSlotsDto>> GetDoctorAllSlotsAsync(Guid doctorUserId, DateOnly? date, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId || d.Id == doctorUserId);
        if (doctor == null)
            return ApiResponse<AvailableSlotsDto>.ErrorResponse("Doctor not found");

        var allUsers = await _userRepo.GetAllAsync(ct);
        var user = allUsers.FirstOrDefault(u => u.Id == doctor.UserId);

        var allSlots = await _slotRepo.GetAllAsync(ct);
        // Include all slot statuses (Available, Booked, Blocked, etc.)
        var doctorSlots = allSlots
            .Where(s => (s.DoctorProfileId == doctor.Id || s.DoctorProfileId == doctor.UserId || s.DoctorProfileId == doctorUserId) && !s.IsDeleted)
            .ToList();

        if (date.HasValue)
            doctorSlots = doctorSlots.Where(s => s.SlotDate == date.Value).ToList();

        var slotDtos = doctorSlots.Select(s => new AppointmentSlotDto
        {
            Id = s.Id,
            Date = s.SlotDate.ToString("yyyy-MM-dd"),
            StartTime = s.StartTime.ToString("HH:mm"),
            EndTime = s.EndTime.ToString("HH:mm"),
            Status = s.Status,
            Price = s.Price,
            Notes = s.Notes,
            MaxPatients = s.MaxPatients,
            CurrentBookings = s.CurrentBookings
        }).ToList();

        var result = new AvailableSlotsDto
        {
            DoctorId = doctor.Id,
            DoctorName = user?.FullName ?? "Unknown",
            Slots = slotDtos
        };

        return ApiResponse<AvailableSlotsDto>.SuccessResponse(result);
    }

    public async Task<ApiResponse> ToggleBlockSlotAsync(Guid slotId, Guid doctorUserId, CancellationToken ct = default)
    {
        var slot = await _slotRepo.GetByIdAsync(slotId, ct);
        if (slot == null)
            return ApiResponse.ErrorResponse("Slot không tồn tại");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || slot.DoctorProfileId != doctor.Id)
            return ApiResponse.ErrorResponse("Không có quyền thực hiện hành động này");

        if (slot.Status == AppointmentSlotStatus.Booked)
            return ApiResponse.ErrorResponse("Không thể khóa slot đã được bệnh nhân đặt lịch");

        if (slot.Status == AppointmentSlotStatus.Available)
        {
            slot.Status = AppointmentSlotStatus.Blocked;
        }
        else if (slot.Status == AppointmentSlotStatus.Blocked)
        {
            slot.Status = AppointmentSlotStatus.Available;
        }
        else
        {
            return ApiResponse.ErrorResponse("Trạng thái slot hiện tại không cho phép thay đổi");
        }

        slot.UpdatedAt = DateTime.UtcNow;
        _slotRepo.Update(slot);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Cập nhật trạng thái slot thành công");
    }

    public async Task<ApiResponse> AddDayOffAsync(Guid doctorUserId, CreateDayOffDto dto, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null)
            return ApiResponse.ErrorResponse("Doctor not found");

        if (dto.StartDate.Date < DateTime.Today)
            return ApiResponse.ErrorResponse("Không thể chọn ngày nghỉ trong quá khứ");

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

    public async Task<ApiResponse<AppointmentSlotDto>> CreateSlotAsync(Guid doctorUserId, CreateSlotDto dto, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null)
            return ApiResponse<AppointmentSlotDto>.ErrorResponse("Doctor not found");

        if (!DateOnly.TryParse(dto.Date, out var slotDate))
            return ApiResponse<AppointmentSlotDto>.ErrorResponse("Invalid date format");

        if (!TimeOnly.TryParse(dto.StartTime, out var startTime) || !TimeOnly.TryParse(dto.EndTime, out var endTime))
            return ApiResponse<AppointmentSlotDto>.ErrorResponse("Invalid time format");

        if (startTime >= endTime)
            return ApiResponse<AppointmentSlotDto>.ErrorResponse("Start time must be before end time");

        // Check for existing slot overlap
        var allSlots = await _slotRepo.GetAllAsync(ct);
        var existingSlots = allSlots.Where(s => s.DoctorProfileId == doctor.Id && s.SlotDate == slotDate).ToList();

        foreach (var existing in existingSlots)
        {
            if (startTime < existing.EndTime && endTime > existing.StartTime)
            {
                return ApiResponse<AppointmentSlotDto>.ErrorResponse("Time slot overlaps with existing slot");
            }
        }

        var slot = new AppointmentSlot
        {
            DoctorProfileId = doctor.Id,
            SlotDate = slotDate,
            StartTime = startTime,
            EndTime = endTime,
            Status = AppointmentSlotStatus.Available,
            Notes = dto.Notes,
            MaxPatients = dto.MaxPatients > 0 ? dto.MaxPatients : 1,
            CurrentBookings = 0,
            DoctorProfile = doctor
        };

        await _slotRepo.AddAsync(slot, ct);
        await _uow.SaveChangesAsync(ct);

        var returnDto = new AppointmentSlotDto
        {
            Id = slot.Id,
            Date = slot.SlotDate.ToString("yyyy-MM-dd"),
            StartTime = slot.StartTime.ToString("HH:mm"),
            EndTime = slot.EndTime.ToString("HH:mm"),
            Status = slot.Status,
            Price = slot.Price,
            Notes = slot.Notes,
            MaxPatients = slot.MaxPatients,
            CurrentBookings = slot.CurrentBookings
        };

        return ApiResponse<AppointmentSlotDto>.SuccessResponse(returnDto, "Slot created successfully");
    }

    public async Task<ApiResponse> DeleteSlotAsync(Guid slotId, Guid doctorUserId, CancellationToken ct = default)
    {
        // Use GetAllAsync instead of GetByIdAsync to respect HasQueryFilter (FindAsync ignores it)
        var allSlots = await _slotRepo.GetAllAsync(ct);
        var slot = allSlots.FirstOrDefault(s => s.Id == slotId);
        if (slot == null)
            return ApiResponse.ErrorResponse("Slot not found");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || slot.DoctorProfileId != doctor.Id)
            return ApiResponse.ErrorResponse("Not authorized");

        if (slot.Status == AppointmentSlotStatus.Booked || slot.Status == AppointmentSlotStatus.Completed)
            return ApiResponse.ErrorResponse("Cannot delete a booked or completed slot");

        // Soft delete via repo (HasQueryFilter prevents deleted slots from appearing in overlap checks)
        _slotRepo.Delete(slot);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Slot deleted successfully");
    }

    public async Task<ApiResponse> UpdateSlotNotesAsync(Guid slotId, Guid doctorUserId, string? notes, CancellationToken ct = default)
    {
        var allSlots = await _slotRepo.GetAllAsync(ct);
        var slot = allSlots.FirstOrDefault(s => s.Id == slotId);
        if (slot == null)
            return ApiResponse.ErrorResponse("Slot not found");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || slot.DoctorProfileId != doctor.Id)
            return ApiResponse.ErrorResponse("Not authorized");

        slot.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        _slotRepo.Update(slot);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Slot notes updated successfully");
    }

    public async Task<ApiResponse> UpdateSlotAsync(Guid slotId, Guid doctorUserId, UpdateSlotDto dto, CancellationToken ct = default)
    {
        var allSlots = await _slotRepo.GetAllAsync(ct);
        var slot = allSlots.FirstOrDefault(s => s.Id == slotId);
        if (slot == null)
            return ApiResponse.ErrorResponse("Slot not found");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || slot.DoctorProfileId != doctor.Id)
            return ApiResponse.ErrorResponse("Not authorized");

        // Cannot edit time of a fully booked slot
        if (slot.Status == AppointmentSlotStatus.Booked && (dto.StartTime != null || dto.EndTime != null))
            return ApiResponse.ErrorResponse("Cannot change time of a fully booked slot");

        // Update time if provided
        if (dto.StartTime != null && TimeOnly.TryParse(dto.StartTime, out var newStart))
            slot.StartTime = newStart;
        if (dto.EndTime != null && TimeOnly.TryParse(dto.EndTime, out var newEnd))
            slot.EndTime = newEnd;

        if (slot.StartTime >= slot.EndTime)
            return ApiResponse.ErrorResponse("Start time must be before end time");

        // Check overlap with other slots (excluding itself)
        var otherSlots = allSlots.Where(s => s.DoctorProfileId == doctor.Id && s.SlotDate == slot.SlotDate && s.Id != slotId).ToList();
        foreach (var existing in otherSlots)
        {
            if (slot.StartTime < existing.EndTime && slot.EndTime > existing.StartTime)
                return ApiResponse.ErrorResponse("Updated time overlaps with an existing slot");
        }

        // Update notes
        if (dto.Notes != null)
            slot.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

        // Update max patients
        if (dto.MaxPatients.HasValue)
        {
            if (dto.MaxPatients.Value < 1)
                return ApiResponse.ErrorResponse("Maximum patients must be at least 1");
            if (dto.MaxPatients.Value < slot.CurrentBookings)
                return ApiResponse.ErrorResponse($"Cannot set max patients below current bookings ({slot.CurrentBookings})");
            slot.MaxPatients = dto.MaxPatients.Value;
        }

        _slotRepo.Update(slot);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Slot updated successfully");
    }
}
