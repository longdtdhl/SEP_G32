using AutoMapper;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Shared.Models;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
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
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public PatientRecordService(
        IRepository<PatientRecord> repo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<Appointment> apptRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<User> userRepo,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _doctorRepo = doctorRepo;
        _apptRepo = apptRepo;
        _patientRepo = patientRepo;
        _userRepo = userRepo;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    private async Task EnrichPatientRecordDtosAsync(List<PatientRecordDto> dtos, CancellationToken ct)
    {
        if (!dtos.Any()) return;
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var usersById = (await _userRepo.GetAllAsync(ct)).ToDictionary(user => user.Id);

        foreach (var dto in dtos)
        {
            if (!dto.PatientId.HasValue)
                continue;

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

            var entity = new PatientRecord
            {
                DoctorId = doctorProfile.Id,
                Doctor = doctorProfile,
                PatientId = dto.PatientId,
                GuestName = dto.GuestName,
                GuestPhone = dto.GuestPhone,
                GuestEmail = dto.GuestEmail,
                PsychologicalHistory = dto.PsychologicalHistory,
                CurrentSymptoms = dto.CurrentSymptoms,
                StressFactors = dto.StressFactors,
                GeneralNotes = dto.GeneralNotes,
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

    public async Task<ApiResponse> UpdateAsync(Guid id, UpdatePatientRecordDto dto, CancellationToken ct = default)
    {
        try
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null)
            {
                return ApiResponse.ErrorResponse("Patient record not found");
            }

            entity.GuestName = dto.GuestName ?? entity.GuestName;
            entity.GuestPhone = dto.GuestPhone ?? entity.GuestPhone;
            entity.GuestEmail = dto.GuestEmail ?? entity.GuestEmail;
            entity.PsychologicalHistory = dto.PsychologicalHistory;
            entity.CurrentSymptoms = dto.CurrentSymptoms;
            entity.StressFactors = dto.StressFactors;
            entity.GeneralNotes = dto.GeneralNotes;

            _repo.Update(entity);
            await _unitOfWork.SaveChangesAsync(ct);
            return ApiResponse.SuccessResponse("Patient record updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResponse($"Failed to update patient record: {ex.Message}");
        }
    }
}
