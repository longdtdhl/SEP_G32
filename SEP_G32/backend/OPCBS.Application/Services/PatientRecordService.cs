using AutoMapper;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Shared.Models;
using OPCBS.Domain.Constants;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using System.Net;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OPCBS.Application.Services;

public class PatientRecordService : IPatientRecordService
{
    private readonly IRepository<PatientRecord> _repo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<Appointment> _apptRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly IRepository<OtpVerification> _otpRepo;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public PatientRecordService(
        IRepository<PatientRecord> repo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<Appointment> apptRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<User> userRepo,
        IRepository<Role> roleRepo,
        IRepository<OtpVerification> otpRepo,
        IEmailService emailService,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _doctorRepo = doctorRepo;
        _apptRepo = apptRepo;
        _patientRepo = patientRepo;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _otpRepo = otpRepo;
        _emailService = emailService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    private async Task EnrichPatientRecordDtosAsync(List<PatientRecordDto> dtos, CancellationToken ct)
    {
        if (!dtos.Any()) return;
        var allPatients = (await _patientRepo.GetAllAsync(ct)).ToList();
        var allUsers = (await _userRepo.GetAllAsync(ct)).ToList();
        var usersById = allUsers.ToDictionary(user => user.Id);

        foreach (var dto in dtos)
        {
            if (!dto.PatientId.HasValue)
            {
                var matchedUser = allUsers.FirstOrDefault(u =>
                    (!string.IsNullOrWhiteSpace(dto.GuestEmail) && string.Equals(u.Email, dto.GuestEmail, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(dto.GuestPhone) && string.Equals(u.PhoneNumber, dto.GuestPhone, StringComparison.OrdinalIgnoreCase))
                );

                if (matchedUser != null)
                {
                    var matchedPatient = allPatients.FirstOrDefault(p => p.UserId == matchedUser.Id || p.Id == matchedUser.Id);
                    if (matchedPatient != null)
                    {
                        dto.PatientId = matchedPatient.UserId;
                    }
                }
            }

            if (!dto.PatientId.HasValue)
            {
                // Fallback extraction from GeneralNotes for legacy records
                if (dto.GuestDateOfBirth == null && !string.IsNullOrWhiteSpace(dto.GeneralNotes))
                {
                    var dobMatch = System.Text.RegularExpressions.Regex.Match(dto.GeneralNotes, @"Ngày sinh:\s*([0-9]{1,2}[\/\-][0-9]{1,2}[\/\-][0-9]{4})");
                    if (dobMatch.Success && DateTime.TryParseExact(dobMatch.Groups[1].Value, new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDob))
                    {
                        dto.GuestDateOfBirth = parsedDob;
                    }
                    else
                    {
                        var ageMatch = System.Text.RegularExpressions.Regex.Match(dto.GeneralNotes, @"Tuổi:\s*([0-9]{1,3})");
                        if (ageMatch.Success && int.TryParse(ageMatch.Groups[1].Value, out var parsedAge) && parsedAge > 0 && parsedAge < 130)
                        {
                            dto.GuestDateOfBirth = new DateTime(DateTime.UtcNow.Year - parsedAge, 1, 1);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(dto.GuestAddress) && !string.IsNullOrWhiteSpace(dto.GeneralNotes))
                {
                    var addrMatch = System.Text.RegularExpressions.Regex.Match(dto.GeneralNotes, @"Địa chỉ:\s*([^\|]+)");
                    if (addrMatch.Success)
                    {
                        dto.GuestAddress = addrMatch.Groups[1].Value.Trim();
                    }
                }

                if (string.IsNullOrWhiteSpace(dto.GuestGender) && !string.IsNullOrWhiteSpace(dto.GeneralNotes))
                {
                    var genderMatch = System.Text.RegularExpressions.Regex.Match(dto.GeneralNotes, @"Giới tính:\s*([^\|]+)");
                    if (genderMatch.Success)
                    {
                        dto.GuestGender = genderMatch.Groups[1].Value.Trim();
                    }
                }

                dto.DateOfBirth ??= dto.GuestDateOfBirth;
                dto.Gender ??= dto.GuestGender;
                dto.Address ??= dto.GuestAddress;
                continue;
            }

            var patient = allPatients.FirstOrDefault(p => p.Id == dto.PatientId.Value || p.UserId == dto.PatientId.Value);
            if (patient != null)
            {
                dto.PatientId = patient.UserId;
                dto.DateOfBirth = patient.DateOfBirth;
                dto.Gender = patient.Gender?.ToString();
                dto.Address = patient.Address;
                dto.EmergencyContactName = patient.EmergencyContactName;
                dto.EmergencyContactPhone = patient.EmergencyContactPhone;

                if (usersById.TryGetValue(patient.UserId, out var user))
                {
                    dto.DisplayName = user.FullName;
                    dto.DisplayPhone = user.PhoneNumber;
                    dto.DisplayEmail = user.Email;
                }
            }
        }
    }

    private static bool MatchesDoctor(Guid doctorId, DoctorProfile doctorProfile) =>
        doctorId == doctorProfile.Id || doctorId == doctorProfile.UserId;

    private async Task<List<PatientRecord>> EnsureAppointmentPatientsHaveRecordsAsync(
        IEnumerable<Appointment> appointments,
        DoctorProfile doctorProfile,
        List<PatientProfile> patientProfiles,
        List<PatientRecord> existingRecords,
        CancellationToken ct)
    {
        var createdAny = false;
        var activeAppointments = appointments.Where(a =>
            MatchesDoctor(a.DoctorId, doctorProfile) &&
            !a.IsDeleted &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Rejected);

        foreach (var appointment in activeAppointments)
        {
            var patient = appointment.PatientId.HasValue
                ? patientProfiles.FirstOrDefault(p => p.Id == appointment.PatientId.Value || p.UserId == appointment.PatientId.Value)
                : null;

            var alreadyTracked = patient != null
                ? existingRecords.Any(record =>
                    MatchesDoctor(record.DoctorId, doctorProfile) &&
                    !record.IsDeleted &&
                    record.PatientId.HasValue &&
                    (record.PatientId == patient.Id || record.PatientId == patient.UserId))
                : existingRecords.Any(record =>
                    MatchesDoctor(record.DoctorId, doctorProfile) &&
                    !record.IsDeleted &&
                    !record.PatientId.HasValue &&
                    ((!string.IsNullOrWhiteSpace(appointment.GuestEmail) &&
                      string.Equals(record.GuestEmail, appointment.GuestEmail, StringComparison.OrdinalIgnoreCase)) ||
                     (string.Equals(record.GuestName, appointment.GuestName, StringComparison.OrdinalIgnoreCase) &&
                      string.Equals(record.GuestPhone, appointment.GuestPhoneNumber, StringComparison.OrdinalIgnoreCase))));

            if (alreadyTracked)
                continue;

            var record = new PatientRecord
            {
                DoctorId = doctorProfile.Id,
                Doctor = doctorProfile,
                PatientId = patient?.Id,
                Patient = patient,
                GuestName = patient == null ? appointment.GuestName : null,
                GuestEmail = patient == null ? appointment.GuestEmail : null,
                GuestPhone = patient == null ? appointment.GuestPhoneNumber : null,
                PsychologicalHistory = appointment.MedicalHistory,
                CurrentSymptoms = appointment.Symptoms,
                GeneralNotes = appointment.Notes
            };

            await _repo.AddAsync(record, ct);
            existingRecords.Add(record);
            createdAny = true;
        }

        if (createdAny)
            await _unitOfWork.SaveChangesAsync(ct);

        return existingRecords;
    }

    public async Task<List<PatientRecordDto>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _repo.GetAllAsync(ct);
        var dtos = _mapper.Map<List<PatientRecordDto>>(entities.OrderByDescending(x => x.CreatedAt));
        await EnrichPatientRecordDtosAsync(dtos, ct);
        return dtos;
    }

    public async Task<List<PatientRecordDto>> GetMyPatientsAsync(Guid doctorUserId, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctorProfile = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctorProfile == null) return new List<PatientRecordDto>();

        var allAppts = await _apptRepo.GetAllAsync(ct);
        var allPatients = (await _patientRepo.GetAllAsync(ct)).ToList();

        // Backfill is idempotent and repairs appointments created before patient-record creation was introduced.
        // Records remain doctor-owned, so this does not expose another doctor's clinical notes.
        var allRecords = (await _repo.GetAllAsync(ct)).ToList();
        await EnsureAppointmentPatientsHaveRecordsAsync(allAppts, doctorProfile, allPatients, allRecords, ct);
        var myRecords = allRecords
            .Where(record => !record.IsDeleted && MatchesDoctor(record.DoctorId, doctorProfile))
            .OrderByDescending(record => record.CreatedAt)
            .ToList();

        var dtos = _mapper.Map<List<PatientRecordDto>>(myRecords);
        await EnrichPatientRecordDtosAsync(dtos, ct);
        return dtos;
    }

    public async Task<bool> CanDoctorAccessPatientAsync(Guid doctorUserId, Guid patientRecordId, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctorProfile = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctorProfile == null) return false;

        var record = await _repo.GetByIdAsync(patientRecordId, ct);
        if (record == null) return false;

        // Doctor created this record.
        if (MatchesDoctor(record.DoctorId, doctorProfile)) return true;

        // Doctor has appointment with this patient
        if (record.PatientId.HasValue)
        {
            var allAppts = await _apptRepo.GetAllAsync(ct);
            var allPatients = await _patientRepo.GetAllAsync(ct);
            var recordPatient = allPatients.FirstOrDefault(p =>
                p.Id == record.PatientId.Value || p.UserId == record.PatientId.Value);
            var recordIdentifiers = new HashSet<Guid> { record.PatientId.Value };
            if (recordPatient != null)
            {
                recordIdentifiers.Add(recordPatient.Id);
                recordIdentifiers.Add(recordPatient.UserId);
            }

            return allAppts.Any(a => a.PatientId.HasValue &&
                MatchesDoctor(a.DoctorId, doctorProfile) &&
                recordIdentifiers.Contains(a.PatientId.Value) &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.Rejected);
        }

        return false;
    }

    public async Task<List<PatientRecordDto>> GetDoctorPatientsAsync(Guid doctorUserId, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctorProfile = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        
        if (doctorProfile == null) return new List<PatientRecordDto>();

        var entities = await _repo.GetAllAsync(ct);
        entities = entities.Where(x => MatchesDoctor(x.DoctorId, doctorProfile)).ToList();
        var dtos = _mapper.Map<List<PatientRecordDto>>(entities.OrderByDescending(x => x.CreatedAt));
        await EnrichPatientRecordDtosAsync(dtos, ct);
        return dtos;
    }

    public async Task<List<PatientRecordDto>> GetSystemPatientsAsync(CancellationToken ct = default)
    {
        var entities = await _repo.GetAllAsync(ct);
        entities = entities.Where(x => x.PatientId != null).ToList();
        var dtos = _mapper.Map<List<PatientRecordDto>>(entities.OrderByDescending(x => x.CreatedAt));
        await EnrichPatientRecordDtosAsync(dtos, ct);
        return dtos;
    }

    public async Task<List<PatientRecordDto>> GetGuestPatientsAsync(CancellationToken ct = default)
    {
        var entities = await _repo.GetAllAsync(ct);
        entities = entities.Where(x => x.PatientId == null).ToList();
        var dtos = _mapper.Map<List<PatientRecordDto>>(entities.OrderByDescending(x => x.CreatedAt));
        await EnrichPatientRecordDtosAsync(dtos, ct);
        return dtos;
    }

    public async Task<PatientRecordDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return null;
        var dto = _mapper.Map<PatientRecordDto>(entity);
        var list = new List<PatientRecordDto> { dto };
        await EnrichPatientRecordDtosAsync(list, ct);
        return list[0];
    }

