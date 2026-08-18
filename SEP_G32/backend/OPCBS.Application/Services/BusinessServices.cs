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
    private readonly IFavoriteDoctorNotificationService? _favoriteNotificationService;

    public BlogService(
        IRepository<BlogPost> blogRepo,
        IRepository<BlogComment> commentRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<User> userRepo,
        IUnitOfWork uow,
        IMapper mapper,
        IFavoriteDoctorNotificationService? favoriteNotificationService = null)
    {
        _blogRepo = blogRepo;
        _commentRepo = commentRepo;
        _doctorRepo = doctorRepo;
        _userRepo = userRepo;
        _uow = uow;
        _mapper = mapper;
        _favoriteNotificationService = favoriteNotificationService;
    }

    public async Task<ApiResponse<List<BlogPostDto>>> GetPublishedBlogsAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var all = await _blogRepo.GetAllAsync(ct);
        var blogs = all.Where(b => b.Status == BlogStatus.Published && !b.IsDeleted).ToList();
        if (!string.IsNullOrWhiteSpace(search))
            blogs = blogs.Where(b => b.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        var total = blogs.Count;
        var items = blogs.OrderByDescending(b => b.PublishedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = _mapper.Map<List<BlogPostDto>>(items);
        await EnrichBlogAuthorsAsync(dtos, items, ct);
        return ApiResponse<List<BlogPostDto>>.SuccessResponse(dtos, pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<BlogPostDto>> GetBlogByIdAsync(Guid blogId, CancellationToken ct)
    {
        var blog = await _blogRepo.GetByIdAsync(blogId, ct);
        if (blog == null) return ApiResponse<BlogPostDto>.ErrorResponse("Blog not found");
        blog.ViewCount++;
        _blogRepo.Update(blog);
        await _uow.SaveChangesAsync(ct);
        var dto = _mapper.Map<BlogPostDto>(blog);
        await EnrichBlogAuthorsAsync(new List<BlogPostDto> { dto }, new List<BlogPost> { blog }, ct);
        return ApiResponse<BlogPostDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<BlogPostDto>> CreateBlogAsync(Guid doctorUserId, CreateBlogPostDto dto, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<BlogPostDto>.ErrorResponse("Doctor not found");

        var thumbnailUrl = !string.IsNullOrWhiteSpace(dto.ThumbnailUrl)
            ? dto.ThumbnailUrl
            : "https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?w=800";

        var blog = new BlogPost { DoctorId = doctor.Id, Title = dto.Title, Content = dto.Content, ThumbnailUrl = thumbnailUrl, Excerpt = dto.Excerpt, Doctor = doctor };
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
        if (dto.ThumbnailUrl != null)
        {
            blog.ThumbnailUrl = !string.IsNullOrWhiteSpace(dto.ThumbnailUrl)
                ? dto.ThumbnailUrl
                : "https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?w=800";
        }
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

    public async Task<ApiResponse<List<BlogPostDto>>> GetDoctorBlogsAsync(Guid doctorUserId, int page, int pageSize, string? status, string? search, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<List<BlogPostDto>>.ErrorResponse("Doctor not found");
        var all = await _blogRepo.GetAllAsync(ct);
        var blogs = all.Where(b => b.DoctorId == doctor.Id && !b.IsDeleted).ToList();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Equals("PendingApproval", StringComparison.OrdinalIgnoreCase)
                ? BlogStatus.Pending.ToString()
                : status;
            if (Enum.TryParse<BlogStatus>(normalizedStatus, true, out var requestedStatus))
                blogs = blogs.Where(b => b.Status == requestedStatus).ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            blogs = blogs.Where(b =>
                b.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(b.Excerpt) && b.Excerpt.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                b.Content.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var total = blogs.Count;
        var items = blogs.OrderByDescending(b => b.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = _mapper.Map<List<BlogPostDto>>(items);
        await EnrichBlogAuthorsAsync(dtos, items, ct);
        return ApiResponse<List<BlogPostDto>>.SuccessResponse(dtos, pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<List<BlogPostDto>>> GetPendingBlogsAsync(int page, int pageSize, CancellationToken ct)
    {
        var all = await _blogRepo.GetAllAsync(ct);
        var blogs = all.Where(b => b.Status == BlogStatus.Pending).ToList();
        var total = blogs.Count;
        var items = blogs.OrderBy(b => b.SubmittedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = _mapper.Map<List<BlogPostDto>>(items);
        await EnrichBlogAuthorsAsync(dtos, items, ct);
        return ApiResponse<List<BlogPostDto>>.SuccessResponse(dtos, pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    private async Task EnrichBlogAuthorsAsync(List<BlogPostDto> dtos, List<BlogPost> sourceBlogs, CancellationToken ct)
    {
        if (!dtos.Any() || !sourceBlogs.Any()) return;

        var doctors = await _doctorRepo.GetAllAsync(ct);
        var users = await _userRepo.GetAllAsync(ct);
        var doctorById = doctors.Where(d => !d.IsDeleted).ToDictionary(d => d.Id);
        var userById = users.ToDictionary(u => u.Id);
        var sourceById = sourceBlogs.ToDictionary(b => b.Id);

        foreach (var dto in dtos)
        {
            if (string.IsNullOrWhiteSpace(dto.AuthorName))
                dto.AuthorName = "OPCBS Specialist";

            if (!sourceById.TryGetValue(dto.Id, out var source) ||
                !doctorById.TryGetValue(source.DoctorId, out var doctor))
            {
                continue;
            }

            dto.AuthorId = doctor.Id;
            dto.AuthorExperienceYears = doctor.ExperienceYears;
            dto.AuthorIsVerified = doctor.VerificationStatus == VerificationStatus.Approved;
            dto.AuthorProfessionalTitle = doctor.ProfessionalTitle ?? dto.AuthorProfessionalTitle;

            if (userById.TryGetValue(doctor.UserId, out var user))
            {
                dto.AuthorName = string.IsNullOrWhiteSpace(user.FullName) ? dto.AuthorName : user.FullName;
                dto.AuthorAvatarUrl = string.IsNullOrWhiteSpace(user.AvatarUrl) ? dto.AuthorAvatarUrl : user.AvatarUrl;
            }
        }
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

        if (_favoriteNotificationService != null)
        {
            try
            {
                var doctor = (await _doctorRepo.GetAllAsync(ct)).FirstOrDefault(d => d.Id == blog.DoctorId);
                if (doctor != null)
                {
                    var doctorName = (await _userRepo.GetAllAsync(ct))
                        .FirstOrDefault(u => u.Id == doctor.UserId)?.FullName ?? "your favorite doctor";
                    await _favoriteNotificationService.NotifyFollowersAsync(
                        doctor.Id,
                        doctor.UserId,
                        doctorName,
                        $"New post from Dr. {doctorName}",
                        $"Dr. {doctorName} published a new post: {blog.Title}",
                        blog.Id,
                        "BlogPost",
                        ct);
                }
            }
            catch
            {
                // Publishing remains successful even if notification delivery is unavailable.
            }
        }

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
    private readonly IRepository<CustomClinicalField>? _customFieldRepo;
    private readonly IRepository<AppointmentSlot>? _slotRepo;
    private readonly IRepository<AppointmentHistory>? _historyRepo;
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
        IMapper mapper,
        IRepository<CustomClinicalField>? customFieldRepo = null,
        IRepository<AppointmentSlot>? slotRepo = null,
        IRepository<AppointmentHistory>? historyRepo = null)
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
        _customFieldRepo = customFieldRepo;
        _slotRepo = slotRepo;
        _historyRepo = historyRepo;
    }

    private async Task EnrichRecordsAsync(List<ConsultationNoteDto>? dtos, CancellationToken ct)
    {
        if (dtos == null || !dtos.Any()) return;
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
                dto.IsPatientConfirmed = entity.IsPatientConfirmed;
                dto.PatientConfirmedAt = entity.PatientConfirmedAt;
                dto.PatientConfirmedById = entity.PatientConfirmedById;
                dto.LastEditedAt = entity.LastEditedAt;
                dto.LastEditedByDoctorId = entity.LastEditedByDoctorId;
                dto.FollowUpAppointmentId = entity.FollowUpAppointmentId;
                dto.NextAppointmentRecommendedSlotId = entity.NextAppointmentRecommendedSlotId;
                dto.NextAppointmentRecommendedDate = entity.NextAppointmentRecommendedDate;

                if (entity.PatientConfirmedById.HasValue && userDict.TryGetValue(entity.PatientConfirmedById.Value, out var confName))
                {
                    dto.PatientConfirmedByName = confName;
                }
            }

            if (dto.FollowUpAppointmentId.HasValue)
            {
                var allAppts = await _apptRepo.GetAllAsync(ct);
                var fAppt = allAppts.FirstOrDefault(a => a.Id == dto.FollowUpAppointmentId.Value);
                if (fAppt != null)
                {
                    dto.FollowUpAppointmentBookingCode = fAppt.BookingCode;
                }
            }

            if (dto.NextAppointmentRecommendedSlotId.HasValue && _slotRepo != null)
            {
                var slot = await _slotRepo.GetByIdAsync(dto.NextAppointmentRecommendedSlotId.Value, ct);
                if (slot != null)
                {
                    dto.NextAppointmentRecommendedSlotStartTime = slot.StartTime.ToString(@"hh\:mm");
                    dto.NextAppointmentRecommendedSlotEndTime = slot.EndTime.ToString(@"hh\:mm");
                }
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

        if (_customFieldRepo != null && dtos.Any())
        {
            try
            {
                var allFields = await _customFieldRepo.GetAllAsync(ct);
                var fieldsByNote = allFields.Where(f => f.OwnerType == "ConsultationNote")
                    .GroupBy(f => f.OwnerId)
                    .ToDictionary(g => g.Key, g => g.OrderBy(f => f.OrderIndex).Select(f => new CustomClinicalFieldDto
                    {
                        Id = f.Id,
                        OwnerType = f.OwnerType,
                        OwnerId = f.OwnerId,
                        SectionKey = f.SectionKey,
                        Title = f.Title,
                        Content = f.Content,
                        FieldType = f.FieldType,
                        OrderIndex = f.OrderIndex,
                        CreatedByDoctorId = f.CreatedByDoctorId
                    }).ToList());

                foreach (var dto in dtos)
                {
                    if (fieldsByNote.TryGetValue(dto.Id, out var fields))
                    {
                        dto.CustomFields = fields;
                    }
                }
            }
            catch { }
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

        Appointment? appointment = null;
        if (dto.AppointmentId.HasValue)
        {
            appointment = await _apptRepo.GetByIdAsync(dto.AppointmentId.Value, ct);
        }

        // Auto-create PatientRecord if missing (e.g., guest booking)
        if (patientRecord == null && appointment != null)
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

        if (patientRecord == null) return ApiResponse<ConsultationNoteDto>.ErrorResponse("Could not resolve or create patient record: Patient record not found");

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
            NextAppointmentRecommendedSlotId = dto.NextAppointmentRecommendedSlotId,
            ConsultationDate = dto.ConsultationDate,
            Visibility = (NoteVisibility)dto.Visibility,
            Doctor = doctor,
            PatientRecord = patientRecord,
            IsPatientConfirmed = false,
            PatientConfirmedAt = null,
            LastEditedAt = DateTime.UtcNow,
            LastEditedByDoctorId = doctor.Id
        };

        if (appointment != null)
        {
            record.Appointment = appointment;
            if (!record.ConsultationDate.HasValue)
                record.ConsultationDate = appointment.AppointmentDate;
        }

        // Auto-create follow-up appointment if slot was selected
        if (dto.NextAppointmentRecommendedSlotId.HasValue && _slotRepo != null)
        {
            var recSlot = await _slotRepo.GetByIdAsync(dto.NextAppointmentRecommendedSlotId.Value, ct);
            if (recSlot != null)
            {
                var slotDateTime = recSlot.SlotDate.ToDateTime(recSlot.StartTime);
                var bookingCode = $"OPCBS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

                var followUpAppt = new Appointment
                {
                    BookingCode = bookingCode,
                    AppointmentSlotId = recSlot.Id,
                    DoctorId = doctor.Id,
                    PatientId = appointment?.PatientId ?? patientRecord.PatientId,
                    GuestName = appointment?.GuestName ?? patientRecord.GuestName,
                    GuestEmail = appointment?.GuestEmail ?? patientRecord.GuestEmail,
                    GuestPhoneNumber = appointment?.GuestPhoneNumber ?? patientRecord.GuestPhone,
                    Notes = string.IsNullOrWhiteSpace(dto.FollowUpNotes)
                        ? $"Follow-up appointment recommended from session {appointment?.BookingCode ?? ""}".Trim()
                        : dto.FollowUpNotes.Trim(),
                    AppointmentDate = slotDateTime,
                    TreatmentPackageId = appointment?.TreatmentPackageId,
                    TreatmentCaseId = appointment?.TreatmentCaseId,
                    ConsultationMode = recSlot.ConsultationMode,
                    Status = AppointmentStatus.Pending,
                    AppointmentSlot = recSlot,
                    Doctor = doctor
                };

                await _apptRepo.AddAsync(followUpAppt, ct);

                recSlot.CurrentBookings++;
                if (recSlot.CurrentBookings >= recSlot.MaxPatients)
                    recSlot.Status = AppointmentSlotStatus.Booked;
                _slotRepo.Update(recSlot);

                record.FollowUpAppointmentId = followUpAppt.Id;
                record.FollowUpAppointment = followUpAppt;

                if (_historyRepo != null)
                {
                    await _historyRepo.AddAsync(new AppointmentHistory
                    {
                        AppointmentId = followUpAppt.Id,
                        NewStatus = AppointmentStatus.Pending,
                        Reason = $"Follow-up appointment auto-created from consultation record {appointment?.BookingCode ?? ""}",
                        ChangedByUserId = doctorUserId,
                        ChangedByRole = "Doctor",
                        Appointment = followUpAppt
                    }, ct);
                }

                if (followUpAppt.PatientId.HasValue)
                {
                    try
                    {
                        var allPatients = await _patientRepo.GetAllAsync(ct);
                        var pat = allPatients.FirstOrDefault(p => p.Id == followUpAppt.PatientId.Value);
                        if (pat != null)
                        {
                            var allUsers = await _userRepo.GetAllAsync(ct);
                            var doctorUser = allUsers.FirstOrDefault(u => u.Id == doctorUserId);
                            await _notificationService.CreateNotificationAsync(
                                pat.UserId,
                                "📅 Follow-up Appointment Scheduled",
                                $"Dr. {doctorUser?.FullName ?? "your doctor"} has scheduled a follow-up appointment for you on {recSlot.SlotDate:MMM dd, yyyy} at {recSlot.StartTime:hh\\:mm}. Please review your appointment details.",
                                Domain.Enums.NotificationType.Appointment,
                                followUpAppt.Id,
                                "Appointment",
                                ct);
                        }
                    }
                    catch { }
                }
            }
        }

        await _recordRepo.AddAsync(record, ct);
        await _uow.SaveChangesAsync(ct);

        if (appointment != null && appointment.PatientId.HasValue)
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
                        record.Id,
                        "ConsultationNote",
                        ct);
                }
            }
            catch { }
        }

        if (_customFieldRepo != null && dto.CustomFields != null && dto.CustomFields.Any())
        {
            int idx = 0;
            foreach (var cf in dto.CustomFields)
            {
                if (!string.IsNullOrWhiteSpace(cf.Title))
                {
                    await _customFieldRepo.AddAsync(new CustomClinicalField
                    {
                        OwnerType = "ConsultationNote",
                        OwnerId = record.Id,
                        SectionKey = string.IsNullOrWhiteSpace(cf.SectionKey) ? "ConsultationNote" : cf.SectionKey,
                        Title = cf.Title,
                        Content = cf.Content,
                        FieldType = cf.FieldType ?? "Text",
                        OrderIndex = cf.OrderIndex > 0 ? cf.OrderIndex : idx++,
                        CreatedByDoctorId = doctor.Id
                    }, ct);
                }
            }
            await _uow.SaveChangesAsync(ct);
        }

        var createdLinkedDto = _mapper.Map<ConsultationNoteDto>(record);
        await EnrichRecordsAsync(new List<ConsultationNoteDto> { createdLinkedDto }, ct);

        return ApiResponse<ConsultationNoteDto>.SuccessResponse(createdLinkedDto, "Record created");
    }

    public async Task<ApiResponse<ConsultationNoteDto>> UpdateAsync(Guid recordId, Guid doctorUserId, UpdateConsultationNoteDto dto, CancellationToken ct)
    {
        var record = await _recordRepo.GetByIdAsync(recordId, ct);
        if (record == null) return ApiResponse<ConsultationNoteDto>.ErrorResponse("Record not found");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || record.DoctorId != doctor.Id)
        {
            return ApiResponse<ConsultationNoteDto>.ErrorResponse("You are not authorized to update this consultation record.");
        }

        if (record.IsPatientConfirmed)
        {
            return ApiResponse<ConsultationNoteDto>.ErrorResponse("This consultation record has been confirmed by the patient and can no longer be edited.");
        }

        record.ConsultationSummary = dto.ConsultationSummary;
        record.Diagnosis = dto.Diagnosis;
        record.Recommendation = dto.Recommendation;
        record.FollowUpNotes = dto.FollowUpNotes;
        record.TherapyPlan = dto.TherapyPlan;
        record.Visibility = (NoteVisibility)dto.Visibility;
        if (!record.AppointmentId.HasValue && dto.ConsultationDate.HasValue)
            record.ConsultationDate = dto.ConsultationDate;

        record.LastEditedAt = DateTime.UtcNow;
        record.LastEditedByDoctorId = doctor.Id;
        record.UpdatedAt = DateTime.UtcNow;

        _recordRepo.Update(record);

        if (_customFieldRepo != null && dto.CustomFields != null)
        {
            try
            {
                var existingFields = (await _customFieldRepo.GetAllAsync(ct))
                    .Where(f => f.OwnerType == "ConsultationNote" && f.OwnerId == record.Id)
                    .ToList();
                foreach (var f in existingFields)
                {
                    _customFieldRepo.Delete(f);
                }
                int idx = 0;
                foreach (var cf in dto.CustomFields)
                {
                    if (!string.IsNullOrWhiteSpace(cf.Title))
                    {
                        await _customFieldRepo.AddAsync(new CustomClinicalField
                        {
                            OwnerType = "ConsultationNote",
                            OwnerId = record.Id,
                            SectionKey = string.IsNullOrWhiteSpace(cf.SectionKey) ? "ConsultationNote" : cf.SectionKey,
                            Title = cf.Title,
                            Content = cf.Content,
                            FieldType = cf.FieldType ?? "Text",
                            OrderIndex = cf.OrderIndex > 0 ? cf.OrderIndex : idx++,
                            CreatedByDoctorId = doctor.Id
                        }, ct);
                    }
                }
            }
            catch { }
        }

        await _uow.SaveChangesAsync(ct);
        var updatedDto = _mapper.Map<ConsultationNoteDto>(record);
        await EnrichRecordsAsync(new List<ConsultationNoteDto> { updatedDto }, ct);
        return ApiResponse<ConsultationNoteDto>.SuccessResponse(updatedDto, "Record updated");
    }

    public async Task<ApiResponse<ConsultationNoteDto>> ConfirmByPatientAsync(Guid recordId, Guid patientUserId, CancellationToken ct = default)
    {
        var record = await _recordRepo.GetByIdAsync(recordId, ct);
        if (record == null) return ApiResponse<ConsultationNoteDto>.ErrorResponse("Consultation record not found.");

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        var patientRecord = await _patientRecordRepo.GetByIdAsync(record.PatientRecordId, ct);

        bool isOwnedByPatient = false;
        if (patient != null)
        {
            if (patientRecord != null && patientRecord.PatientId == patient.Id)
            {
                isOwnedByPatient = true;
            }
            else if (record.AppointmentId.HasValue)
            {
                var appointment = await _apptRepo.GetByIdAsync(record.AppointmentId.Value, ct);
                if (appointment != null && appointment.PatientId == patient.Id)
                {
                    isOwnedByPatient = true;
                }
            }
        }

        if (!isOwnedByPatient)
        {
            return ApiResponse<ConsultationNoteDto>.ErrorResponse("You are not authorized to confirm this consultation record.");
        }

        if (record.IsPatientConfirmed)
        {
            var existingDto = _mapper.Map<ConsultationNoteDto>(record);
            await EnrichRecordsAsync(new List<ConsultationNoteDto> { existingDto }, ct);
            return ApiResponse<ConsultationNoteDto>.SuccessResponse(existingDto, "Consultation notes confirmed successfully.");
        }

        record.IsPatientConfirmed = true;
        record.PatientConfirmedAt = DateTime.UtcNow;
        record.PatientConfirmedById = patientUserId;
        record.UpdatedAt = DateTime.UtcNow;

        _recordRepo.Update(record);
        await _uow.SaveChangesAsync(ct);

        var confirmedDto = _mapper.Map<ConsultationNoteDto>(record);
        await EnrichRecordsAsync(new List<ConsultationNoteDto> { confirmedDto }, ct);

        try
        {
            var doctor = await _doctorRepo.GetByIdAsync(record.DoctorId, ct);
            if (doctor != null)
            {
                var allUsers = await _userRepo.GetAllAsync(ct);
                var patientUser = allUsers.FirstOrDefault(u => u.Id == patientUserId);
                await _notificationService.CreateNotificationAsync(
                    doctor.UserId,
                    "✅ Consultation Notes Confirmed",
                    $"{patientUser?.FullName ?? "The patient"} has reviewed and confirmed the consultation notes.",
                    Domain.Enums.NotificationType.ConsultationNote,
                    record.Id,
                    "ConsultationNote",
                    ct);
            }
        }
        catch { }

        return ApiResponse<ConsultationNoteDto>.SuccessResponse(confirmedDto, "Consultation notes confirmed successfully.");
    }

    public async Task<ApiResponse<List<ConsultationNoteDto>>> GetByPatientRecordAsync(Guid patientRecordId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var allPatientRecords = await _patientRecordRepo.GetAllAsync(ct);
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var allAppts = await _apptRepo.GetAllAsync(ct);

        var validIds = new HashSet<Guid> { patientRecordId };
        var matchingPr = allPatientRecords.FirstOrDefault(pr => pr.Id == patientRecordId || (pr.PatientId.HasValue && pr.PatientId.Value == patientRecordId));
        if (matchingPr != null)
        {
            validIds.Add(matchingPr.Id);
            if (matchingPr.PatientId.HasValue)
            {
                validIds.Add(matchingPr.PatientId.Value);
                var pat = allPatients.FirstOrDefault(p => p.Id == matchingPr.PatientId.Value || p.UserId == matchingPr.PatientId.Value);
                if (pat != null)
                {
                    validIds.Add(pat.Id);
                    validIds.Add(pat.UserId);
                }
            }
        }
        else
        {
            var pat = allPatients.FirstOrDefault(p => p.Id == patientRecordId || p.UserId == patientRecordId);
            if (pat != null)
            {
                validIds.Add(pat.Id);
                validIds.Add(pat.UserId);
                var prs = allPatientRecords.Where(pr => pr.PatientId.HasValue && (pr.PatientId.Value == pat.Id || pr.PatientId.Value == pat.UserId)).ToList();
                foreach (var pr in prs)
                {
                    validIds.Add(pr.Id);
                }
            }
        }

        var patientApptIds = allAppts
            .Where(a => a.PatientId.HasValue && validIds.Contains(a.PatientId.Value))
            .Select(a => a.Id)
            .ToHashSet();

        var records = await _recordRepo.GetAllAsync(ct);
        var filtered = records
            .Where(x => validIds.Contains(x.PatientRecordId) || (x.AppointmentId.HasValue && patientApptIds.Contains(x.AppointmentId.Value)))
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        var total = filtered.Count;
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
        var allPatientRecords = (await _patientRecordRepo.GetAllAsync(ct)) ?? new List<PatientRecord>();
        var allPatients = (await _patientRepo.GetAllAsync(ct)) ?? new List<PatientProfile>();
        var allAppts = (await _apptRepo.GetAllAsync(ct)) ?? new List<Appointment>();

        var validIds = new HashSet<Guid> { patientUserId };
        var pat = allPatients.FirstOrDefault(p => p.UserId == patientUserId || p.Id == patientUserId);
        PatientRecord? pr = null;
        if (pat != null)
        {
            validIds.Add(pat.Id);
            validIds.Add(pat.UserId);
            var prs = allPatientRecords.Where(r => r.PatientId.HasValue && (r.PatientId.Value == pat.Id || r.PatientId.Value == pat.UserId)).ToList();
            foreach (var r in prs)
            {
                validIds.Add(r.Id);
            }
        }
        else
        {
            pr = allPatientRecords.FirstOrDefault(p => p.Id == patientUserId || (p.PatientId.HasValue && p.PatientId.Value == patientUserId));
            if (pr != null)
            {
                validIds.Add(pr.Id);
                if (pr.PatientId.HasValue)
                {
                    validIds.Add(pr.PatientId.Value);
                }
            }
        }

        if (pat == null && pr == null)
            return ApiResponse<List<ConsultationNoteDto>>.ErrorResponse("Patient record not found");

        var patientApptIds = allAppts
            .Where(a => a.PatientId.HasValue && validIds.Contains(a.PatientId.Value))
            .Select(a => a.Id)
            .ToHashSet();

        var records = (await _recordRepo.GetAllAsync(ct)) ?? new List<ConsultationNote>();
        var filtered = records
            .Where(x => validIds.Contains(x.PatientRecordId) || (x.AppointmentId.HasValue && patientApptIds.Contains(x.AppointmentId.Value)))
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        var total = filtered.Count;
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

        // Retrieve existing requests for this doctor
        var all = await _verRepo.GetAllAsync(ct);
        var existingReq = all.Where(v => v.DoctorProfileId == doctor.Id)
                             .OrderByDescending(v => v.CreatedAt)
                             .FirstOrDefault();

        VerificationRequest request;
        // Draft or Submitted/Pending: update existing record in place and reset review state
        if (existingReq != null && (existingReq.Status == VerificationStatus.Draft || existingReq.Status == VerificationStatus.Submitted))
        {
            existingReq.Status = VerificationStatus.Submitted;
            existingReq.RejectionReason = null;
            existingReq.ReviewedAt = null;
            existingReq.ReviewedBy = null;
            existingReq.SubmittedAt = DateTime.UtcNow;
            existingReq.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto?.CertificateUrl))
            {
                existingReq.CertificateUrl = dto.CertificateUrl;
                existingReq.CertificatePublicId = dto.CertificatePublicId;
                existingReq.CertificateFileName = dto.CertificateFileName;
                existingReq.CertificateContentType = dto.CertificateContentType;
                existingReq.CertificateUploadedAt = DateTime.UtcNow;
            }

            _verRepo.Update(existingReq);
            request = existingReq;
        }
        else
        {
            // Rejected or Approved (or first submission): create a NEW request record to keep history
            request = new VerificationRequest
            {
                DoctorProfileId = doctor.Id,
                Status = VerificationStatus.Submitted,
                DoctorProfile = doctor,
                CertificateUrl = dto?.CertificateUrl,
                CertificatePublicId = dto?.CertificatePublicId,
                CertificateFileName = dto?.CertificateFileName,
                CertificateContentType = dto?.CertificateContentType,
                CertificateUploadedAt = !string.IsNullOrWhiteSpace(dto?.CertificateUrl) ? DateTime.UtcNow : null,
                SubmittedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await _verRepo.AddAsync(request, ct);
        }

        await _uow.SaveChangesAsync(ct);

        var result = await BuildDtoAsync(request, ct);
        return ApiResponse<VerificationRequestDto>.SuccessResponse(result, "Verification submitted successfully");
    }

    public async Task<ApiResponse<VerificationRequestDto>> GetVerificationStatusAsync(Guid doctorUserId, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<VerificationRequestDto>.ErrorResponse("Doctor not found");

        var all = await _verRepo.GetAllAsync(ct);
        var request = all.Where(v => v.DoctorProfileId == doctor.Id).OrderByDescending(v => v.CreatedAt).FirstOrDefault();
        if (request == null)
        {
            var doctorUser = await _userRepo.GetByIdAsync(doctor.UserId, ct);
            var draftDto = new VerificationRequestDto
            {
                DoctorProfileId = doctor.Id,
                DoctorName = doctorUser?.FullName ?? "Unknown",
                AvatarUrl = doctorUser?.AvatarUrl,
                LicenseNumber = doctor.LicenseNumber,
                Specialization = doctor.ProfessionalTitle,
                ExperienceYears = doctor.ExperienceYears,
                Biography = doctor.Biography,
                Status = doctor.VerificationStatus.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            return ApiResponse<VerificationRequestDto>.SuccessResponse(draftDto);
        }

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
        return await GetAllVerificationsAsync("Submitted", null, page, pageSize, ct);
    }

    public async Task<ApiResponse<List<VerificationRequestDto>>> GetAllVerificationsAsync(string? status, string? search, int page, int pageSize, CancellationToken ct)
    {
        var all = await _verRepo.GetAllAsync(ct);
        var dtos = new List<VerificationRequestDto>();
        foreach (var item in all)
            dtos.Add(await BuildDtoAsync(item, ct));

        var filtered = dtos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<VerificationStatus>(status, true, out var statusEnum))
            filtered = filtered.Where(v => string.Equals(v.Status, statusEnum.ToString(), StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            filtered = filtered.Where(v => (v.DoctorName != null && v.DoctorName.Contains(term, StringComparison.OrdinalIgnoreCase))
                                        || (v.LicenseNumber != null && v.LicenseNumber.Contains(term, StringComparison.OrdinalIgnoreCase))
                                        || (v.Specialization != null && v.Specialization.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        var list = filtered.OrderByDescending(v => v.SubmittedAt).ToList();
        var total = list.Count;
        var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return ApiResponse<List<VerificationRequestDto>>.SuccessResponse(items, pagination: new PaginationMetadata
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

    public async Task<ApiResponse> RequestAdditionalInfoAsync(Guid requestId, Guid supportUserId, string reason, CancellationToken ct)
    {
        var request = await _verRepo.GetByIdAsync(requestId, ct);
        if (request == null) return ApiResponse.ErrorResponse("Request not found");
        request.Status = VerificationStatus.RequiresAdditionalInfo;
        request.RejectionReason = reason;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedBy = supportUserId;
        _verRepo.Update(request);

        var doctor = await _doctorRepo.GetByIdAsync(request.DoctorProfileId, ct);
        if (doctor != null) { doctor.VerificationStatus = VerificationStatus.RequiresAdditionalInfo; _doctorRepo.Update(doctor); }

        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Additional information requested");
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

        // Retrieve previously approved certificate if current request is under review following an approved request
        string? prevApprovedUrl = null;
        string? prevApprovedFileName = null;
        DateTime? prevApprovedUploadedAt = null;

        var allRequests = await _verRepo.GetAllAsync(ct);
        var prevApproved = allRequests.Where(v => v.DoctorProfileId == request.DoctorProfileId 
                                               && v.Status == VerificationStatus.Approved 
                                               && v.Id != request.Id)
                                      .OrderByDescending(v => v.ReviewedAt ?? v.CreatedAt)
                                      .FirstOrDefault();

        if (prevApproved != null)
        {
            prevApprovedUrl = prevApproved.CertificateUrl;
            prevApprovedFileName = prevApproved.CertificateFileName;
            prevApprovedUploadedAt = prevApproved.CertificateUploadedAt ?? prevApproved.CreatedAt;
        }

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
            CertificatePublicId = request.CertificatePublicId,
            CertificateFileName = request.CertificateFileName,
            CertificateContentType = request.CertificateContentType,
            CertificateUploadedAt = request.CertificateUploadedAt ?? request.CreatedAt,
            SubmittedAt = request.SubmittedAt != default ? request.SubmittedAt : request.CreatedAt,
            CreatedAt = request.CreatedAt,
            PreviousApprovedCertificateUrl = prevApprovedUrl,
            PreviousApprovedCertificateFileName = prevApprovedFileName,
            PreviousApprovedCertificateUploadedAt = prevApprovedUploadedAt
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
        if (dto.DurationDays <= 0)
            return ApiResponse<ServicePackageDto>.ErrorResponse("Package duration must be greater than 0 days");
        if (dto.MaxDailySlotsCapacity is <= 0)
            return ApiResponse<ServicePackageDto>.ErrorResponse("Daily slot capacity must be greater than 0 when specified");
        if (dto.MaxPatientCapacity is <= 0)
            return ApiResponse<ServicePackageDto>.ErrorResponse("Patient capacity must be greater than 0 when specified");

        var pkg = new ServicePackage { Name = dto.Name, Description = dto.Description, DurationDays = dto.DurationDays, Price = dto.Price, MaxPatientCapacity = dto.MaxPatientCapacity, MaxDailySlotsCapacity = dto.MaxDailySlotsCapacity, IsFeatured = dto.IsFeatured };
        await _pkgRepo.AddAsync(pkg, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<ServicePackageDto>.SuccessResponse(_mapper.Map<ServicePackageDto>(pkg), "Package created");
    }

    public async Task<ApiResponse<ServicePackageDto>> UpdateAsync(Guid packageId, CreateServicePackageDto dto, CancellationToken ct)
    {
        var pkg = await _pkgRepo.GetByIdAsync(packageId, ct);
        if (pkg == null) return ApiResponse<ServicePackageDto>.ErrorResponse("Package not found");
        if (dto.DurationDays <= 0)
            return ApiResponse<ServicePackageDto>.ErrorResponse("Package duration must be greater than 0 days");
        if (dto.MaxDailySlotsCapacity is <= 0)
            return ApiResponse<ServicePackageDto>.ErrorResponse("Daily slot capacity must be greater than 0 when specified");
        if (dto.MaxPatientCapacity is <= 0)
            return ApiResponse<ServicePackageDto>.ErrorResponse("Patient capacity must be greater than 0 when specified");
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
    private readonly IRepository<Role> _roleRepo;
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

    public AdminService(IRepository<User> userRepo, IRepository<Role> roleRepo, IRepository<DoctorProfile> doctorRepo, IRepository<PatientProfile> patientRepo, IRepository<Appointment> apptRepo, IRepository<AuditLog> auditRepo, IRepository<Specialization> specRepo, IRepository<VerificationRequest> verRepo, IRepository<BlogPost> blogRepo, IRepository<SystemConfig> configRepo, IUnitOfWork uow, IMapper mapper)
    { _userRepo = userRepo; _roleRepo = roleRepo; _doctorRepo = doctorRepo; _patientRepo = patientRepo; _apptRepo = apptRepo; _auditRepo = auditRepo; _specRepo = specRepo; _verRepo = verRepo; _blogRepo = blogRepo; _configRepo = configRepo; _uow = uow; _mapper = mapper; }

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
            TotalUsers = users.Count(u => !u.IsDeleted),
            TotalDoctors = doctors.Count(),
            TotalPatients = patients.Count(),
            TotalAppointments = appts.Count(),
            PendingVerifications = vers.Count(v => v.Status == VerificationStatus.Submitted),
            PendingBlogs = blogs.Count(b => b.Status == BlogStatus.Pending)
        });
    }

    public async Task<ApiResponse<List<UserListDto>>> GetUsersAsync(string? search, string? role, int page, int pageSize, CancellationToken ct)
    {
        var allUsers = await _userRepo.GetAllAsync(ct);
        var allRoles = await _roleRepo.GetAllAsync(ct);
        var roleDict = allRoles.ToDictionary(r => r.Id, r => r.Name);

        var users = allUsers.Where(u => !u.IsDeleted).ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            users = users.Where(u => u.Email.Contains(term, StringComparison.OrdinalIgnoreCase)
                                  || u.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)
                                  || u.PhoneNumber.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var dtos = new List<UserListDto>();
        foreach (var u in users)
        {
            var dto = _mapper.Map<UserListDto>(u);
            if (roleDict.TryGetValue(u.RoleId, out var rName))
                dto.Role = rName;
            else if (string.IsNullOrEmpty(dto.Role))
                dto.Role = "User";
            dtos.Add(dto);
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            dtos = dtos.Where(d => string.Equals(d.Role, role, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var total = dtos.Count;
        var items = dtos.OrderByDescending(d => d.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ApiResponse<List<UserListDto>>.SuccessResponse(items, pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<UserListDto>> GetUserByIdAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null || user.IsDeleted) return ApiResponse<UserListDto>.ErrorResponse("User not found");

        var dto = _mapper.Map<UserListDto>(user);
        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        if (role != null) dto.Role = role.Name;

        return ApiResponse<UserListDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse> LockUserAsync(Guid userId, Guid requestingAdminId, CancellationToken ct)
    {
        if (userId == requestingAdminId)
            return ApiResponse.ErrorResponse("System Administrators cannot lock or disable their own account.");

        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null || user.IsDeleted) return ApiResponse.ErrorResponse("User not found");

        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        if (role != null && string.Equals(role.Name, "SystemAdmin", StringComparison.OrdinalIgnoreCase))
            return ApiResponse.ErrorResponse("System Administrator accounts cannot be locked directly.");

        user.Status = UserStatus.Locked;
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("User account locked successfully");
    }

    public async Task<ApiResponse> UnlockUserAsync(Guid userId, Guid? requestingAdminId = null, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null || user.IsDeleted) return ApiResponse.ErrorResponse("User not found");
        user.Status = UserStatus.Active;
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("User account unlocked successfully");
    }

    public async Task<ApiResponse<List<RoleDto>>> GetRolesAsync(CancellationToken ct = default)
    {
        var roles = await _roleRepo.GetAllAsync(ct);
        var users = await _userRepo.GetAllAsync(ct);

        var list = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            UserCount = users.Count(u => !u.IsDeleted && u.RoleId == r.Id)
        }).OrderBy(r => r.Name).ToList();

        return ApiResponse<List<RoleDto>>.SuccessResponse(list);
    }

    public async Task<ApiResponse<List<AuditLogDto>>> GetAuditLogsAsync(string? entityName, int page, int pageSize, CancellationToken ct = default)
    {
        var all = await _auditRepo.GetAllAsync(ct);
        var logs = all.ToList();
        if (!string.IsNullOrWhiteSpace(entityName))
            logs = logs.Where(l => l.EntityName.Contains(entityName, StringComparison.OrdinalIgnoreCase)).ToList();
        var total = logs.Count;
        var items = logs.OrderByDescending(l => l.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ApiResponse<List<AuditLogDto>>.SuccessResponse(_mapper.Map<List<AuditLogDto>>(items), pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<List<SpecializationDto>>> GetSpecializationsAsync(CancellationToken ct = default)
    {
        var all = await _specRepo.GetAllAsync(ct);
        return ApiResponse<List<SpecializationDto>>.SuccessResponse(_mapper.Map<List<SpecializationDto>>(all.Where(s => !s.IsDeleted).ToList()));
    }

    public async Task<ApiResponse<SpecializationDto>> CreateSpecializationAsync(string name, string? description, Guid? requestingAdminId = null, CancellationToken ct = default)
    {
        var spec = new Specialization { Name = name, Description = description };
        await _specRepo.AddAsync(spec, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<SpecializationDto>.SuccessResponse(_mapper.Map<SpecializationDto>(spec), "Specialization created");
    }

    public async Task<ApiResponse<SpecializationDto>> UpdateSpecializationAsync(Guid id, string name, string? description, Guid? requestingAdminId = null, CancellationToken ct = default)
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

    public async Task<ApiResponse> DeleteSpecializationAsync(Guid id, Guid? requestingAdminId = null, CancellationToken ct = default)
    {
        var spec = await _specRepo.GetByIdAsync(id, ct);
        if (spec == null) return ApiResponse.ErrorResponse("Specialization not found");
        spec.IsDeleted = true;
        _specRepo.Update(spec);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Specialization deleted successfully");
    }

    public async Task<ApiResponse<Dictionary<string, string>>> GetSystemSettingsAsync(CancellationToken ct = default)
    {
        var configs = await _configRepo.GetAllAsync(ct);
        var dict = configs.ToDictionary(c => c.Key, c => c.Value);
        return ApiResponse<Dictionary<string, string>>.SuccessResponse(dict);
    }

    public async Task<ApiResponse> UpdateSystemSettingsAsync(Dictionary<string, string> settings, Guid? requestingAdminId = null, CancellationToken ct = default)
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
    private readonly IRepository<CustomClinicalField>? _customFieldRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IFavoriteDoctorNotificationService? _favoriteNotificationService;

    public TreatmentPackageService(
        IRepository<TreatmentPackage> packageRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<User> userRepo,
        IRepository<TreatmentCase> caseRepo,
        INotificationService notificationService,
        IUnitOfWork uow,
        IMapper mapper,
        IFavoriteDoctorNotificationService? favoriteNotificationService = null,
        IRepository<CustomClinicalField>? customFieldRepo = null)
    {
        _packageRepo = packageRepo;
        _doctorRepo = doctorRepo;
        _patientRepo = patientRepo;
        _userRepo = userRepo;
        _caseRepo = caseRepo;
        _notificationService = notificationService;
        _uow = uow;
        _mapper = mapper;
        _favoriteNotificationService = favoriteNotificationService;
        _customFieldRepo = customFieldRepo;
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
                p.Status is TreatmentPackageStatus.Assigned
                    or TreatmentPackageStatus.Accepted
                    or TreatmentPackageStatus.Active
                    or TreatmentPackageStatus.CancellationPending);
            if (existingActive != null)
                return ApiResponse<TreatmentPackageDto>.ErrorResponse("This patient already has an active treatment package. Please cancel or complete the existing package before creating a new one.");
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
            RecommendedSessionsPerWeek = dto.RecommendedSessionsPerWeek > 0 ? dto.RecommendedSessionsPerWeek : 1,
            ExpirationDate = DateTime.UtcNow.AddDays(validityDays),
            Price = dto.Price,
            Status = patient != null ? TreatmentPackageStatus.Assigned : TreatmentPackageStatus.Created,
            AssignedDate = patient != null ? DateTime.UtcNow : null,
            Doctor = doctor,
            Patient = patient
        };

        await _packageRepo.AddAsync(package, ct);
        await _uow.SaveChangesAsync(ct);

        if (_customFieldRepo != null && dto.CustomFields != null && dto.CustomFields.Any())
        {
            int idx = 0;
            foreach (var cf in dto.CustomFields)
            {
                if (!string.IsNullOrWhiteSpace(cf.Title))
                {
                    await _customFieldRepo.AddAsync(new CustomClinicalField
                    {
                        OwnerType = "TreatmentPackage",
                        OwnerId = package.Id,
                        SectionKey = string.IsNullOrWhiteSpace(cf.SectionKey) ? "BasicInformation" : cf.SectionKey,
                        Title = cf.Title,
                        Content = cf.Content,
                        FieldType = cf.FieldType ?? "Text",
                        OrderIndex = cf.OrderIndex > 0 ? cf.OrderIndex : idx++,
                        CreatedByDoctorId = doctor.Id
                    }, ct);
                }
            }
            await _uow.SaveChangesAsync(ct);
        }

        // Send notification to patient (TreatmentCase is created only when patient accepts)
        if (patient != null)
        {
            try
            {
                var allUsers = await _userRepo.GetAllAsync(ct);
                var doctorUser = allUsers.FirstOrDefault(u => u.Id == doctorUserId);
                await _notificationService.CreateNotificationAsync(
                    patient.UserId,
                    "📦 New Treatment Package",
                    $"Dr. {doctorUser?.FullName ?? "your doctor"} has created a treatment package \"{dto.Name}\" for you. Please review and accept to start treatment.",
                    Domain.Enums.NotificationType.Package,
                    package.Id,
                    "TreatmentPackage",
                    ct);
            }
            catch { }
        }
        else if (_favoriteNotificationService != null)
        {
            try
            {
                var doctorName = (await _userRepo.GetAllAsync(ct))
                    .FirstOrDefault(u => u.Id == doctor.UserId)?.FullName ?? "your favorite doctor";
                await _favoriteNotificationService.NotifyFollowersAsync(
                    doctor.Id,
                    doctor.UserId,
                    doctorName,
                    $"New treatment program from Dr. {doctorName}",
                    $"Dr. {doctorName} added a new treatment program: {package.Name}",
                    doctor.Id,
                    "FavoriteDoctor",
                    ct);
            }
            catch
            {
                // A template is still created when follower notification delivery is unavailable.
            }
        }

        var createdDto = _mapper.Map<TreatmentPackageDto>(package);
        await EnrichNamesAsync(new List<TreatmentPackageDto> { createdDto }, ct);

        return ApiResponse<TreatmentPackageDto>.SuccessResponse(createdDto, patient != null ? "Treatment package created and assigned. Treatment case will be created when patient accepts." : "Template treatment package created successfully");
    }

    public async Task<ApiResponse<TreatmentPackageDto>> UpdateAsync(Guid packageId, Guid doctorUserId, UpdateTreatmentPackageDto dto, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null)
            return ApiResponse<TreatmentPackageDto>.ErrorResponse("Doctor not found");

        var package = await _packageRepo.GetByIdAsync(packageId, ct);
        if (package == null)
            return ApiResponse<TreatmentPackageDto>.ErrorResponse("Treatment package not found");

        if (package.DoctorId != doctor.Id)
            return ApiResponse<TreatmentPackageDto>.ErrorResponse("You are not authorized to edit this treatment package");

        package.Name = dto.Name;
        package.Description = dto.Description;
        package.TargetOutcome = dto.TargetOutcome;
        package.RecommendedExercises = dto.RecommendedExercises;
        package.Instructions = dto.Instructions;
        package.SessionQuantity = dto.SessionQuantity;
        package.ValidityDays = dto.ValidityDays > 0 ? dto.ValidityDays : 90;
        package.RecommendedSessionsPerWeek = dto.RecommendedSessionsPerWeek > 0 ? dto.RecommendedSessionsPerWeek : 1;
        package.Price = dto.Price;
        package.UpdatedAt = DateTime.UtcNow;

        _packageRepo.Update(package);

        if (_customFieldRepo != null && dto.CustomFields != null)
        {
            try
            {
                var existingFields = (await _customFieldRepo.GetAllAsync(ct))
                    .Where(f => f.OwnerType == "TreatmentPackage" && f.OwnerId == package.Id)
                    .ToList();
                foreach (var f in existingFields)
                {
                    _customFieldRepo.Delete(f);
                }
                int idx = 0;
                foreach (var cf in dto.CustomFields)
                {
                    if (!string.IsNullOrWhiteSpace(cf.Title))
                    {
                        await _customFieldRepo.AddAsync(new CustomClinicalField
                        {
                            OwnerType = "TreatmentPackage",
                            OwnerId = package.Id,
                            SectionKey = string.IsNullOrWhiteSpace(cf.SectionKey) ? "BasicInformation" : cf.SectionKey,
                            Title = cf.Title,
                            Content = cf.Content,
                            FieldType = cf.FieldType ?? "Text",
                            OrderIndex = cf.OrderIndex > 0 ? cf.OrderIndex : idx++,
                            CreatedByDoctorId = doctor.Id
                        }, ct);
                    }
                }
            }
            catch { }
        }

        await _uow.SaveChangesAsync(ct);

        var resultDto = _mapper.Map<TreatmentPackageDto>(package);
        await EnrichNamesAsync(new List<TreatmentPackageDto> { resultDto }, ct);
        return ApiResponse<TreatmentPackageDto>.SuccessResponse(resultDto, "Treatment package updated successfully");
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
            if (dto.CancellationRequestedByUserId.HasValue &&
                userDict.TryGetValue(dto.CancellationRequestedByUserId.Value, out var requestedByName))
            {
                dto.CancellationRequestedByName = requestedByName;
            }
        }

        if (_customFieldRepo != null && dtos.Any())
        {
            try
            {
                var allFields = await _customFieldRepo.GetAllAsync(ct);
                var fieldsByPackage = allFields.Where(f => f.OwnerType == "TreatmentPackage")
                    .GroupBy(f => f.OwnerId)
                    .ToDictionary(g => g.Key, g => g.OrderBy(f => f.OrderIndex).Select(f => new CustomClinicalFieldDto
                    {
                        Id = f.Id,
                        OwnerType = f.OwnerType,
                        OwnerId = f.OwnerId,
                        SectionKey = f.SectionKey,
                        Title = f.Title,
                        Content = f.Content,
                        FieldType = f.FieldType,
                        OrderIndex = f.OrderIndex,
                        CreatedByDoctorId = f.CreatedByDoctorId
                    }).ToList());

                foreach (var dto in dtos)
                {
                    if (fieldsByPackage.TryGetValue(dto.Id, out var fields))
                    {
                        dto.CustomFields = fields;
                    }
                }
            }
            catch { }
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
            return ApiResponse<List<TreatmentPackageDto>>.ErrorResponse("Patient profile not found");

        var all = await _packageRepo.GetAllAsync(ct);
        var packages = all.Where(p => !p.IsDeleted && p.PatientId == patient.Id).ToList();
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

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == userId);
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == userId);
        var canView = (doctor != null && package.DoctorId == doctor.Id)
            || (patient != null && package.PatientId == patient.Id);

        if (!canView)
            return ApiResponse<TreatmentPackageDto>.ErrorResponse("Not authorized to view this package");

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
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId || p.Id == patientUserId);
        if (patient == null)
            return ApiResponse.ErrorResponse("Patient profile not found");

        if (package.PatientId.HasValue &&
            package.PatientId != patient.Id &&
            package.PatientId != patient.UserId &&
            package.PatientId != patientUserId)
        {
            return ApiResponse.ErrorResponse("Not authorized to accept this package");
        }

        if (package.Status != TreatmentPackageStatus.Assigned && package.Status != TreatmentPackageStatus.Active && package.Status != TreatmentPackageStatus.Created)
            return ApiResponse.ErrorResponse("Only assigned packages can be accepted");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.Id == package.DoctorId || d.UserId == package.DoctorId);
        var doctorIdToUse = doctor?.Id ?? package.DoctorId;
        var patientIdToUse = patient.Id;

        // Auto assign patient id if not set yet
        if (!package.PatientId.HasValue)
        {
            package.PatientId = patientIdToUse;
        }

        var allCases = await _caseRepo.GetAllAsync(ct);
        var existingCase = allCases.FirstOrDefault(c =>
            c.TreatmentPackageId == package.Id &&
            !c.IsDeleted);

        if (package.Status == TreatmentPackageStatus.Active && existingCase != null)
            return ApiResponse.SuccessResponse("Treatment package was already accepted.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var now = DateTime.UtcNow;
            package.Status = TreatmentPackageStatus.Active;
            package.AcceptedDate ??= now;
            package.ActiveDate ??= now;
            package.UpdatedAt = now;
            _packageRepo.Update(package);

            if (existingCase == null)
            {
                var treatmentCase = new TreatmentCase
                {
                    TreatmentPackageId = package.Id,
                    DoctorId = doctorIdToUse,
                    PatientId = patientIdToUse,
                    CaseName = package.Name,
                    CaseDescription = package.Description,
                    PrimaryConcern = !string.IsNullOrWhiteSpace(package.TargetOutcome) ? package.TargetOutcome : package.Name,

                    // Preserve the proposal exactly as it was when the patient accepted it.
                    PackageNameSnapshot = package.Name,
                    PackageDescriptionSnapshot = package.Description,
                    TotalSessionsSnapshot = package.SessionQuantity,
                    DurationDaysSnapshot = package.ValidityDays,
                    RecommendedSessionsPerWeekSnapshot = package.RecommendedSessionsPerWeek,
                    PriceSnapshot = package.Price,
                    TargetOutcomesSnapshot = package.TargetOutcome,
                    RecommendedExercisesSnapshot = package.RecommendedExercises,
                    PatientGuidanceSnapshot = package.Instructions,

                    TotalSessions = package.SessionQuantity,
                    CompletedSessions = 0,
                    RemainingSessions = package.SessionQuantity,
                    OverallProgressPercent = 0,
                    StartDate = now,
                    ExpectedEndDate = now.AddDays(package.ValidityDays > 0 ? package.ValidityDays : 90),
                    Status = TreatmentCaseStatus.Active
                };
                await _caseRepo.AddAsync(treatmentCase, ct);
            }
            else if (existingCase.Status == TreatmentCaseStatus.OnHold || existingCase.Status == TreatmentCaseStatus.Cancelled)
            {
                existingCase.Status = TreatmentCaseStatus.Active;
                existingCase.UpdatedAt = now;
                _caseRepo.Update(existingCase);
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
            return ApiResponse.SuccessResponse("Treatment package accepted and treatment case created.");
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            return ApiResponse.ErrorResponse("Unable to accept the treatment package. Please try again.");
        }
    }

    public async Task<ApiResponse> RejectPackageAsync(Guid packageId, Guid patientUserId, string? reason, CancellationToken ct)
    {
        var package = await _packageRepo.GetByIdAsync(packageId, ct);
        if (package == null)
            return ApiResponse.ErrorResponse("Treatment package not found");

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId || p.Id == patientUserId);
        if (patient == null)
            return ApiResponse.ErrorResponse("Patient profile not found");

        if (package.PatientId.HasValue &&
            package.PatientId != patient.Id &&
            package.PatientId != patient.UserId &&
            package.PatientId != patientUserId)
        {
            return ApiResponse.ErrorResponse("Not authorized to reject this package");
        }

        if (package.Status == TreatmentPackageStatus.Rejected)
            return ApiResponse.SuccessResponse("Treatment package was already rejected.");

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

        // Only active proposals and treatment packages support the two-party cancellation flow.
        if (package.Status is TreatmentPackageStatus.Completed
            or TreatmentPackageStatus.Cancelled
            or TreatmentPackageStatus.Rejected
            or TreatmentPackageStatus.Expired)
            return ApiResponse.ErrorResponse("This package cannot be cancelled");

        if (package.Status != TreatmentPackageStatus.CancellationPending)
        {
            package.Status = TreatmentPackageStatus.CancellationPending;
            package.CancellationRequestedByUserId = userId;
            package.CancellationRequestedAt = DateTime.UtcNow;
            package.CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            package.UpdatedAt = DateTime.UtcNow;
            _packageRepo.Update(package);
            await _uow.SaveChangesAsync(ct);

            var recipientUserId = isDoctor ? patient!.UserId : doctor!.UserId;
            try
            {
                await _notificationService.CreateNotificationAsync(
                    recipientUserId,
                    "Treatment package cancellation requested",
                    $"The {(isDoctor ? "doctor" : "patient")} requested cancellation for \"{package.Name}\". Please review the package and confirm cancellation.",
                    Domain.Enums.NotificationType.Package,
                    package.Id,
                    "TreatmentPackage",
                    ct);
            }
            catch { }

            return ApiResponse.SuccessResponse("Cancellation request sent. The package remains active until the other party confirms.");
        }

        if (package.CancellationRequestedByUserId == userId)
            return ApiResponse.ErrorResponse("Cancellation is awaiting confirmation from the other party.");

        package.Status = TreatmentPackageStatus.Cancelled;
        package.CancellationReason ??= "Cancelled after confirmation by both parties.";
        package.RejectionReason = reason ?? "Cancelled by " + (isDoctor ? "doctor" : "patient");
        package.UpdatedAt = DateTime.UtcNow;
        _packageRepo.Update(package);

        // Keep the original request reason in the final package history.
        package.RejectionReason = package.CancellationReason ?? reason ?? "Cancelled after confirmation by both parties.";

        // Cascade cancel to any linked active TreatmentCase
        var allCases = await _caseRepo.GetAllAsync(ct);
        var linkedActiveCases = allCases.Where(c =>
            c.TreatmentPackageId == package.Id &&
            !c.IsDeleted &&
            (c.Status == TreatmentCaseStatus.Active || c.Status == TreatmentCaseStatus.OnHold)).ToList();

        foreach (var linkedCase in linkedActiveCases)
        {
            linkedCase.Status = TreatmentCaseStatus.Cancelled;
            linkedCase.ClosureNote = $"Automatically cancelled due to treatment package cancellation. Reason: {package.RejectionReason}";
            linkedCase.ActualEndDate = DateTime.UtcNow;
            linkedCase.UpdatedAt = DateTime.UtcNow;
            _caseRepo.Update(linkedCase);
        }

        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Treatment package cancelled successfully");
    }
}

public class SubscriptionService : ISubscriptionService
{
    private readonly IRepository<DoctorSubscription> _subRepo;
    private readonly IRepository<ServicePackage> _pkgRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<PaymentTransaction> _paymentRepo;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public SubscriptionService(
        IRepository<DoctorSubscription> subRepo,
        IRepository<ServicePackage> pkgRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<User> userRepo,
        IRepository<PaymentTransaction> paymentRepo,
        IPaymentService paymentService,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _subRepo = subRepo;
        _pkgRepo = pkgRepo;
        _doctorRepo = doctorRepo;
        _userRepo = userRepo;
        _paymentRepo = paymentRepo;
        _paymentService = paymentService;
        _uow = uow;
        _mapper = mapper;
    }

    private static DateTime GetNextSubscriptionStartDate(
        IEnumerable<DoctorSubscription> subscriptions,
        Guid doctorProfileId,
        DateTime now,
        Guid? excludedSubscriptionId = null)
    {
        var latestExpiration = subscriptions
            .Where(s => s.DoctorProfileId == doctorProfileId
                     && s.Status == SubscriptionStatus.Active
                     && s.ExpirationDate > now
                     && (!excludedSubscriptionId.HasValue || s.Id != excludedSubscriptionId.Value))
            .Select(s => s.ExpirationDate)
            .DefaultIfEmpty(now)
            .Max();

        return latestExpiration > now ? latestExpiration : now;
    }

    // Purchases are queued as consecutive subscription records to preserve payment history.
    // The current-plan screen must still show the end of the uninterrupted entitlement.
    private static DateTime GetEffectiveSubscriptionExpiration(
        IEnumerable<DoctorSubscription> subscriptions,
        Guid doctorProfileId,
        DoctorSubscription activeSubscription,
        DateTime now)
    {
        var coverageEnd = activeSubscription.ExpirationDate;
        var queuedSubscriptions = subscriptions
            .Where(s => s.DoctorProfileId == doctorProfileId
                     && s.Status == SubscriptionStatus.Active
                     && s.ExpirationDate > now
                     && s.Id != activeSubscription.Id)
            .OrderBy(s => s.StartDate)
            .ToList();

        foreach (var queued in queuedSubscriptions)
        {
            if (queued.StartDate > coverageEnd) break;
            if (queued.ExpirationDate > coverageEnd)
                coverageEnd = queued.ExpirationDate;
        }

        return coverageEnd;
    }

    private static string GetDisplayStatus(DoctorSubscription subscription, DateTime now) =>
        subscription.Status == SubscriptionStatus.Active && subscription.ExpirationDate <= now
            ? SubscriptionStatus.Expired.ToString()
            : subscription.Status.ToString();

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
            var allSubs = await _subRepo.GetAllAsync(ct);
            var startDate = GetNextSubscriptionStartDate(allSubs, doctor.Id, DateTime.UtcNow);

            var freeSub = new DoctorSubscription
            {
                DoctorProfileId = doctor.Id,
                ServicePackageId = servicePackageId,
                Status = SubscriptionStatus.Active,
                StartDate = startDate,
                ExpirationDate = startDate.AddDays(package.DurationDays),
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
                MaxDailySlotsCapacity = package.MaxDailySlotsCapacity,
                MaxPatientCapacity = package.MaxPatientCapacity,
                CreatedAt = freeSub.CreatedAt
            }, "Free trial activated successfully!");
        }

        // PAID PACKAGE — use VNPay
        var purchaseNow = DateTime.UtcNow;
        var subscription = new DoctorSubscription
        {
            DoctorProfileId = doctor.Id,
            ServicePackageId = servicePackageId,
            Status = SubscriptionStatus.PendingPayment,
            // The final period is calculated again at successful payment time.
            StartDate = purchaseNow,
            ExpirationDate = purchaseNow.AddDays(package.DurationDays),
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
            MaxDailySlotsCapacity = package.MaxDailySlotsCapacity,
            MaxPatientCapacity = package.MaxPatientCapacity,
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

        var allSubs = await _subRepo.GetAllAsync(ct);
        var startDate = GetNextSubscriptionStartDate(allSubs, doctor.Id, DateTime.UtcNow);

        var subscription = new DoctorSubscription
        {
            DoctorProfileId = doctor.Id,
            ServicePackageId = servicePackageId,
            Status = SubscriptionStatus.Active,
            StartDate = startDate,
            ExpirationDate = startDate.AddDays(package.DurationDays),
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
            MaxDailySlotsCapacity = package.MaxDailySlotsCapacity,
            MaxPatientCapacity = package.MaxPatientCapacity,
            CreatedAt = subscription.CreatedAt
        }, "Subscription created and activated successfully.");
    }

    public async Task<ApiResponse<SubscriptionDto>> GetActiveSubscriptionAsync(Guid doctorUserId, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<SubscriptionDto>.ErrorResponse("Doctor not found");

        var now = DateTime.UtcNow;
        var allSubs = await _subRepo.GetAllAsync(ct);
        var activeSub = allSubs
            .Where(s => s.DoctorProfileId == doctor.Id
                     && s.Status == SubscriptionStatus.Active
                     && s.StartDate <= now
                     && s.ExpirationDate > now)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefault();

        if (activeSub == null)
        {
            // Fallback: Check for PendingPayment or latest subscription if active is absent
            activeSub = allSubs.Where(s => s.DoctorProfileId == doctor.Id)
                               .OrderByDescending(s => s.CreatedAt)
                               .FirstOrDefault();

            if (activeSub == null)
                return ApiResponse<SubscriptionDto>.ErrorResponse("No active subscription");
        }

        var pkg = await _pkgRepo.GetByIdAsync(activeSub.ServicePackageId, ct);
        var effectiveExpiration = GetEffectiveSubscriptionExpiration(allSubs, doctor.Id, activeSub, now);
        return ApiResponse<SubscriptionDto>.SuccessResponse(new SubscriptionDto
        {
            Id = activeSub.Id,
            DoctorId = doctor.Id,
            ServicePackageId = activeSub.ServicePackageId,
            PackageName = pkg?.Name ?? "Unknown",
            Status = GetDisplayStatus(activeSub, now),
            StartDate = activeSub.StartDate,
            ExpirationDate = effectiveExpiration,
            EndDate = effectiveExpiration,
            AmountPaid = pkg?.Price ?? 0,
            MaxDailySlotsCapacity = pkg?.MaxDailySlotsCapacity,
            MaxPatientCapacity = pkg?.MaxPatientCapacity,
            CreatedAt = activeSub.CreatedAt
        });
    }

    public async Task<ApiResponse<List<SubscriptionDto>>> GetSubscriptionHistoryAsync(Guid doctorUserId, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<List<SubscriptionDto>>.ErrorResponse("Doctor not found");

        var now = DateTime.UtcNow;
        var allSubs = await _subRepo.GetAllAsync(ct);
        var subs = allSubs.Where(s => s.DoctorProfileId == doctor.Id).OrderByDescending(s => s.CreatedAt).ToList();

        var dtos = new List<SubscriptionDto>();
        foreach (var sub in subs)
        {
            var pkg = await _pkgRepo.GetByIdAsync(sub.ServicePackageId, ct);
            dtos.Add(new SubscriptionDto
            {
                Id = sub.Id,
                DoctorId = doctor.Id,
                ServicePackageId = sub.ServicePackageId,
                PackageName = pkg?.Name ?? "Unknown",
                Status = GetDisplayStatus(sub, now),
                StartDate = sub.StartDate,
                ExpirationDate = sub.ExpirationDate,
                EndDate = sub.ExpirationDate,
                AmountPaid = pkg?.Price ?? 0,
                MaxDailySlotsCapacity = pkg?.MaxDailySlotsCapacity,
                MaxPatientCapacity = pkg?.MaxPatientCapacity,
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
                var allSubs = await _subRepo.GetAllAsync(ct);
                var package = await _pkgRepo.GetByIdAsync(sub.ServicePackageId, ct);
                if (package == null)
                    return ApiResponse.ErrorResponse("Service package not found");

                var now = DateTime.UtcNow;
                var startDate = GetNextSubscriptionStartDate(allSubs, sub.DoctorProfileId, now, sub.Id);
                sub.Status = SubscriptionStatus.Active;
                sub.StartDate = startDate;
                sub.ExpirationDate = startDate.AddDays(package.DurationDays);
                sub.UpdatedAt = now;
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

    public async Task<ApiResponse<List<SubscriptionDto>>> GetAllSubscriptionsAsync(string? status, string? search, int page, int pageSize, CancellationToken ct)
    {
        var allSubs = await _subRepo.GetAllAsync(ct);
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var allPackages = await _pkgRepo.GetAllAsync(ct);
        var allUsers = await _userRepo.GetAllAsync(ct);
        var now = DateTime.UtcNow;

        var docDict = allDoctors.ToDictionary(d => d.Id);
        var userDict = allUsers.ToDictionary(u => u.Id);
        var pkgDict = allPackages.ToDictionary(p => p.Id);

        var list = new List<SubscriptionDto>();
        foreach (var sub in allSubs)
        {
            var doctorName = "Unknown Doctor";
            if (docDict.TryGetValue(sub.DoctorProfileId, out var doc) && userDict.TryGetValue(doc.UserId, out var u))
            {
                doctorName = u.FullName;
            }

            var pkgName = pkgDict.TryGetValue(sub.ServicePackageId, out var pkg) ? pkg.Name : "Custom Package";
            var amount = pkg?.Price ?? 0;

            list.Add(new SubscriptionDto
            {
                Id = sub.Id,
                DoctorId = sub.DoctorProfileId,
                DoctorName = doctorName,
                ServicePackageId = sub.ServicePackageId,
                PackageName = pkgName,
                Status = GetDisplayStatus(sub, now),
                StartDate = sub.StartDate,
                ExpirationDate = sub.ExpirationDate,
                EndDate = sub.ExpirationDate,
                AmountPaid = amount,
                MaxDailySlotsCapacity = pkg?.MaxDailySlotsCapacity,
                MaxPatientCapacity = pkg?.MaxPatientCapacity,
                CreatedAt = sub.CreatedAt
            });
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            list = list.Where(s => string.Equals(s.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            list = list.Where(s => (s.DoctorName != null && s.DoctorName.Contains(term, StringComparison.OrdinalIgnoreCase))
                                || s.PackageName.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var total = list.Count;
        var items = list.OrderByDescending(s => s.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ApiResponse<List<SubscriptionDto>>.SuccessResponse(items, pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<SubscriptionDto>> GetSubscriptionByIdAsync(Guid subscriptionId, CancellationToken ct)
    {
        var sub = await _subRepo.GetByIdAsync(subscriptionId, ct);
        if (sub == null) return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found");

        var doc = await _doctorRepo.GetByIdAsync(sub.DoctorProfileId, ct);
        var user = doc != null ? await _userRepo.GetByIdAsync(doc.UserId, ct) : null;
        var pkg = await _pkgRepo.GetByIdAsync(sub.ServicePackageId, ct);

        return ApiResponse<SubscriptionDto>.SuccessResponse(new SubscriptionDto
        {
            Id = sub.Id,
            DoctorId = sub.DoctorProfileId,
            DoctorName = user?.FullName ?? "Unknown Doctor",
            ServicePackageId = sub.ServicePackageId,
            PackageName = pkg?.Name ?? "Custom Package",
            Status = GetDisplayStatus(sub, DateTime.UtcNow),
            StartDate = sub.StartDate,
            ExpirationDate = sub.ExpirationDate,
            EndDate = sub.ExpirationDate,
            AmountPaid = pkg?.Price ?? 0,
            MaxDailySlotsCapacity = pkg?.MaxDailySlotsCapacity,
            MaxPatientCapacity = pkg?.MaxPatientCapacity,
            CreatedAt = sub.CreatedAt
        });
    }
}
