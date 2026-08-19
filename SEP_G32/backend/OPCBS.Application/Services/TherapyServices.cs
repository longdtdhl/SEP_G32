using OPCBS.Application.DTOs.Therapy;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Entities;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Services;

public class TherapyAssignmentService : ITherapyAssignmentService
{
    private readonly IRepository<TherapyAssignment> _assignmentRepo;
    private readonly IRepository<TreatmentPackage> _packageRepo;
    private readonly IUnitOfWork _uow;

    public TherapyAssignmentService(
        IRepository<TherapyAssignment> assignmentRepo,
        IRepository<TreatmentPackage> packageRepo,
        IUnitOfWork uow)
    {
        _assignmentRepo = assignmentRepo;
        _packageRepo = packageRepo;
        _uow = uow;
    }

    public async Task<ApiResponse<List<TherapyAssignmentDto>>> GetByPackageAsync(Guid treatmentPackageId, CancellationToken ct)
    {
        var all = await _assignmentRepo.GetAllAsync(ct);
        var filtered = all
            .Where(a => a.TreatmentPackageId == treatmentPackageId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Select(MapToDto)
            .ToList();
        return ApiResponse<List<TherapyAssignmentDto>>.SuccessResponse(filtered);
    }

    public async Task<ApiResponse<TherapyAssignmentDto>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await _assignmentRepo.GetByIdAsync(id, ct);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<TherapyAssignmentDto>.ErrorResponse("Không tìm thấy bài tập.");
        return ApiResponse<TherapyAssignmentDto>.SuccessResponse(MapToDto(entity));
    }

    public async Task<ApiResponse<TherapyAssignmentDto>> CreateAsync(CreateAssignmentDto dto, CancellationToken ct)
    {
        var pkg = await _packageRepo.GetByIdAsync(dto.TreatmentPackageId, ct);
        if (pkg == null || pkg.IsDeleted)
            return ApiResponse<TherapyAssignmentDto>.ErrorResponse("Không tìm thấy gói điều trị.");

        var entity = new TherapyAssignment
        {
            TreatmentPackageId = dto.TreatmentPackageId,
            Title = dto.Title,
            Description = dto.Description,
            DetailedInstructions = dto.DetailedInstructions,
            ResourceUrl = dto.ResourceUrl,
            DueDate = dto.DueDate,
            Status = Domain.Enums.HomeworkStatus.Assigned,
            TreatmentPackage = pkg
        };

        await _assignmentRepo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<TherapyAssignmentDto>.SuccessResponse(MapToDto(entity), "Đã giao bài tập thành công.");
    }

    public async Task<ApiResponse<TherapyAssignmentDto>> SubmitAsync(Guid id, SubmitAssignmentDto dto, CancellationToken ct)
    {
        var entity = await _assignmentRepo.GetByIdAsync(id, ct);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<TherapyAssignmentDto>.ErrorResponse("Assignment not found.");

        entity.PatientSubmission = dto.PatientSubmission;
        entity.PatientSubmissionUrl = dto.PatientSubmissionUrl;
        entity.SubmittedAt = DateTime.UtcNow;
        entity.Status = Domain.Enums.HomeworkStatus.Submitted;
        entity.UpdatedAt = DateTime.UtcNow;
        _assignmentRepo.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<TherapyAssignmentDto>.SuccessResponse(MapToDto(entity), "Assignment submitted successfully.");
    }

    public async Task<ApiResponse<TherapyAssignmentDto>> FeedbackAsync(Guid id, FeedbackAssignmentDto dto, CancellationToken ct)
    {
        var entity = await _assignmentRepo.GetByIdAsync(id, ct);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<TherapyAssignmentDto>.ErrorResponse("Không tìm thấy bài tập.");
        if (entity.Status == Domain.Enums.HomeworkStatus.Assigned)
            return ApiResponse<TherapyAssignmentDto>.ErrorResponse("Bệnh nhân chưa nộp bài tập.");

        entity.DoctorFeedback = dto.DoctorFeedback;
        entity.FeedbackAt = DateTime.UtcNow;
        entity.Status = Domain.Enums.HomeworkStatus.Reviewed;
        entity.UpdatedAt = DateTime.UtcNow;
        _assignmentRepo.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<TherapyAssignmentDto>.SuccessResponse(MapToDto(entity), "Đã nhận xét bài tập.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _assignmentRepo.GetByIdAsync(id, ct);
        if (entity == null) return ApiResponse.ErrorResponse("Không tìm thấy bài tập.");
        entity.IsDeleted = true;
        _assignmentRepo.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Đã xóa bài tập.");
    }

    private static TherapyAssignmentDto MapToDto(TherapyAssignment e) => new()
    {
        Id = e.Id,
        TreatmentPackageId = e.TreatmentPackageId,
        Title = e.Title,
        Description = e.Description,
        DetailedInstructions = e.DetailedInstructions,
        ResourceUrl = e.ResourceUrl,
        DueDate = e.DueDate,
        Status = (int)e.Status,
        PatientSubmission = e.PatientSubmission,
        PatientSubmissionUrl = e.PatientSubmissionUrl,
        SubmittedAt = e.SubmittedAt,
        DoctorFeedback = e.DoctorFeedback,
        FeedbackAt = e.FeedbackAt,
        CreatedAt = e.CreatedAt
    };
}

public class EmotionJournalService : IEmotionJournalService
{
    private readonly IRepository<EmotionJournal> _journalRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _uow;