    public async Task<PatientRecordDto?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var entities = await _repo.GetAllAsync(ct);
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patientProfile = allPatients.FirstOrDefault(p => p.UserId == userId || p.Id == userId);
        var targetPatientProfileId = patientProfile?.Id ?? userId;

        var entity = entities.FirstOrDefault(x => x.PatientId == targetPatientProfileId);
        if (entity == null) return null;
        var dto = _mapper.Map<PatientRecordDto>(entity);
        var list = new List<PatientRecordDto> { dto };
        await EnrichPatientRecordDtosAsync(list, ct);
        return list[0];
    }

    private static string? FormatGeneralNotesWithGuestDemographics(string? rawNotes, DateTime? dob, string? gender, string? address)
    {
        var parts = new List<string>();
        if (dob.HasValue) parts.Add($"Ngày sinh: {dob.Value:dd/MM/yyyy}");
        if (!string.IsNullOrWhiteSpace(gender)) parts.Add($"Giới tính: {gender.Trim()}");
        if (!string.IsNullOrWhiteSpace(address)) parts.Add($"Địa chỉ: {address.Trim()}");

        if (!string.IsNullOrWhiteSpace(rawNotes))
        {
            var cleaned = rawNotes;
            if (dob.HasValue) cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"Ngày sinh:\s*[0-9]{1,2}[\/\-][0-9]{1,2}[\/\-][0-9]{4}\s*(\|)?", "").Trim();
            if (!string.IsNullOrWhiteSpace(gender)) cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"Giới tính:\s*[^\|]+\s*(\|)?", "").Trim();
            if (!string.IsNullOrWhiteSpace(address)) cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"Địa chỉ:\s*[^\|]+\s*(\|)?", "").Trim();
            cleaned = cleaned.Trim(' ', '|');
            if (!string.IsNullOrWhiteSpace(cleaned)) parts.Add(cleaned);
        }

        return parts.Count > 0 ? string.Join(" | ", parts) : null;
    }

    public async Task<ApiResponse> CreateAsync(Guid doctorUserId, CreatePatientRecordDto dto, CancellationToken ct = default)
    {
        try
        {
            var allDoctors = await _doctorRepo.GetAllAsync(ct);
            var doctorProfile = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
            
            if (doctorProfile == null)
            {
                return ApiResponse.ErrorResponse("Doctor profile not found");
            }

            if (dto.PatientId.HasValue && dto.PatientId.Value != Guid.Empty)
            {
                var allPatients = await _patientRepo.GetAllAsync(ct);
                var pat = allPatients.FirstOrDefault(p => p.Id == dto.PatientId.Value || p.UserId == dto.PatientId.Value);
                if (pat == null)
                {
                    return ApiResponse.ErrorResponse("Could not resolve or create patient record: Patient record not found");
                }
            }

            var mergedNotes = FormatGeneralNotesWithGuestDemographics(dto.GeneralNotes, dto.GuestDateOfBirth, dto.GuestGender, dto.GuestAddress);

            var entity = new PatientRecord
            {
                DoctorId = doctorProfile.Id,
                Doctor = doctorProfile,
                PatientId = dto.PatientId,
                GuestName = dto.GuestName,
                GuestPhone = dto.GuestPhone,
                GuestEmail = dto.GuestEmail,
                GuestDateOfBirth = dto.GuestDateOfBirth,
                GuestGender = dto.GuestGender,
                GuestAddress = dto.GuestAddress,
                PsychologicalHistory = dto.PsychologicalHistory,
                CurrentSymptoms = dto.CurrentSymptoms,
                StressFactors = dto.StressFactors,
                GeneralNotes = mergedNotes,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return ApiResponse.SuccessResponse("Patient record created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResponse($"Failed to create patient record: {ex.Message}");
        }
    }

    public async Task<ApiResponse> CreateBatchAsync(Guid doctorUserId, List<CreatePatientRecordDto> dtos, CancellationToken ct = default)
    {
        try
        {
            if (dtos == null || dtos.Count == 0)
            {
                return ApiResponse.ErrorResponse("No patient records provided for import.");
            }

            var allDoctors = await _doctorRepo.GetAllAsync(ct);
            var doctorProfile = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
            if (doctorProfile == null)
            {
                return ApiResponse.ErrorResponse("Doctor profile not found.");
            }

            var entities = new List<PatientRecord>();
            foreach (var dto in dtos)
            {
                if (string.IsNullOrWhiteSpace(dto.GuestName))
                    continue;

                var mergedNotes = FormatGeneralNotesWithGuestDemographics(dto.GeneralNotes, dto.GuestDateOfBirth, dto.GuestGender, dto.GuestAddress);

                var entity = new PatientRecord
                {
                    DoctorId = doctorProfile.Id,
                    Doctor = doctorProfile,
                    PatientId = dto.PatientId,
                    GuestName = dto.GuestName.Trim(),
                    GuestPhone = dto.GuestPhone?.Trim(),
                    GuestEmail = dto.GuestEmail?.Trim(),
                    GuestDateOfBirth = dto.GuestDateOfBirth,
                    GuestGender = dto.GuestGender?.Trim(),
                    GuestAddress = dto.GuestAddress?.Trim(),
                    PsychologicalHistory = dto.PsychologicalHistory?.Trim(),
                    CurrentSymptoms = dto.CurrentSymptoms?.Trim(),
                    StressFactors = dto.StressFactors?.Trim(),
                    GeneralNotes = mergedNotes?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };
                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                return ApiResponse.ErrorResponse("No valid patient records found in import list.");
            }

            await _repo.AddRangeAsync(entities, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return ApiResponse.SuccessResponse($"Successfully imported {entities.Count} patient record(s).");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResponse($"Failed to batch import patient records: {ex.Message}");
        }
    }

    public async Task<ApiResponse> UpdateAsync(Guid id, UpdatePatientRecordDto dto, CancellationToken ct = default)
    {
        try
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null)
            {
                return ApiResponse.ErrorResponse("Patient record not found");
            }

            var mergedNotes = FormatGeneralNotesWithGuestDemographics(dto.GeneralNotes ?? entity.GeneralNotes, dto.GuestDateOfBirth ?? entity.GuestDateOfBirth, dto.GuestGender ?? entity.GuestGender, dto.GuestAddress ?? entity.GuestAddress);

            entity.GuestName = dto.GuestName ?? entity.GuestName;
            entity.GuestPhone = dto.GuestPhone ?? entity.GuestPhone;
            entity.GuestEmail = dto.GuestEmail ?? entity.GuestEmail;
            entity.GuestDateOfBirth = dto.GuestDateOfBirth ?? entity.GuestDateOfBirth;
            entity.GuestGender = dto.GuestGender ?? entity.GuestGender;
            entity.GuestAddress = dto.GuestAddress ?? entity.GuestAddress;
            entity.PsychologicalHistory = dto.PsychologicalHistory;
            entity.CurrentSymptoms = dto.CurrentSymptoms;
            entity.StressFactors = dto.StressFactors;
            entity.GeneralNotes = mergedNotes;

            _repo.Update(entity);
            await _unitOfWork.SaveChangesAsync(ct);
            return ApiResponse.SuccessResponse("Patient record updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResponse($"Failed to update patient record: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteAsync(Guid doctorUserId, Guid id, CancellationToken ct = default)
    {
        try
        {
            var doctorProfile = await ResolveDoctorProfileAsync(doctorUserId, ct);
            if (doctorProfile == null)
            {
                return ApiResponse.ErrorResponse("Doctor profile not found.");
            }

            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null || entity.IsDeleted)
            {
                return ApiResponse.ErrorResponse("Patient record not found.");
            }

            if (entity.DoctorId != doctorProfile.Id)
            {
                return ApiResponse.ErrorResponse("You are not authorized to delete this patient record.");
            }

            entity.IsDeleted = true;
            _repo.Update(entity);
            await _unitOfWork.SaveChangesAsync(ct);
            return ApiResponse.SuccessResponse("Patient record deleted successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResponse($"Failed to delete patient record: {ex.Message}");
        }
    }

    public async Task<ApiResponse> CreateAccountForGuestAsync(Guid doctorUserId, Guid patientRecordId, CancellationToken ct = default)
    {
        var doctorProfile = await ResolveDoctorProfileAsync(doctorUserId, ct);
        if (doctorProfile == null)
            return ApiResponse.ErrorResponse("Doctor profile not found.");

        var record = await _repo.GetByIdAsync(patientRecordId, ct);
        if (record == null || record.IsDeleted)
            return ApiResponse.ErrorResponse("Patient record not found.");

        if (!MatchesDoctor(record.DoctorId, doctorProfile))
            return ApiResponse.ErrorResponse("You do not have permission to create an account for this patient.");

        if (record.PatientId.HasValue)
            return ApiResponse.ErrorResponse("This patient already has a registered account.");

        var email = NormalizeEmail(record.GuestEmail);
        if (string.IsNullOrWhiteSpace(email))
            return ApiResponse.ErrorResponse("Guest email is required to create a patient account.");

        var phone = record.GuestPhone?.Trim();
        if (string.IsNullOrWhiteSpace(phone))
            return ApiResponse.ErrorResponse("Guest phone number is required to create a patient account.");

        var allUsers = (await _userRepo.GetAllAsync(ct)).ToList();
        if (allUsers.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)))
            return ApiResponse.ErrorResponse("This email has already been registered.");

        if (allUsers.Any(u => string.Equals(u.PhoneNumber, phone, StringComparison.OrdinalIgnoreCase)))
            return ApiResponse.ErrorResponse("This phone number has already been registered.");

        var patientRole = (await _roleRepo.GetAllAsync(ct))
            .FirstOrDefault(r => r.Name == RoleConstants.Patient);
        if (patientRole == null)
            return ApiResponse.ErrorResponse("Patient role not found. Please run seed data.");

        var fullName = string.IsNullOrWhiteSpace(record.GuestName)
            ? email.Split('@')[0]
            : record.GuestName.Trim();

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(CreateUnusablePassword()),
            FullName = fullName,
            PhoneNumber = phone,
            RoleId = patientRole.Id,
            Role = patientRole,
            Status = UserStatus.Active,
            IsEmailVerified = true,
            CreatedBy = doctorUserId
        };

        var patientProfile = new PatientProfile
        {
            UserId = user.Id,
            User = user,
            MedicalHistory = record.PsychologicalHistory,
            CreatedBy = doctorUserId
        };

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await _userRepo.AddAsync(user, ct);
            await _patientRepo.AddAsync(patientProfile, ct);

            record.PatientId = patientProfile.Id;
            record.Patient = patientProfile;
            record.UpdatedAt = DateTime.UtcNow;
            record.UpdatedBy = doctorUserId;
            _repo.Update(record);

            var allAppointments = await _apptRepo.GetAllAsync(ct);
            var guestAppointments = allAppointments.Where(a =>
                !a.IsDeleted &&
                !a.PatientId.HasValue &&
                MatchesDoctor(a.DoctorId, doctorProfile) &&
                string.Equals(NormalizeEmail(a.GuestEmail), email, StringComparison.OrdinalIgnoreCase));

            foreach (var appointment in guestAppointments)
            {
                appointment.PatientId = patientProfile.Id;
                appointment.Patient = patientProfile;
                appointment.UpdatedAt = DateTime.UtcNow;
                appointment.UpdatedBy = doctorUserId;
                _apptRepo.Update(appointment);
            }

            var otpCode = await CreateSetPasswordOtpAsync(user, ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            var emailSent = await TrySendInvitationEmailAsync(user.Email, user.FullName, otpCode, ct);
            return ApiResponse.SuccessResponse(emailSent
                ? "Patient account created and invitation email sent."
                : "Patient account created, but the invitation email could not be sent. Please use Resend Invitation.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            return ApiResponse.ErrorResponse($"Failed to create patient account: {ex.Message}");
        }
    }

    public async Task<ApiResponse> ResendGuestAccountInvitationAsync(Guid doctorUserId, Guid patientRecordId, CancellationToken ct = default)
    {
        var doctorProfile = await ResolveDoctorProfileAsync(doctorUserId, ct);
        if (doctorProfile == null)
            return ApiResponse.ErrorResponse("Doctor profile not found.");

        var record = await _repo.GetByIdAsync(patientRecordId, ct);
        if (record == null || record.IsDeleted)
            return ApiResponse.ErrorResponse("Patient record not found.");

        if (!MatchesDoctor(record.DoctorId, doctorProfile))
            return ApiResponse.ErrorResponse("You do not have permission to resend this invitation.");

        if (!record.PatientId.HasValue)
            return ApiResponse.ErrorResponse("Create the patient account before resending an invitation.");

        var patient = (await _patientRepo.GetAllAsync(ct))
            .FirstOrDefault(p => p.Id == record.PatientId.Value || p.UserId == record.PatientId.Value);
        if (patient == null)
            return ApiResponse.ErrorResponse("Linked patient profile not found.");

        var user = await _userRepo.GetByIdAsync(patient.UserId, ct);
        if (user == null || user.IsDeleted)
            return ApiResponse.ErrorResponse("Linked patient account not found.");

        var otpCode = await CreateSetPasswordOtpAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var emailSent = await TrySendInvitationEmailAsync(user.Email, user.FullName, otpCode, ct);
        return emailSent
            ? ApiResponse.SuccessResponse("Invitation email resent successfully.")
            : ApiResponse.ErrorResponse("Invitation email could not be sent. Please check SMTP settings and try again.");
    }

    private async Task<DoctorProfile?> ResolveDoctorProfileAsync(Guid doctorUserId, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        return allDoctors.FirstOrDefault(d => d.UserId == doctorUserId || d.Id == doctorUserId);
    }

    private async Task<string> CreateSetPasswordOtpAsync(User user, CancellationToken ct)
    {
        var otpCode = GenerateOtp();
        await _otpRepo.AddAsync(new OtpVerification
        {
            UserId = user.Id,
            Code = otpCode,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = user
        }, ct);
        return otpCode;
    }

    private async Task<bool> TrySendInvitationEmailAsync(string email, string patientName, string otpCode, CancellationToken ct)
    {
        try
        {
            var resetLink = $"{GetPublicWebBaseUrl()}/Account/ResetPassword?email={Uri.EscapeDataString(email)}&otpCode={Uri.EscapeDataString(otpCode)}";
            var safeName = WebUtility.HtmlEncode(patientName);
            var html = $@"
                <p>Hello {safeName},</p>
                <p>Your doctor has invited you to use OPCBS to continue managing appointments and treatment progress online.</p>
                <p>Please set your password using the secure link below. This link expires in 24 hours.</p>
                <p><a href=""{resetLink}"" style=""display:inline-block;background:#0f766e;color:#fff;padding:12px 18px;border-radius:8px;text-decoration:none;font-weight:600;"">Set your password</a></p>
                <p>If the button does not work, copy and paste this URL into your browser:</p>
                <p>{WebUtility.HtmlEncode(resetLink)}</p>
                <p>If you did not expect this invitation, please contact OPCBS support.</p>";

            await _emailService.SendEmailAsync(email, "OPCBS - Set up your patient account", html, ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeEmail(string? email) => email?.Trim().ToLowerInvariant() ?? string.Empty;

    private static string CreateUnusablePassword() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

    private static string GenerateOtp()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var value = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
        return value.ToString("D6");
    }

    private static string GetPublicWebBaseUrl()
    {
        var configured = Environment.GetEnvironmentVariable("OPCBS_PUBLIC_WEB_URL");
        return string.IsNullOrWhiteSpace(configured) ? "http://localhost:5044" : configured.Trim().TrimEnd('/');
    }
}
