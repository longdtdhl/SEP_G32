using AutoMapper;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Services;

public class BlogService : IBlogService
{
    private readonly IRepository<BlogPost> _blogRepo;
    private readonly IRepository<BlogComment> _commentRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public BlogService(IRepository<BlogPost> blogRepo, IRepository<BlogComment> commentRepo, IRepository<DoctorProfile> doctorRepo, IRepository<User> userRepo, IUnitOfWork uow, IMapper mapper)
    { _blogRepo = blogRepo; _commentRepo = commentRepo; _doctorRepo = doctorRepo; _userRepo = userRepo; _uow = uow; _mapper = mapper; }

    public async Task<ApiResponse<List<BlogPostDto>>> GetPublishedBlogsAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var all = await _blogRepo.GetAllAsync(ct);
        var blogs = all.Where(b => b.Status == BlogStatus.Published && !b.IsDeleted).ToList();
        if (!string.IsNullOrWhiteSpace(search))
            blogs = blogs.Where(b => b.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        var total = blogs.Count;
        var items = blogs.OrderByDescending(b => b.PublishedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ApiResponse<List<BlogPostDto>>.SuccessResponse(_mapper.Map<List<BlogPostDto>>(items), pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<BlogPostDto>> GetBlogByIdAsync(Guid blogId, CancellationToken ct)
    {
        var blog = await _blogRepo.GetByIdAsync(blogId, ct);
        if (blog == null) return ApiResponse<BlogPostDto>.ErrorResponse("Blog not found");
        blog.ViewCount++;
        _blogRepo.Update(blog);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<BlogPostDto>.SuccessResponse(_mapper.Map<BlogPostDto>(blog));
    }

    public async Task<ApiResponse<BlogPostDto>> CreateBlogAsync(Guid doctorUserId, CreateBlogPostDto dto, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<BlogPostDto>.ErrorResponse("Doctor not found");

        var blog = new BlogPost { DoctorId = doctor.Id, Title = dto.Title, Content = dto.Content, ThumbnailUrl = dto.ThumbnailUrl, Excerpt = dto.Excerpt, Doctor = doctor };
        await _blogRepo.AddAsync(blog, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<BlogPostDto>.SuccessResponse(_mapper.Map<BlogPostDto>(blog), "Blog created");
    }

    public async Task<ApiResponse<BlogPostDto>> UpdateBlogAsync(Guid blogId, Guid doctorUserId, UpdateBlogPostDto dto, CancellationToken ct)
    {
        var blog = await _blogRepo.GetByIdAsync(blogId, ct);
        if (blog == null) return ApiResponse<BlogPostDto>.ErrorResponse("Blog not found");
        if (!string.IsNullOrWhiteSpace(dto.Title)) blog.Title = dto.Title;
        if (!string.IsNullOrWhiteSpace(dto.Content)) blog.Content = dto.Content;
        if (!string.IsNullOrWhiteSpace(dto.ThumbnailUrl)) blog.ThumbnailUrl = dto.ThumbnailUrl;
        if (dto.Excerpt != null) blog.Excerpt = dto.Excerpt;
        blog.UpdatedAt = DateTime.UtcNow;
        _blogRepo.Update(blog);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<BlogPostDto>.SuccessResponse(_mapper.Map<BlogPostDto>(blog), "Blog updated");
    }

    public async Task<ApiResponse> SubmitBlogForReviewAsync(Guid blogId, Guid doctorUserId, CancellationToken ct)
    {
        var blog = await _blogRepo.GetByIdAsync(blogId, ct);
        if (blog == null) return ApiResponse.ErrorResponse("Blog not found");
        if (blog.Status != BlogStatus.Draft && blog.Status != BlogStatus.Rejected)
            return ApiResponse.ErrorResponse("Only draft or rejected blogs can be submitted");
        blog.Status = BlogStatus.Pending;
        blog.SubmittedAt = DateTime.UtcNow;
        _blogRepo.Update(blog);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Blog submitted for review");
    }

    public async Task<ApiResponse<List<BlogPostDto>>> GetDoctorBlogsAsync(Guid doctorUserId, int page, int pageSize, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<List<BlogPostDto>>.ErrorResponse("Doctor not found");
        var all = await _blogRepo.GetAllAsync(ct);
        var blogs = all.Where(b => b.DoctorId == doctor.Id && !b.IsDeleted).ToList();
        var total = blogs.Count;
        var items = blogs.OrderByDescending(b => b.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ApiResponse<List<BlogPostDto>>.SuccessResponse(_mapper.Map<List<BlogPostDto>>(items), pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<List<BlogPostDto>>> GetPendingBlogsAsync(int page, int pageSize, CancellationToken ct)
    {
        var all = await _blogRepo.GetAllAsync(ct);
        var blogs = all.Where(b => b.Status == BlogStatus.Pending).ToList();
        var total = blogs.Count;
        var items = blogs.OrderBy(b => b.SubmittedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ApiResponse<List<BlogPostDto>>.SuccessResponse(_mapper.Map<List<BlogPostDto>>(items), pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse> ApproveBlogAsync(Guid blogId, Guid supportUserId, CancellationToken ct)
    {
        var blog = await _blogRepo.GetByIdAsync(blogId, ct);
        if (blog == null) return ApiResponse.ErrorResponse("Blog not found");
        blog.Status = BlogStatus.Published;
        blog.ApprovedAt = DateTime.UtcNow;
        blog.ApprovedBy = supportUserId;
        blog.PublishedAt = DateTime.UtcNow;
        _blogRepo.Update(blog);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Blog approved and published");
    }

    public async Task<ApiResponse> RejectBlogAsync(Guid blogId, Guid supportUserId, string? reason, CancellationToken ct)
    {
        var blog = await _blogRepo.GetByIdAsync(blogId, ct);
        if (blog == null) return ApiResponse.ErrorResponse("Blog not found");
        blog.Status = BlogStatus.Rejected;
        blog.RejectionReason = reason;
        _blogRepo.Update(blog);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Blog rejected");
    }

    public async Task<ApiResponse> DeleteBlogAsync(Guid blogId, Guid doctorUserId, CancellationToken ct)
    {
        var blog = await _blogRepo.GetByIdAsync(blogId, ct);
        if (blog == null) return ApiResponse.ErrorResponse("Blog not found");
        blog.IsDeleted = true;
        _blogRepo.Update(blog);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Blog deleted");
    }

    // ── Comment methods (real DB implementation) ──

    public async Task<ApiResponse<List<BlogCommentDto>>> GetCommentsForBlogAsync(Guid blogPostId, CancellationToken ct)
    {
        var allComments = await _commentRepo.GetAllAsync(ct);
        var comments = allComments
            .Where(c => c.BlogPostId == blogPostId && !c.IsDeleted && c.IsApproved)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        var allUsers = await _userRepo.GetAllAsync(ct);
        var result = comments.Select(c =>
        {
            var userName = c.AuthorName ?? "Ẩn danh";
            if (c.PatientId.HasValue && c.Patient?.User != null)
            {
                userName = c.Patient.User.FullName;
            }
            else if (c.PatientId.HasValue)
            {
                // Fallback: look up user
                var patient = c.Patient;
                if (patient != null)
                {
                    var user = allUsers.FirstOrDefault(u => u.Id == patient.UserId);
                    if (user != null) userName = user.FullName;
                }
            }
            return new BlogCommentDto
            {
                Id = c.Id,
                BlogPostId = c.BlogPostId,
                UserName = userName,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            };
        }).ToList();

        return ApiResponse<List<BlogCommentDto>>.SuccessResponse(result);
    }

    public async Task<ApiResponse<BlogCommentDto>> AddCommentAsync(Guid userId, CreateBlogCommentDto dto, CancellationToken ct)
    {
        var blog = await _blogRepo.GetByIdAsync(dto.BlogPostId, ct);
        if (blog == null) return ApiResponse<BlogCommentDto>.ErrorResponse("Blog not found");

        var allUsers = await _userRepo.GetAllAsync(ct);
        var user = allUsers.FirstOrDefault(u => u.Id == userId);
        var userName = user?.FullName ?? "Ẩn danh";

        var comment = new BlogComment
        {
            BlogPostId = dto.BlogPostId,
            AuthorName = userName,
            Content = dto.Content,
            IsApproved = true,
            BlogPost = blog
        };

        await _commentRepo.AddAsync(comment, ct);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<BlogCommentDto>.SuccessResponse(new BlogCommentDto
        {
            Id = comment.Id,
            BlogPostId = comment.BlogPostId,
            UserName = userName,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        }, "Comment added");
    }

    public async Task<ApiResponse<BlogCommentDto>> UpdateCommentAsync(Guid commentId, Guid userId, UpdateBlogCommentDto dto, CancellationToken ct)
    {
        var comment = await _commentRepo.GetByIdAsync(commentId, ct);
        if (comment == null) return ApiResponse<BlogCommentDto>.ErrorResponse("Comment not found");

        comment.Content = dto.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        _commentRepo.Update(comment);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<BlogCommentDto>.SuccessResponse(new BlogCommentDto
        {
            Id = comment.Id,
            BlogPostId = comment.BlogPostId,
            UserName = comment.AuthorName ?? "Ẩn danh",
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        }, "Comment updated");
    }

    public async Task<ApiResponse> DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken ct)
    {
        var comment = await _commentRepo.GetByIdAsync(commentId, ct);
        if (comment == null) return ApiResponse.ErrorResponse("Comment not found");

        comment.IsDeleted = true;
        _commentRepo.Update(comment);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Comment deleted");
    }
}

public class ReviewService : IReviewService
{
    private readonly IRepository<Review> _reviewRepo;
    private readonly IRepository<Appointment> _apptRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ReviewService(IRepository<Review> reviewRepo, IRepository<Appointment> apptRepo, IRepository<PatientProfile> patientRepo, IRepository<DoctorProfile> doctorRepo, IUnitOfWork uow, IMapper mapper)
    { _reviewRepo = reviewRepo; _apptRepo = apptRepo; _patientRepo = patientRepo; _doctorRepo = doctorRepo; _uow = uow; _mapper = mapper; }

    public async Task<ApiResponse<ReviewDto>> CreateReviewAsync(Guid patientUserId, CreateReviewDto dto, CancellationToken ct)
    {
        var appointment = await _apptRepo.GetByIdAsync(dto.AppointmentId, ct);
        if (appointment == null) return ApiResponse<ReviewDto>.ErrorResponse("Appointment not found");
        if (appointment.Status != AppointmentStatus.Completed) return ApiResponse<ReviewDto>.ErrorResponse("Only completed appointments can be reviewed");

        var allReviews = await _reviewRepo.GetAllAsync(ct);
        if (allReviews.Any(r => r.AppointmentId == dto.AppointmentId))
            return ApiResponse<ReviewDto>.ErrorResponse("Appointment already reviewed");

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        if (patient == null) return ApiResponse<ReviewDto>.ErrorResponse("Patient not found");

        var review = new Review
        {
            AppointmentId = dto.AppointmentId, DoctorId = appointment.DoctorId, PatientId = patient.Id,
            Rating = dto.Rating, Comment = dto.Comment,
            Appointment = appointment, Doctor = appointment.Doctor, Patient = patient
        };
        await _reviewRepo.AddAsync(review, ct);

        // Recalculate doctor average rating
        var doctor = await _doctorRepo.GetByIdAsync(appointment.DoctorId, ct);
        if (doctor != null)
        {
            var doctorReviews = allReviews.Where(r => r.DoctorId == doctor.Id).ToList();
            doctorReviews.Add(review);
            doctor.AverageRating = (decimal)doctorReviews.Average(r => r.Rating);
            doctor.ReviewCount = doctorReviews.Count;
            _doctorRepo.Update(doctor);
        }

        await _uow.SaveChangesAsync(ct);
        return ApiResponse<ReviewDto>.SuccessResponse(_mapper.Map<ReviewDto>(review), "Review submitted");
    }

    public async Task<ApiResponse<List<ReviewDto>>> GetDoctorReviewsAsync(Guid doctorProfileId, int page, int pageSize, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.Id == doctorProfileId || d.UserId == doctorProfileId);
        var docId = doctor?.Id ?? doctorProfileId;

        var all = await _reviewRepo.GetAllAsync(ct);
        var reviews = all.Where(r => r.DoctorId == docId && r.IsVisible).ToList();
        var total = reviews.Count;
        var items = reviews.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ApiResponse<List<ReviewDto>>.SuccessResponse(_mapper.Map<List<ReviewDto>>(items), pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }
}

public class ConsultationNoteService : IConsultationNoteService
{
    private readonly IRepository<ConsultationNote> _recordRepo;
    private readonly IRepository<Appointment> _apptRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<PatientRecord> _patientRecordRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<TreatmentPackage> _packageRepo;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ConsultationNoteService(
        IRepository<ConsultationNote> recordRepo,
        IRepository<Appointment> apptRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<PatientRecord> patientRecordRepo,
        IRepository<User> userRepo,
        IRepository<TreatmentPackage> packageRepo,
        INotificationService notificationService,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _recordRepo = recordRepo;
        _apptRepo = apptRepo;
        _doctorRepo = doctorRepo;
        _patientRepo = patientRepo;
        _patientRecordRepo = patientRecordRepo;
        _userRepo = userRepo;
        _packageRepo = packageRepo;
        _notificationService = notificationService;
        _uow = uow;
        _mapper = mapper;
    }

    private async Task EnrichRecordsAsync(List<ConsultationNoteDto> dtos, CancellationToken ct)
    {
        if (!dtos.Any()) return;
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var allUsers = await _userRepo.GetAllAsync(ct);
        var allPatientRecords = await _patientRecordRepo.GetAllAsync(ct);
        var allNotes = await _recordRepo.GetAllAsync(ct);

        var userDict = allUsers.ToDictionary(u => u.Id, u => u.FullName);
        var doctorUserMap = allDoctors.ToDictionary(d => d.Id, d => d.UserId);
        var patientUserMap = allPatients.ToDictionary(p => p.Id, p => p.UserId);
        var patientRecordMap = allPatientRecords.ToDictionary(pr => pr.Id);

        // Build appointment → package name lookup
        Dictionary<Guid, string> apptPackageMap = new();
        try
        {
            var allAppts = await _apptRepo.GetAllAsync(ct);
            var allPackages = await _packageRepo.GetAllAsync(ct);
            var packageDict = allPackages.ToDictionary(p => p.Id, p => p.Name);
            foreach (var appt in allAppts.Where(a => a.TreatmentPackageId.HasValue))
            {
                if (appt.TreatmentPackageId.HasValue && packageDict.TryGetValue(appt.TreatmentPackageId.Value, out var pkgName))
                    apptPackageMap[appt.Id] = pkgName;
            }
        }
        catch { }

        foreach (var dto in dtos)
        {
            // Enrich from entity for fields AutoMapper may miss
            var entity = allNotes.FirstOrDefault(n => n.Id == dto.Id);
            if (entity != null)
            {
                dto.ConsultationDate = entity.ConsultationDate;
                dto.Visibility = (int)entity.Visibility;
            }

            if (string.IsNullOrEmpty(dto.DoctorName) && doctorUserMap.TryGetValue(dto.DoctorId, out var docUserId) && userDict.TryGetValue(docUserId, out var docName))
                dto.DoctorName = docName;
                
            if (patientRecordMap.TryGetValue(dto.PatientRecordId, out var pr))
            {
                if (pr.PatientId.HasValue && patientUserMap.TryGetValue(pr.PatientId.Value, out var patUserId) && userDict.TryGetValue(patUserId, out var patName))
                {
                    dto.PatientName = patName;
                }
                else
                {
                    dto.WalkInPatientName = pr.GuestName;
                    dto.WalkInPatientPhone = pr.GuestPhone;
                    dto.WalkInPatientEmail = pr.GuestEmail;
                }
            }

            // Enrich package name from appointment
            if (dto.AppointmentId.HasValue && apptPackageMap.TryGetValue(dto.AppointmentId.Value, out var packageName))
                dto.PackageName = packageName;
        }
    }

    public async Task<ApiResponse<ConsultationNoteDto>> CreateAsync(Guid doctorUserId, CreateConsultationNoteDto dto, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<ConsultationNoteDto>.ErrorResponse("Doctor not found");

        if (dto.NextAppointmentRecommendedDate.HasValue && dto.NextAppointmentRecommendedDate.Value.Date < DateTime.Today)
        {
            return ApiResponse<ConsultationNoteDto>.ErrorResponse("Recommended follow-up date cannot be in the past.");
        }

        PatientRecord? patientRecord = null;
        if (dto.PatientRecordId != Guid.Empty)
        {
            patientRecord = await _patientRecordRepo.GetByIdAsync(dto.PatientRecordId, ct);
        }

        // Auto-create PatientRecord if missing (e.g., guest booking)
        if (patientRecord == null && dto.AppointmentId.HasValue)
        {
            var appointment = await _apptRepo.GetByIdAsync(dto.AppointmentId.Value, ct);
            if (appointment != null)
            {
                if (appointment.PatientId.HasValue)
                {
                    // Check if patient already has a record with this doctor
                    var allRecords = await _patientRecordRepo.GetAllAsync(ct);
                    patientRecord = allRecords.FirstOrDefault(r => r.PatientId == appointment.PatientId && r.DoctorId == doctor.Id);
                }

                if (patientRecord == null)
                {
                    patientRecord = new PatientRecord
                    {
                        DoctorId = doctor.Id,
                        PatientId = appointment.PatientId,
                        Doctor = doctor,
                        GuestName = appointment.GuestName,
                        GuestEmail = appointment.GuestEmail,
                        GuestPhone = appointment.GuestPhoneNumber,
                        GeneralNotes = $"Auto-created from appointment {appointment.BookingCode}"
                    };
                    if (appointment.PatientId.HasValue)
                    {
                        var allPatients = await _patientRepo.GetAllAsync(ct);
                        var pat = allPatients.FirstOrDefault(p => p.Id == appointment.PatientId.Value);
                        if (pat != null) patientRecord.Patient = pat;
                    }
                    await _patientRecordRepo.AddAsync(patientRecord, ct);
                    await _uow.SaveChangesAsync(ct);
                }
            }
        }

        if (patientRecord == null) return ApiResponse<ConsultationNoteDto>.ErrorResponse("Could not resolve or create patient record");

        var record = new ConsultationNote
        {
            AppointmentId = dto.AppointmentId,
            DoctorId = doctor.Id,
            PatientRecordId = patientRecord.Id,
            ConsultationSummary = dto.ConsultationSummary,
            Diagnosis = dto.Diagnosis,
            Recommendation = dto.Recommendation,
            FollowUpNotes = dto.FollowUpNotes,
            TherapyPlan = dto.TherapyPlan,
            NextAppointmentRecommendedDate = dto.NextAppointmentRecommendedDate,
            ConsultationDate = dto.ConsultationDate,
            Visibility = (NoteVisibility)dto.Visibility,
            Doctor = doctor,
            PatientRecord = patientRecord
        };
        if (dto.AppointmentId.HasValue)
        {
            var appointment = await _apptRepo.GetByIdAsync(dto.AppointmentId.Value, ct);
            if (appointment != null)
            {
                record.Appointment = appointment;
                // Auto-fill ConsultationDate from appointment date if not set
                if (!record.ConsultationDate.HasValue)
                    record.ConsultationDate = appointment.AppointmentDate;
                if (appointment.PatientId.HasValue)
                {
                    try
                    {
                        var allPatients = await _patientRepo.GetAllAsync(ct);
                        var pat = allPatients.FirstOrDefault(p => p.Id == appointment.PatientId.Value);
                        var allUsers = await _userRepo.GetAllAsync(ct);
                        var doctorUser = allUsers.FirstOrDefault(u => u.Id == doctorUserId);
                        if (pat != null)
                        {
                            await _notificationService.CreateNotificationAsync(
                                pat.UserId,
                                "📋 New Consultation Record",
                                $"Dr. {doctorUser?.FullName ?? "your doctor"} has created a consultation record for your appointment. Please review it.",
                                Domain.Enums.NotificationType.ConsultationNote,
                                record.Id, // this might be empty guid since not saved yet
                                "ConsultationNote",
                                ct);
                        }
                    }
                    catch { }
                }
            }
        }
        await _recordRepo.AddAsync(record, ct);
        await _uow.SaveChangesAsync(ct);
        var createdLinkedDto = _mapper.Map<ConsultationNoteDto>(record);
        await EnrichRecordsAsync(new List<ConsultationNoteDto> { createdLinkedDto }, ct);

        return ApiResponse<ConsultationNoteDto>.SuccessResponse(createdLinkedDto, "Record created");
    }

    public async Task<ApiResponse<ConsultationNoteDto>> UpdateAsync(Guid recordId, Guid doctorUserId, UpdateConsultationNoteDto dto, CancellationToken ct)
    {
        var record = await _recordRepo.GetByIdAsync(recordId, ct);
        if (record == null) return ApiResponse<ConsultationNoteDto>.ErrorResponse("Record not found");
        record.ConsultationSummary = dto.ConsultationSummary;
        record.Diagnosis = dto.Diagnosis;
        record.Recommendation = dto.Recommendation;
        record.FollowUpNotes = dto.FollowUpNotes;
        record.TherapyPlan = dto.TherapyPlan;
        record.Visibility = (NoteVisibility)dto.Visibility;
        // Only allow updating ConsultationDate if not linked to an appointment
        if (!record.AppointmentId.HasValue && dto.ConsultationDate.HasValue)
            record.ConsultationDate = dto.ConsultationDate;
        record.UpdatedAt = DateTime.UtcNow;
        _recordRepo.Update(record);
        await _uow.SaveChangesAsync(ct);
        var updatedDto = _mapper.Map<ConsultationNoteDto>(record);
        await EnrichRecordsAsync(new List<ConsultationNoteDto> { updatedDto }, ct);
        return ApiResponse<ConsultationNoteDto>.SuccessResponse(updatedDto, "Record updated");
    }

    public async Task<ApiResponse<List<ConsultationNoteDto>>> GetByPatientRecordAsync(Guid patientRecordId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var records = await _recordRepo.GetAllAsync(ct);
        var filtered = records.Where(x => x.PatientRecordId == patientRecordId).OrderByDescending(x => x.CreatedAt);

        var total = filtered.Count();
        var paged = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var dtos = _mapper.Map<List<ConsultationNoteDto>>(paged);
        await EnrichRecordsAsync(dtos, ct);

        var pagination = new PaginationMetadata
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };

        return ApiResponse<List<ConsultationNoteDto>>.SuccessResponse(dtos, "Records retrieved successfully", pagination);
    }

    public async Task<ApiResponse<List<ConsultationNoteDto>>> GetByPatientAsync(Guid patientUserId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var patientRecord = await _patientRecordRepo.GetByIdAsync(patientUserId, ct);
        if (patientRecord == null)
        {
            // Try to find by patient UserId
            var allPatients = await _patientRepo.GetAllAsync(ct);
            var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
            if (patient != null)
            {
                var allPatientRecords = await _patientRecordRepo.GetAllAsync(ct);
                patientRecord = allPatientRecords.FirstOrDefault(pr => pr.PatientId == patient.Id);
            }
        }
        
        if (patientRecord == null) return ApiResponse<List<ConsultationNoteDto>>.ErrorResponse("Patient record not found");
        var all = await _recordRepo.GetAllAsync(ct);
        var records = all.Where(r => r.PatientRecordId == patientRecord.Id).ToList();
        var total = records.Count;
        var items = records.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = _mapper.Map<List<ConsultationNoteDto>>(items);
        await EnrichRecordsAsync(dtos, ct);
        return ApiResponse<List<ConsultationNoteDto>>.SuccessResponse(dtos, pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<ConsultationNoteDto>> GetByIdAsync(Guid recordId, Guid userId, CancellationToken ct)
    {
        var record = await _recordRepo.GetByIdAsync(recordId, ct);
        if (record == null) return ApiResponse<ConsultationNoteDto>.ErrorResponse("Record not found");
        var dto = _mapper.Map<ConsultationNoteDto>(record);
        await EnrichRecordsAsync(new List<ConsultationNoteDto> { dto }, ct);
        return ApiResponse<ConsultationNoteDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<List<ConsultationNoteDto>>> GetByAppointmentAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null)
            return ApiResponse<List<ConsultationNoteDto>>.ErrorResponse("Doctor not found");

        var all = await _recordRepo.GetAllAsync(ct);
        var records = all.Where(r => r.AppointmentId == appointmentId && r.DoctorId == doctor.Id).ToList();
        var dtos = _mapper.Map<List<ConsultationNoteDto>>(records);
        await EnrichRecordsAsync(dtos, ct);
        return ApiResponse<List<ConsultationNoteDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<List<ConsultationNoteDto>>> GetByDoctorAsync(Guid doctorUserId, int page, int pageSize, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<List<ConsultationNoteDto>>.ErrorResponse("Doctor not found");

        var all = await _recordRepo.GetAllAsync(ct);
        var records = all.Where(r => r.DoctorId == doctor.Id).ToList();
        var total = records.Count;
        var items = records.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = _mapper.Map<List<ConsultationNoteDto>>(items);
        await EnrichRecordsAsync(dtos, ct);
        return ApiResponse<List<ConsultationNoteDto>>.SuccessResponse(
            dtos,
            pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }
}

public class VerificationService : IVerificationService
{
    private readonly IRepository<VerificationRequest> _verRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public VerificationService(
        IRepository<VerificationRequest> verRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<User> userRepo,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _verRepo = verRepo;
        _doctorRepo = doctorRepo;
        _userRepo = userRepo;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponse<VerificationRequestDto>> SubmitVerificationAsync(Guid doctorUserId, SubmitVerificationDto? dto, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<VerificationRequestDto>.ErrorResponse("Doctor not found");
        if (doctor.VerificationStatus == VerificationStatus.Approved)
            return ApiResponse<VerificationRequestDto>.ErrorResponse("Already verified");

        // Update doctor profile with submitted data
        if (dto != null)
        {
            if (!string.IsNullOrWhiteSpace(dto.LicenseNumber)) doctor.LicenseNumber = dto.LicenseNumber;
            if (!string.IsNullOrWhiteSpace(dto.Specialization)) doctor.ProfessionalTitle = dto.Specialization;
            if (dto.ExperienceYears > 0) doctor.ExperienceYears = dto.ExperienceYears;
            if (!string.IsNullOrWhiteSpace(dto.Education)) doctor.Biography = dto.Education;
        }

        doctor.VerificationStatus = VerificationStatus.Submitted;
        _doctorRepo.Update(doctor);

        var request = new VerificationRequest { DoctorProfileId = doctor.Id, Status = VerificationStatus.Submitted, DoctorProfile = doctor, CertificateUrl = dto?.CertificateUrl };
        await _verRepo.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        var result = await BuildDtoAsync(request, ct);
        return ApiResponse<VerificationRequestDto>.SuccessResponse(result, "Verification submitted");
    }

    public async Task<ApiResponse<VerificationRequestDto>> GetVerificationStatusAsync(Guid doctorUserId, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<VerificationRequestDto>.ErrorResponse("Doctor not found");

        var all = await _verRepo.GetAllAsync(ct);
        var request = all.Where(v => v.DoctorProfileId == doctor.Id).OrderByDescending(v => v.CreatedAt).FirstOrDefault();
        if (request == null) return ApiResponse<VerificationRequestDto>.ErrorResponse("No verification request found");

        var dto = await BuildDtoAsync(request, ct);
        return ApiResponse<VerificationRequestDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<VerificationRequestDto>> GetVerificationByIdAsync(Guid requestId, CancellationToken ct)
    {
        var request = await _verRepo.GetByIdAsync(requestId, ct);
        if (request == null) return ApiResponse<VerificationRequestDto>.ErrorResponse("Verification request not found");

        var dto = await BuildDtoAsync(request, ct);
        return ApiResponse<VerificationRequestDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<List<VerificationRequestDto>>> GetPendingVerificationsAsync(int page, int pageSize, CancellationToken ct)
    {
        return await GetAllVerificationsAsync("Submitted", page, pageSize, ct);
    }

    public async Task<ApiResponse<List<VerificationRequestDto>>> GetAllVerificationsAsync(string? status, int page, int pageSize, CancellationToken ct)
    {
        var all = await _verRepo.GetAllAsync(ct);
        var filtered = all.AsEnumerable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<VerificationStatus>(status, true, out var statusEnum))
            filtered = filtered.Where(v => v.Status == statusEnum);

        var list = filtered.OrderByDescending(v => v.CreatedAt).ToList();
        var total = list.Count;
        var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var dtos = new List<VerificationRequestDto>();
        foreach (var item in items)
            dtos.Add(await BuildDtoAsync(item, ct));

        return ApiResponse<List<VerificationRequestDto>>.SuccessResponse(dtos, pagination: new PaginationMetadata
        {
            Page = page, PageSize = pageSize, TotalItems = total
        });
    }

    public async Task<ApiResponse> ApproveVerificationAsync(Guid requestId, Guid supportUserId, CancellationToken ct)
    {
        var request = await _verRepo.GetByIdAsync(requestId, ct);
        if (request == null) return ApiResponse.ErrorResponse("Request not found");
        request.Status = VerificationStatus.Approved;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedBy = supportUserId;
        _verRepo.Update(request);

        var doctor = await _doctorRepo.GetByIdAsync(request.DoctorProfileId, ct);
        if (doctor != null)
        {
            doctor.VerificationStatus = VerificationStatus.Approved;
            doctor.IsVisible = true;
            _doctorRepo.Update(doctor);

            // Activate the doctor's user account
            var user = await _userRepo.GetByIdAsync(doctor.UserId, ct);
            if (user != null && user.Status != UserStatus.Active)
            {
                user.Status = UserStatus.Active;
                _userRepo.Update(user);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Verification approved");
    }

    public async Task<ApiResponse> RejectVerificationAsync(Guid requestId, Guid supportUserId, string reason, CancellationToken ct)
    {
        var request = await _verRepo.GetByIdAsync(requestId, ct);
        if (request == null) return ApiResponse.ErrorResponse("Request not found");
        request.Status = VerificationStatus.Rejected;
        request.RejectionReason = reason;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedBy = supportUserId;
        _verRepo.Update(request);

        var doctor = await _doctorRepo.GetByIdAsync(request.DoctorProfileId, ct);
        if (doctor != null) { doctor.VerificationStatus = VerificationStatus.Rejected; _doctorRepo.Update(doctor); }

        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Verification rejected");
    }

    /// <summary>Manually builds DTO with doctor profile + reviewer data from repositories</summary>
    private async Task<VerificationRequestDto> BuildDtoAsync(VerificationRequest request, CancellationToken ct)
    {
        var doctor = await _doctorRepo.GetByIdAsync(request.DoctorProfileId, ct);
        User? doctorUser = null;
        if (doctor != null) doctorUser = await _userRepo.GetByIdAsync(doctor.UserId, ct);

        User? reviewer = null;
        if (request.ReviewedBy.HasValue)
            reviewer = await _userRepo.GetByIdAsync(request.ReviewedBy.Value, ct);

        return new VerificationRequestDto
        {
            Id = request.Id,
            DoctorProfileId = request.DoctorProfileId,
            DoctorName = doctorUser?.FullName ?? "Unknown",
            AvatarUrl = doctorUser?.AvatarUrl,
            LicenseNumber = doctor?.LicenseNumber,
            Specialization = doctor?.ProfessionalTitle,
            ExperienceYears = doctor?.ExperienceYears ?? 0,
            Biography = doctor?.Biography,
            Status = request.Status.ToString(),
            RejectionReason = request.RejectionReason,
            ReviewedAt = request.ReviewedAt,
            ReviewedBy = request.ReviewedBy,
            ReviewedByName = reviewer?.FullName,
            CertificateUrl = request.CertificateUrl,
            CreatedAt = request.CreatedAt
        };
    }
}

public class NotificationService : INotificationService
{
    private readonly IRepository<Notification> _notifRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public NotificationService(IRepository<Notification> notifRepo, IUnitOfWork uow, IMapper mapper)
    { _notifRepo = notifRepo; _uow = uow; _mapper = mapper; }

    public async Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(Guid userId, int page, int pageSize, CancellationToken ct)
    {
        var all = await _notifRepo.GetAllAsync(ct);
        var notifs = all.Where(n => n.UserId == userId && !n.IsDeleted).OrderByDescending(n => n.CreatedAt).ToList();
        var total = notifs.Count;
        var items = notifs.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = items.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type.ToString(),
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            ReadAt = n.ReadAt,
            RelatedEntityId = n.RelatedEntityId,
            RelatedEntityType = n.RelatedEntityType
        }).ToList();
        return ApiResponse<List<NotificationDto>>.SuccessResponse(dtos, pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId, CancellationToken ct)
    {
        var all = await _notifRepo.GetAllAsync(ct);
        var count = all.Count(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
        return ApiResponse<int>.SuccessResponse(count);
    }

    public async Task<ApiResponse> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct)
    {
        var notif = await _notifRepo.GetByIdAsync(notificationId, ct);
        if (notif == null || notif.UserId != userId) return ApiResponse.ErrorResponse("Notification not found");
        notif.IsRead = true;
        notif.ReadAt = DateTime.UtcNow;
        _notifRepo.Update(notif);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Marked as read");
    }

    public async Task<ApiResponse> MarkAllAsReadAsync(Guid userId, CancellationToken ct)
    {
        var all = await _notifRepo.GetAllAsync(ct);
        var unread = all.Where(n => n.UserId == userId && !n.IsRead).ToList();
        foreach (var n in unread) { n.IsRead = true; n.ReadAt = DateTime.UtcNow; }
        _notifRepo.UpdateRange(unread);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("All marked as read");
    }

    public async Task CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, Guid? relatedEntityId, string? relatedEntityType, CancellationToken ct)
    {
        var notif = new Notification
        {
            UserId = userId, Title = title, Message = message, Type = type,
            RelatedEntityId = relatedEntityId, RelatedEntityType = relatedEntityType,
            User = null! // Will be resolved by EF via FK
        };
        await _notifRepo.AddAsync(notif, ct);
        await _uow.SaveChangesAsync(ct);
    }
}

public class ServicePackageService : IServicePackageService
{
    private readonly IRepository<ServicePackage> _pkgRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ServicePackageService(IRepository<ServicePackage> pkgRepo, IUnitOfWork uow, IMapper mapper)
    { _pkgRepo = pkgRepo; _uow = uow; _mapper = mapper; }

    public async Task<ApiResponse<List<ServicePackageDto>>> GetActivePackagesAsync(bool includeInactive, CancellationToken ct)
    {
        var all = await _pkgRepo.GetAllAsync(ct);
        var query = all.Where(p => !p.IsDeleted);
        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }
        var list = query.OrderBy(p => p.DisplayOrder).ToList();
        return ApiResponse<List<ServicePackageDto>>.SuccessResponse(_mapper.Map<List<ServicePackageDto>>(list));
    }

    public async Task<ApiResponse<ServicePackageDto>> GetByIdAsync(Guid packageId, CancellationToken ct)
    {
        var pkg = await _pkgRepo.GetByIdAsync(packageId, ct);
        if (pkg == null) return ApiResponse<ServicePackageDto>.ErrorResponse("Package not found");
        return ApiResponse<ServicePackageDto>.SuccessResponse(_mapper.Map<ServicePackageDto>(pkg));
    }

    public async Task<ApiResponse<ServicePackageDto>> CreateAsync(CreateServicePackageDto dto, CancellationToken ct)
    {
        var pkg = new ServicePackage { Name = dto.Name, Description = dto.Description, DurationDays = dto.DurationDays, Price = dto.Price, MaxPatientCapacity = dto.MaxPatientCapacity, MaxDailySlotsCapacity = dto.MaxDailySlotsCapacity, IsFeatured = dto.IsFeatured };
        await _pkgRepo.AddAsync(pkg, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<ServicePackageDto>.SuccessResponse(_mapper.Map<ServicePackageDto>(pkg), "Package created");
    }

    public async Task<ApiResponse<ServicePackageDto>> UpdateAsync(Guid packageId, CreateServicePackageDto dto, CancellationToken ct)
    {
        var pkg = await _pkgRepo.GetByIdAsync(packageId, ct);
        if (pkg == null) return ApiResponse<ServicePackageDto>.ErrorResponse("Package not found");
        pkg.Name = dto.Name; pkg.Description = dto.Description; pkg.DurationDays = dto.DurationDays;
        pkg.Price = dto.Price; pkg.MaxPatientCapacity = dto.MaxPatientCapacity;
        pkg.MaxDailySlotsCapacity = dto.MaxDailySlotsCapacity; pkg.IsFeatured = dto.IsFeatured;
        pkg.UpdatedAt = DateTime.UtcNow;
        _pkgRepo.Update(pkg);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<ServicePackageDto>.SuccessResponse(_mapper.Map<ServicePackageDto>(pkg), "Package updated");
    }

    public async Task<ApiResponse> ToggleActiveAsync(Guid packageId, CancellationToken ct)
    {
        var pkg = await _pkgRepo.GetByIdAsync(packageId, ct);
        if (pkg == null) return ApiResponse.ErrorResponse("Package not found");
        pkg.IsActive = !pkg.IsActive;
        _pkgRepo.Update(pkg);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse(pkg.IsActive ? "Package activated" : "Package deactivated");
    }
}

public class AdminService : IAdminService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<Appointment> _apptRepo;
    private readonly IRepository<AuditLog> _auditRepo;
    private readonly IRepository<Specialization> _specRepo;
    private readonly IRepository<VerificationRequest> _verRepo;
    private readonly IRepository<BlogPost> _blogRepo;
    private readonly IRepository<SystemConfig> _configRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AdminService(IRepository<User> userRepo, IRepository<DoctorProfile> doctorRepo, IRepository<PatientProfile> patientRepo, IRepository<Appointment> apptRepo, IRepository<AuditLog> auditRepo, IRepository<Specialization> specRepo, IRepository<VerificationRequest> verRepo, IRepository<BlogPost> blogRepo, IRepository<SystemConfig> configRepo, IUnitOfWork uow, IMapper mapper)
    { _userRepo = userRepo; _doctorRepo = doctorRepo; _patientRepo = patientRepo; _apptRepo = apptRepo; _auditRepo = auditRepo; _specRepo = specRepo; _verRepo = verRepo; _blogRepo = blogRepo; _configRepo = configRepo; _uow = uow; _mapper = mapper; }

    public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken ct)
    {
        var users = await _userRepo.GetAllAsync(ct);
        var doctors = await _doctorRepo.GetAllAsync(ct);
        var patients = await _patientRepo.GetAllAsync(ct);
        var appts = await _apptRepo.GetAllAsync(ct);
        var vers = await _verRepo.GetAllAsync(ct);
        var blogs = await _blogRepo.GetAllAsync(ct);

        return ApiResponse<DashboardStatsDto>.SuccessResponse(new DashboardStatsDto
        {
            TotalUsers = users.Count(),
            TotalDoctors = doctors.Count(),
            TotalPatients = patients.Count(),
            TotalAppointments = appts.Count(),
            PendingVerifications = vers.Count(v => v.Status == VerificationStatus.Submitted),
            PendingBlogs = blogs.Count(b => b.Status == BlogStatus.Pending)
        });
    }

    public async Task<ApiResponse<List<UserListDto>>> GetUsersAsync(string? search, string? role, int page, int pageSize, CancellationToken ct)
    {
        var all = await _userRepo.GetAllAsync(ct);
        var users = all.Where(u => !u.IsDeleted).ToList();
        if (!string.IsNullOrWhiteSpace(search))
            users = users.Where(u => u.Email.Contains(search, StringComparison.OrdinalIgnoreCase) || u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        var total = users.Count;
        var items = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ApiResponse<List<UserListDto>>.SuccessResponse(_mapper.Map<List<UserListDto>>(items), pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse> LockUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null) return ApiResponse.ErrorResponse("User not found");
        user.Status = UserStatus.Locked;
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("User locked");
    }

    public async Task<ApiResponse> UnlockUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null) return ApiResponse.ErrorResponse("User not found");
        user.Status = UserStatus.Active;
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("User unlocked");
    }

    public async Task<ApiResponse<List<AuditLogDto>>> GetAuditLogsAsync(string? entityName, int page, int pageSize, CancellationToken ct)
    {
        var all = await _auditRepo.GetAllAsync(ct);
        var logs = all.ToList();
        if (!string.IsNullOrWhiteSpace(entityName))
            logs = logs.Where(l => l.EntityName.Contains(entityName, StringComparison.OrdinalIgnoreCase)).ToList();
        var total = logs.Count;
        var items = logs.OrderByDescending(l => l.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ApiResponse<List<AuditLogDto>>.SuccessResponse(_mapper.Map<List<AuditLogDto>>(items), pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<List<SpecializationDto>>> GetSpecializationsAsync(CancellationToken ct)
    {
        var all = await _specRepo.GetAllAsync(ct);
        return ApiResponse<List<SpecializationDto>>.SuccessResponse(_mapper.Map<List<SpecializationDto>>(all.Where(s => !s.IsDeleted).ToList()));
    }

    public async Task<ApiResponse<SpecializationDto>> CreateSpecializationAsync(string name, string? description, CancellationToken ct)
    {
        var spec = new Specialization { Name = name, Description = description };
        await _specRepo.AddAsync(spec, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<SpecializationDto>.SuccessResponse(_mapper.Map<SpecializationDto>(spec), "Specialization created");
    }

    public async Task<ApiResponse<SpecializationDto>> UpdateSpecializationAsync(Guid id, string name, string? description, CancellationToken ct)
    {
        var spec = await _specRepo.GetByIdAsync(id, ct);
        if (spec == null) return ApiResponse<SpecializationDto>.ErrorResponse("Specialization not found");
        spec.Name = name;
        spec.Description = description;
        spec.UpdatedAt = DateTime.UtcNow;
        _specRepo.Update(spec);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<SpecializationDto>.SuccessResponse(_mapper.Map<SpecializationDto>(spec), "Specialization updated");
    }

    public async Task<ApiResponse> DeleteSpecializationAsync(Guid id, CancellationToken ct)
    {
        var spec = await _specRepo.GetByIdAsync(id, ct);
        if (spec == null) return ApiResponse.ErrorResponse("Specialization not found");
        spec.IsDeleted = true;
        _specRepo.Update(spec);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Specialization deleted successfully");
    }

    public async Task<ApiResponse<Dictionary<string, string>>> GetSystemSettingsAsync(CancellationToken ct)
    {
        var configs = await _configRepo.GetAllAsync(ct);
        var dict = configs.ToDictionary(c => c.Key, c => c.Value);
        return ApiResponse<Dictionary<string, string>>.SuccessResponse(dict);
    }

    public async Task<ApiResponse> UpdateSystemSettingsAsync(Dictionary<string, string> settings, CancellationToken ct)
    {
        var configs = await _configRepo.GetAllAsync(ct);
        var configDict = configs.ToDictionary(c => c.Key, c => c);

        foreach (var (key, value) in settings)
        {
            if (configDict.TryGetValue(key, out var config))
            {
                config.Value = value ?? string.Empty;
                _configRepo.Update(config);
            }
            else
            {
                await _configRepo.AddAsync(new SystemConfig { Key = key, Value = value ?? string.Empty }, ct);
            }
        }
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Settings updated successfully");
    }
}

public class TreatmentPackageService : ITreatmentPackageService
{
    private readonly IRepository<TreatmentPackage> _packageRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<TreatmentCase> _caseRepo;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public TreatmentPackageService(
        IRepository<TreatmentPackage> packageRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<User> userRepo,
        IRepository<TreatmentCase> caseRepo,
        INotificationService notificationService,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _packageRepo = packageRepo;
        _doctorRepo = doctorRepo;
        _patientRepo = patientRepo;
        _userRepo = userRepo;
        _caseRepo = caseRepo;
        _notificationService = notificationService;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponse<TreatmentPackageDto>> CreateAsync(Guid doctorUserId, CreateTreatmentPackageDto dto, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null)
            return ApiResponse<TreatmentPackageDto>.ErrorResponse("Doctor not found");

        PatientProfile? patient = null;
        if (dto.PatientId.HasValue && dto.PatientId.Value != Guid.Empty)
        {
            var allPatients = await _patientRepo.GetAllAsync(ct);
            patient = allPatients.FirstOrDefault(p => p.Id == dto.PatientId.Value || p.UserId == dto.PatientId.Value);
            if (patient == null)
                return ApiResponse<TreatmentPackageDto>.ErrorResponse("Patient not found");

            // Constraint: 1 patient can only have 1 active treatment package
            var allPackages = await _packageRepo.GetAllAsync(ct);
            var existingActive = allPackages.FirstOrDefault(p =>
                p.PatientId == dto.PatientId.Value &&
                !p.IsDeleted &&
                p.Status != TreatmentPackageStatus.Completed &&
                p.Status != TreatmentPackageStatus.Cancelled &&
                p.Status != TreatmentPackageStatus.Rejected);
            if (existingActive != null)
                return ApiResponse<TreatmentPackageDto>.ErrorResponse("Bệnh nhân này đã có gói điều trị đang hoạt động. Vui lòng hủy gói cũ trước khi tạo gói mới.");
        }

        var validityDays = dto.ValidityDays > 0 ? dto.ValidityDays : 90;
        var package = new TreatmentPackage
        {
            DoctorId = doctor.Id,
            PatientId = patient?.Id,
            Name = dto.Name,
            Description = dto.Description,
            TargetOutcome = dto.TargetOutcome,
            RecommendedExercises = dto.RecommendedExercises,
            Instructions = dto.Instructions,
            SessionQuantity = dto.SessionQuantity,
            RemainingSessions = dto.SessionQuantity,
            ValidityDays = validityDays,
            ExpirationDate = DateTime.UtcNow.AddDays(validityDays),
            Price = dto.Price,
            Status = patient != null ? TreatmentPackageStatus.Assigned : TreatmentPackageStatus.Created,
            AssignedDate = patient != null ? DateTime.UtcNow : null,
            Doctor = doctor,
            Patient = patient
        };

        await _packageRepo.AddAsync(package, ct);
        await _uow.SaveChangesAsync(ct);
        var createdDto = _mapper.Map<TreatmentPackageDto>(package);
        await EnrichNamesAsync(new List<TreatmentPackageDto> { createdDto }, ct);

        // Notify patient about new treatment package if assigned
        if (patient != null)
        {
            try
            {
                var allUsers = await _userRepo.GetAllAsync(ct);
                var doctorUser = allUsers.FirstOrDefault(u => u.Id == doctorUserId);
                await _notificationService.CreateNotificationAsync(
                    patient.UserId,
                    "📦 New Treatment Package",
                    $"Dr. {doctorUser?.FullName ?? "your doctor"} has created a treatment package \"{dto.Name}\" for you. Please review and confirm.",
                    Domain.Enums.NotificationType.Package,
                    package.Id,
                    "TreatmentPackage",
                    ct);
            }
            catch { }
        }

        return ApiResponse<TreatmentPackageDto>.SuccessResponse(createdDto, patient != null ? "Treatment package created and assigned to patient" : "Template treatment package created successfully");
    }

    /// <summary>Resolve DoctorName/PatientName from User entities (nav props not loaded by generic repo)</summary>
    private async Task EnrichNamesAsync(List<TreatmentPackageDto> dtos, CancellationToken ct)
    {
        if (!dtos.Any()) return;
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var allUsers = await _userRepo.GetAllAsync(ct);
        var userDict = allUsers.ToDictionary(u => u.Id, u => u.FullName);
        var doctorUserMap = allDoctors.ToDictionary(d => d.Id, d => d.UserId);
        var patientUserMap = allPatients.ToDictionary(p => p.Id, p => p.UserId);

        foreach (var dto in dtos)
        {
            if (doctorUserMap.TryGetValue(dto.DoctorId, out var docUserId))
            {
                dto.DoctorProfileId = dto.DoctorId;
                dto.DoctorId = docUserId;
                if (string.IsNullOrEmpty(dto.DoctorName) && userDict.TryGetValue(docUserId, out var docName))
                    dto.DoctorName = docName;
            }
            if (dto.PatientId.HasValue && patientUserMap.TryGetValue(dto.PatientId.Value, out var patUserId))
            {
                dto.PatientId = patUserId;
                if (string.IsNullOrEmpty(dto.PatientName) && userDict.TryGetValue(patUserId, out var patName))
                    dto.PatientName = patName;
            }
        }
    }

    public async Task<ApiResponse<List<TreatmentPackageDto>>> GetByDoctorAsync(Guid doctorUserId, int page, int pageSize, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null)
            return ApiResponse<List<TreatmentPackageDto>>.ErrorResponse("Doctor not found");

        var all = await _packageRepo.GetAllAsync(ct);
        var packages = all.Where(p => p.DoctorId == doctor.Id && !p.IsDeleted).ToList();
        var total = packages.Count;
        var items = packages.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = _mapper.Map<List<TreatmentPackageDto>>(items);
        await EnrichNamesAsync(dtos, ct);
        return ApiResponse<List<TreatmentPackageDto>>.SuccessResponse(dtos, pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<List<TreatmentPackageDto>>> GetByPatientAsync(Guid patientUserId, int page, int pageSize, CancellationToken ct)
    {
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId || p.Id == patientUserId);
        if (patient == null)
            return ApiResponse<List<TreatmentPackageDto>>.ErrorResponse("Patient not found");

        var all = await _packageRepo.GetAllAsync(ct);
        var packages = all.Where(p => p.PatientId == patient.Id && !p.IsDeleted).ToList();
        var total = packages.Count;
        var items = packages.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = _mapper.Map<List<TreatmentPackageDto>>(items);
        await EnrichNamesAsync(dtos, ct);
        return ApiResponse<List<TreatmentPackageDto>>.SuccessResponse(dtos, pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<List<TreatmentPackageDto>>> GetByDoctorAndPatientAsync(Guid doctorUserId, Guid patientUserId, int page, int pageSize, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId || d.Id == doctorUserId);
        if (doctor == null)
            return ApiResponse<List<TreatmentPackageDto>>.ErrorResponse("Doctor not found");

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId || p.Id == patientUserId);
        if (patient == null)
            return ApiResponse<List<TreatmentPackageDto>>.ErrorResponse("Patient not found");

        var all = await _packageRepo.GetAllAsync(ct);
        var packages = all.Where(p => p.DoctorId == doctor.Id && p.PatientId == patient.Id && !p.IsDeleted).ToList();
        var total = packages.Count;
        var items = packages.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = _mapper.Map<List<TreatmentPackageDto>>(items);
        await EnrichNamesAsync(dtos, ct);
        return ApiResponse<List<TreatmentPackageDto>>.SuccessResponse(dtos, pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<TreatmentPackageDto>> GetByIdAsync(Guid packageId, Guid userId, CancellationToken ct)
    {
        var package = await _packageRepo.GetByIdAsync(packageId, ct);
        if (package == null)
            return ApiResponse<TreatmentPackageDto>.ErrorResponse("Treatment package not found");
        var dto = _mapper.Map<TreatmentPackageDto>(package);
        await EnrichNamesAsync(new List<TreatmentPackageDto> { dto }, ct);
        return ApiResponse<TreatmentPackageDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse> AcceptPackageAsync(Guid packageId, Guid patientUserId, CancellationToken ct)
    {
        var package = await _packageRepo.GetByIdAsync(packageId, ct);
        if (package == null)
            return ApiResponse.ErrorResponse("Treatment package not found");

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        if (patient == null || package.PatientId != patient.Id)
            return ApiResponse.ErrorResponse("Not authorized to accept this package");

        if (package.Status != TreatmentPackageStatus.Assigned)
            return ApiResponse.ErrorResponse("Only assigned packages can be accepted");

        package.Status = TreatmentPackageStatus.Active;
        package.AcceptedDate = DateTime.UtcNow;
        package.ActiveDate = DateTime.UtcNow;
        package.UpdatedAt = DateTime.UtcNow;
        _packageRepo.Update(package);

        // Auto-create Treatment Case when package becomes Active
        var allCases = await _caseRepo.GetAllAsync(ct);
        var existingCase = allCases.FirstOrDefault(c =>
            c.TreatmentPackageId == package.Id &&
            c.DoctorId == package.DoctorId &&
            c.PatientId == patient.Id &&
            !c.IsDeleted);

        if (existingCase == null)
        {
            var treatmentCase = new TreatmentCase
            {
                TreatmentPackageId = package.Id,
                DoctorId = package.DoctorId,
                PatientId = patient.Id,
                CaseName = package.Name,
                CaseDescription = package.Description,
                PrimaryConcern = package.TargetOutcome,
                TotalSessions = package.SessionQuantity,
                RemainingSessions = package.SessionQuantity,
                StartDate = DateTime.UtcNow,
                ExpectedEndDate = DateTime.UtcNow.AddDays(package.ValidityDays),
                Status = TreatmentCaseStatus.Active,
                TreatmentPackage = package,
                Doctor = (await _doctorRepo.GetByIdAsync(package.DoctorId, ct))!,
                Patient = patient
            };
            await _caseRepo.AddAsync(treatmentCase, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Treatment package accepted and treatment case created.");
    }

    public async Task<ApiResponse> RejectPackageAsync(Guid packageId, Guid patientUserId, string? reason, CancellationToken ct)
    {
        var package = await _packageRepo.GetByIdAsync(packageId, ct);
        if (package == null)
            return ApiResponse.ErrorResponse("Treatment package not found");

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        if (patient == null || package.PatientId != patient.Id)
            return ApiResponse.ErrorResponse("Not authorized to reject this package");

        if (package.Status != TreatmentPackageStatus.Assigned)
            return ApiResponse.ErrorResponse("Only assigned packages can be rejected");

        package.Status = TreatmentPackageStatus.Rejected;
        package.RejectionReason = reason;
        package.UpdatedAt = DateTime.UtcNow;
        _packageRepo.Update(package);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Treatment package rejected");
    }

    public async Task<ApiResponse> CancelPackageAsync(Guid packageId, Guid userId, string? reason, CancellationToken ct)
    {
        var package = await _packageRepo.GetByIdAsync(packageId, ct);
        if (package == null)
            return ApiResponse.ErrorResponse("Treatment package not found");

        // Check authorization - allow both doctor (owner) and patient (assigned)
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == userId);
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == userId);

        bool isDoctor = doctor != null && package.DoctorId == doctor.Id;
        bool isPatient = patient != null && package.PatientId == patient.Id;

        if (!isDoctor && !isPatient)
            return ApiResponse.ErrorResponse("Not authorized to cancel this package");

        // Only non-completed, non-cancelled packages can be cancelled
        if (package.Status == TreatmentPackageStatus.Completed || package.Status == TreatmentPackageStatus.Cancelled)
            return ApiResponse.ErrorResponse("This package cannot be cancelled");

        package.Status = TreatmentPackageStatus.Cancelled;
        package.RejectionReason = reason ?? "Đã hủy bởi " + (isDoctor ? "bác sĩ" : "bệnh nhân");
        package.UpdatedAt = DateTime.UtcNow;
        _packageRepo.Update(package);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Treatment package cancelled successfully");
    }
}

public class SubscriptionService : ISubscriptionService
{
    private readonly IRepository<DoctorSubscription> _subRepo;
    private readonly IRepository<ServicePackage> _pkgRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<PaymentTransaction> _paymentRepo;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public SubscriptionService(
        IRepository<DoctorSubscription> subRepo,
        IRepository<ServicePackage> pkgRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<PaymentTransaction> paymentRepo,
        IPaymentService paymentService,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _subRepo = subRepo;
        _pkgRepo = pkgRepo;
        _doctorRepo = doctorRepo;
        _paymentRepo = paymentRepo;
        _paymentService = paymentService;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponse<SubscriptionDto>> PurchaseAsync(Guid doctorUserId, Guid servicePackageId, string returnUrl, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<SubscriptionDto>.ErrorResponse("Doctor not found");

        var package = await _pkgRepo.GetByIdAsync(servicePackageId, ct);
        if (package == null || !package.IsActive)
            return ApiResponse<SubscriptionDto>.ErrorResponse("Service package not found or inactive");

        // FREE PACKAGE — activate immediately without VNPay
        if (package.Price <= 0)
        {
            // Deactivate existing active subscriptions
            var allSubs = await _subRepo.GetAllAsync(ct);
            var activeSubs = allSubs.Where(s => s.DoctorProfileId == doctor.Id && s.Status == SubscriptionStatus.Active).ToList();
            foreach (var activeSub in activeSubs)
            {
                activeSub.Status = SubscriptionStatus.Expired;
                _subRepo.Update(activeSub);
            }

            var freeSub = new DoctorSubscription
            {
                DoctorProfileId = doctor.Id,
                ServicePackageId = servicePackageId,
                Status = SubscriptionStatus.Active,
                StartDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddDays(package.DurationDays),
                DoctorProfile = doctor,
                ServicePackage = package
            };
            await _subRepo.AddAsync(freeSub, ct);

            var freePayment = new PaymentTransaction
            {
                DoctorSubscriptionId = freeSub.Id,
                TransactionCode = $"TXN-FREE-{Guid.NewGuid():N}",
                Amount = 0,
                PaymentMethod = "Free",
                PaymentStatus = Domain.Enums.PaymentStatus.Success,
                PaidAt = DateTime.UtcNow,
                DoctorSubscription = freeSub
            };
            await _paymentRepo.AddAsync(freePayment, ct);
            await _uow.SaveChangesAsync(ct);

            return ApiResponse<SubscriptionDto>.SuccessResponse(new SubscriptionDto
            {
                Id = freeSub.Id,
                PackageName = package.Name,
                Status = freeSub.Status.ToString(),
                StartDate = freeSub.StartDate,
                ExpirationDate = freeSub.ExpirationDate,
                CreatedAt = freeSub.CreatedAt
            }, "Free trial activated successfully!");
        }

        // PAID PACKAGE — use VNPay
        var subscription = new DoctorSubscription
        {
            DoctorProfileId = doctor.Id,
            ServicePackageId = servicePackageId,
            Status = SubscriptionStatus.PendingPayment,
            StartDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(package.DurationDays),
            DoctorProfile = doctor,
            ServicePackage = package
        };

        await _subRepo.AddAsync(subscription, ct);

        // Create payment transaction record
        var payment = new PaymentTransaction
        {
            DoctorSubscriptionId = subscription.Id,
            TransactionCode = $"TXN-{Guid.NewGuid():N}",
            Amount = package.Price,
            PaymentMethod = "VNPay",
            PaymentStatus = Domain.Enums.PaymentStatus.Pending,
            DoctorSubscription = subscription
        };
        await _paymentRepo.AddAsync(payment, ct);
        await _uow.SaveChangesAsync(ct);

        // Generate VNPay redirect URL
        string paymentUrl = await _paymentService.CreatePaymentUrlAsync(
            subscription.Id,
            package.Price,
            $"Subscribe {package.Name} package",
            returnUrl,
            ct);

        return ApiResponse<SubscriptionDto>.SuccessResponse(new SubscriptionDto
        {
            Id = subscription.Id,
            PackageName = package.Name,
            Status = subscription.Status.ToString(),
            StartDate = subscription.StartDate,
            ExpirationDate = subscription.ExpirationDate,
            CreatedAt = subscription.CreatedAt,
            PaymentUrl = paymentUrl
        }, "Subscription created. Redirect to VNPay for payment.");
    }

    public async Task<ApiResponse<SubscriptionDto>> CreateSubscriptionDirectAsync(Guid doctorUserId, Guid servicePackageId, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<SubscriptionDto>.ErrorResponse("Doctor not found");

        var package = await _pkgRepo.GetByIdAsync(servicePackageId, ct);
        if (package == null || !package.IsActive)
            return ApiResponse<SubscriptionDto>.ErrorResponse("Service package not found or inactive");

        // Deactivate any existing active subscriptions for this doctor
        var allSubs = await _subRepo.GetAllAsync(ct);
        var activeSubs = allSubs.Where(s => s.DoctorProfileId == doctor.Id && s.Status == SubscriptionStatus.Active).ToList();
        foreach (var activeSub in activeSubs)
        {
            activeSub.Status = SubscriptionStatus.Expired;
            _subRepo.Update(activeSub);
        }

        var subscription = new DoctorSubscription
        {
            DoctorProfileId = doctor.Id,
            ServicePackageId = servicePackageId,
            Status = SubscriptionStatus.Active, // Active immediately
            StartDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(package.DurationDays),
            DoctorProfile = doctor,
            ServicePackage = package
        };

        await _subRepo.AddAsync(subscription, ct);

        // Create payment transaction record as Paid
        var payment = new PaymentTransaction
        {
            DoctorSubscriptionId = subscription.Id,
            TransactionCode = $"TXN-{Guid.NewGuid():N}",
            Amount = package.Price,
            PaymentMethod = "VNPay (Direct Active)",
            PaymentStatus = Domain.Enums.PaymentStatus.Success,
            DoctorSubscription = subscription
        };
        await _paymentRepo.AddAsync(payment, ct);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<SubscriptionDto>.SuccessResponse(new SubscriptionDto
        {
            Id = subscription.Id,
            PackageName = package.Name,
            Status = subscription.Status.ToString(),
            StartDate = subscription.StartDate,
            ExpirationDate = subscription.ExpirationDate,
            CreatedAt = subscription.CreatedAt
        }, "Subscription created and activated successfully.");
    }

    public async Task<ApiResponse<SubscriptionDto>> GetActiveSubscriptionAsync(Guid doctorUserId, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<SubscriptionDto>.ErrorResponse("Doctor not found");

        var allSubs = await _subRepo.GetAllAsync(ct);
        var activeSub = allSubs.FirstOrDefault(s =>
            s.DoctorProfileId == doctor.Id &&
            s.Status == SubscriptionStatus.Active &&
            s.ExpirationDate > DateTime.UtcNow);

        if (activeSub == null) return ApiResponse<SubscriptionDto>.ErrorResponse("No active subscription");

        var pkg = await _pkgRepo.GetByIdAsync(activeSub.ServicePackageId, ct);
        return ApiResponse<SubscriptionDto>.SuccessResponse(new SubscriptionDto
        {
            Id = activeSub.Id,
            PackageName = pkg?.Name ?? "Unknown",
            Status = activeSub.Status.ToString(),
            StartDate = activeSub.StartDate,
            ExpirationDate = activeSub.ExpirationDate,
            CreatedAt = activeSub.CreatedAt
        });
    }

    public async Task<ApiResponse<List<SubscriptionDto>>> GetSubscriptionHistoryAsync(Guid doctorUserId, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<List<SubscriptionDto>>.ErrorResponse("Doctor not found");

        var allSubs = await _subRepo.GetAllAsync(ct);
        var subs = allSubs.Where(s => s.DoctorProfileId == doctor.Id).OrderByDescending(s => s.CreatedAt).ToList();

        var dtos = new List<SubscriptionDto>();
        foreach (var sub in subs)
        {
            var pkg = await _pkgRepo.GetByIdAsync(sub.ServicePackageId, ct);
            dtos.Add(new SubscriptionDto
            {
                Id = sub.Id,
                PackageName = pkg?.Name ?? "Unknown",
                Status = sub.Status.ToString(),
                StartDate = sub.StartDate,
                ExpirationDate = sub.ExpirationDate,
                CreatedAt = sub.CreatedAt
            });
        }

        return ApiResponse<List<SubscriptionDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse> ProcessPaymentCallbackAsync(IDictionary<string, string> queryParams, CancellationToken ct)
    {
        var paymentResult = await _paymentService.ProcessCallbackAsync(queryParams, ct);
        if (!paymentResult.IsSuccess || string.IsNullOrEmpty(paymentResult.TransactionCode))
        {
            return ApiResponse.ErrorResponse(paymentResult.Message ?? "Invalid payment callback");
        }

        if (Guid.TryParse(paymentResult.TransactionCode, out var subscriptionId))
        {
            var sub = await _subRepo.GetByIdAsync(subscriptionId, ct);
            if (sub != null && sub.Status == SubscriptionStatus.PendingPayment)
            {
                // Deactivate any existing active subscriptions for this doctor
                var allSubs = await _subRepo.GetAllAsync(ct);
                var activeSubs = allSubs.Where(s => s.DoctorProfileId == sub.DoctorProfileId && s.Status == SubscriptionStatus.Active).ToList();
                foreach (var activeSub in activeSubs)
                {
                    activeSub.Status = SubscriptionStatus.Expired;
                    activeSub.UpdatedAt = DateTime.UtcNow;
                    _subRepo.Update(activeSub);
                }

                sub.Status = SubscriptionStatus.Active;
                sub.UpdatedAt = DateTime.UtcNow;
                _subRepo.Update(sub);

                // Update payment transaction record status to success
                var allPayments = await _paymentRepo.GetAllAsync(ct);
                var txn = allPayments.FirstOrDefault(p => p.DoctorSubscriptionId == subscriptionId);
                if (txn != null)
                {
                    txn.PaymentStatus = Domain.Enums.PaymentStatus.Success;
                    txn.TransactionCode = paymentResult.TransactionCode;
                    txn.UpdatedAt = DateTime.UtcNow;
                    _paymentRepo.Update(txn);
                }

                await _uow.SaveChangesAsync(ct);
                return ApiResponse.SuccessResponse("Payment processed, subscription activated");
            }
        }

        return ApiResponse.ErrorResponse("Invalid payment callback");
    }
}
