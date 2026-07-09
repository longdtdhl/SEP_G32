using AutoMapper;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Shared.Models;
using OPCBS.Domain.Entities;
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
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public PatientRecordService(IRepository<PatientRecord> repo, IRepository<DoctorProfile> doctorRepo, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _doctorRepo = doctorRepo;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PatientRecordDto>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _repo.GetAllAsync(ct);
        return _mapper.Map<List<PatientRecordDto>>(entities.OrderByDescending(x => x.CreatedAt));
    }

    public async Task<List<PatientRecordDto>> GetDoctorPatientsAsync(Guid doctorUserId, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctorProfile = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        
        if (doctorProfile == null) return new List<PatientRecordDto>();

        var entities = await _repo.GetAllAsync(ct);
        entities = entities.Where(x => x.DoctorId == doctorProfile.Id).ToList();
        return _mapper.Map<List<PatientRecordDto>>(entities.OrderByDescending(x => x.CreatedAt));
    }

    public async Task<List<PatientRecordDto>> GetSystemPatientsAsync(CancellationToken ct = default)
    {
        var entities = await _repo.GetAllAsync(ct);
        entities = entities.Where(x => x.PatientId != null).ToList();
        return _mapper.Map<List<PatientRecordDto>>(entities.OrderByDescending(x => x.CreatedAt));
    }

    public async Task<List<PatientRecordDto>> GetGuestPatientsAsync(CancellationToken ct = default)
    {
        var entities = await _repo.GetAllAsync(ct);
        entities = entities.Where(x => x.PatientId == null).ToList();
        return _mapper.Map<List<PatientRecordDto>>(entities.OrderByDescending(x => x.CreatedAt));
    }

    public async Task<PatientRecordDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity == null ? null : _mapper.Map<PatientRecordDto>(entity);
    }

    public async Task<PatientRecordDto?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var entities = await _repo.GetAllAsync(ct);
        var entity = entities.FirstOrDefault(x => x.PatientId == userId);
        return entity == null ? null : _mapper.Map<PatientRecordDto>(entity);
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
