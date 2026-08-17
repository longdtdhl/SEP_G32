using OPCBS.Application.DTOs.Appointments;
using OPCBS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OPCBS.Application.Interfaces.Services;

public interface IPatientRecordService
{
    Task<List<PatientRecordDto>> GetAllAsync(CancellationToken ct = default);
    Task<List<PatientRecordDto>> GetSystemPatientsAsync(CancellationToken ct = default);
    Task<List<PatientRecordDto>> GetGuestPatientsAsync(CancellationToken ct = default);
    Task<List<PatientRecordDto>> GetMyPatientsAsync(Guid doctorUserId, CancellationToken ct = default);
    Task<bool> CanDoctorAccessPatientAsync(Guid doctorUserId, Guid patientRecordId, CancellationToken ct = default);
    Task<PatientRecordDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PatientRecordDto?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse> CreateAsync(Guid doctorId, CreatePatientRecordDto dto, CancellationToken ct = default);
    Task<ApiResponse> UpdateAsync(Guid id, UpdatePatientRecordDto dto, CancellationToken ct = default);
    Task<ApiResponse> CreateAccountForGuestAsync(Guid doctorUserId, Guid patientRecordId, CancellationToken ct = default);
    Task<ApiResponse> ResendGuestAccountInvitationAsync(Guid doctorUserId, Guid patientRecordId, CancellationToken ct = default);
}
