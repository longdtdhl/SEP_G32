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
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public PatientRecordService(
        IRepository<PatientRecord> repo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<Appointment> apptRepo,
        IRepository<PatientProfile> patientRepo,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _doctorRepo = doctorRepo;
        _apptRepo = apptRepo;
        _patientRepo = patientRepo;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    private async Task EnrichPatientRecordDtosAsync(List<PatientRecordDto> dtos, CancellationToken ct)
    {
        if (!dtos.Any()) return;
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patientUserMap = allPatients.ToDictionary(p => p.Id, p => p.UserId);

        foreach (var dto in dtos)
        {
            if (dto.PatientId.HasValue && patientUserMap.TryGetValue(dto.PatientId.Value, out var patUserId))
            {
                dto.PatientId = patUserId;
            }
        }
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

        // Get all patient IDs from appointments with this doctor
        var allAppts = await _apptRepo.GetAllAsync(ct);
        var myPatientUserIds = allAppts
            .Where(a => a.DoctorId == doctorProfile.Id && a.PatientId.HasValue &&
                        a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Rejected)
            .Select(a => a.PatientId!.Value)
            .Distinct()
            .ToHashSet();

        // Get patient records: either created by this doctor OR linked to a patient who had an appointment
        var allRecords = await _repo.GetAllAsync(ct);
        var myRecords = allRecords.Where(r =>
            r.DoctorId == doctorProfile.Id ||  // Records created by this doctor
            (r.PatientId.HasValue && myPatientUserIds.Contains(r.PatientId.Value))  // Patients with appointments
        ).OrderByDescending(x => x.CreatedAt).ToList();

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

        // Doctor created this record
        if (record.DoctorId == doctorProfile.Id) return true;

        // Doctor has appointment with this patient
        if (record.PatientId.HasValue)
        {
            var allAppts = await _apptRepo.GetAllAsync(ct);
            return allAppts.Any(a =>
                a.DoctorId == doctorProfile.Id &&
                a.PatientId == record.PatientId &&
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
        entities = entities.Where(x => x.DoctorId == doctorProfile.Id).ToList();
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