    public EmotionJournalService(
        IRepository<EmotionJournal> journalRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<User> userRepo,
        IUnitOfWork uow)
    {
        _journalRepo = journalRepo;
        _patientRepo = patientRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<ApiResponse<List<EmotionJournalDto>>> GetByPatientAsync(Guid patientUserId, CancellationToken ct)
    {
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        if (patient == null)
            return ApiResponse<List<EmotionJournalDto>>.ErrorResponse("Không tìm thấy hồ sơ bệnh nhân.");

        var user = await _userRepo.GetByIdAsync(patient.UserId, ct);
        var all = await _journalRepo.GetAllAsync(ct);
        var filtered = all
            .Where(j => j.PatientId == patient.Id && !j.IsDeleted)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => MapToDto(j, user?.FullName))
            .ToList();
        return ApiResponse<List<EmotionJournalDto>>.SuccessResponse(filtered);
    }

    public async Task<ApiResponse<List<EmotionJournalDto>>> GetSharedByPatientAsync(Guid patientId, CancellationToken ct)
    {
        var patient = await _patientRepo.GetByIdAsync(patientId, ct);
        if (patient == null)
        {
            var allPatients = await _patientRepo.GetAllAsync(ct);
            patient = allPatients.FirstOrDefault(p => p.Id == patientId || p.UserId == patientId);
        }
        if (patient == null)
            return ApiResponse<List<EmotionJournalDto>>.ErrorResponse("Không tìm thấy hồ sơ bệnh nhân.");

        var user = await _userRepo.GetByIdAsync(patient.UserId, ct);
        var all = await _journalRepo.GetAllAsync(ct);
        var filtered = all
            .Where(j => (j.PatientId == patient.Id || j.PatientId == patient.UserId) && j.IsShared && !j.IsDeleted)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => MapToDto(j, user?.FullName))
            .ToList();
        return ApiResponse<List<EmotionJournalDto>>.SuccessResponse(filtered);
    }

    public async Task<ApiResponse<EmotionJournalDto>> CreateAsync(CreateJournalDto dto, Guid patientUserId, CancellationToken ct)
    {
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        if (patient == null)
            return ApiResponse<EmotionJournalDto>.ErrorResponse("Không tìm thấy hồ sơ bệnh nhân.");

        if (dto.MoodScale < 1 || dto.MoodScale > 5)
            return ApiResponse<EmotionJournalDto>.ErrorResponse("Thang điểm cảm xúc phải từ 1 đến 5.");
        if (dto.StressScale < 1 || dto.StressScale > 5)
            return ApiResponse<EmotionJournalDto>.ErrorResponse("Thang điểm căng thẳng phải từ 1 đến 5.");
        if (dto.DepressionScale.HasValue && (dto.DepressionScale.Value < 1 || dto.DepressionScale.Value > 5))
            return ApiResponse<EmotionJournalDto>.ErrorResponse("Thang điểm trầm cảm phải từ 1 đến 5.");
        if (dto.SleepHours.HasValue && (dto.SleepHours.Value < 0 || dto.SleepHours.Value > 24))
            return ApiResponse<EmotionJournalDto>.ErrorResponse("Số giờ ngủ phải từ 0 đến 24 giờ.");

        var user = await _userRepo.GetByIdAsync(patient.UserId, ct);
        var entity = new EmotionJournal
        {
            PatientId = patient.Id,
            Title = dto.Title,
            Content = dto.Content,
            MoodScale = dto.MoodScale,
            StressScale = dto.StressScale,
            SleepHours = dto.SleepHours,
            DepressionScale = dto.DepressionScale,
            IsShared = dto.IsShared,
            Patient = patient
        };

        await _journalRepo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<EmotionJournalDto>.SuccessResponse(MapToDto(entity, user?.FullName), "Đã lưu nhật ký cảm xúc.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, Guid patientUserId, CancellationToken ct)
    {
        var entity = await _journalRepo.GetByIdAsync(id, ct);
        if (entity == null) return ApiResponse.ErrorResponse("Không tìm thấy nhật ký.");

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        if (patient == null || entity.PatientId != patient.Id)
            return ApiResponse.ErrorResponse("Bạn không có quyền xóa nhật ký này.");

        entity.IsDeleted = true;
        _journalRepo.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Đã xóa nhật ký.");
    }

    private static EmotionJournalDto MapToDto(EmotionJournal j, string? patientName) => new()
    {
        Id = j.Id,
        PatientId = j.PatientId,
        PatientName = patientName,
        Title = j.Title,
        Content = j.Content,
        MoodScale = j.MoodScale,
        StressScale = j.StressScale,
        SleepHours = j.SleepHours,
        DepressionScale = j.DepressionScale,
        IsShared = j.IsShared,
        CreatedAt = j.CreatedAt
    };
}
