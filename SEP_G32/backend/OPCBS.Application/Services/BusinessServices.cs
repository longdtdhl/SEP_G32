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
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public BlogService(IRepository<BlogPost> blogRepo, IRepository<DoctorProfile> doctorRepo, IUnitOfWork uow, IMapper mapper)
    { _blogRepo = blogRepo; _doctorRepo = doctorRepo; _uow = uow; _mapper = mapper; }

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
        if (blog.Status != BlogStatus.Pending) return ApiResponse.ErrorResponse("Only pending blogs can be approved");
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
        if (blog.Status != BlogStatus.Pending) return ApiResponse.ErrorResponse("Only pending blogs can be rejected");
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
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || blog.DoctorId != doctor.Id)
            return ApiResponse.ErrorResponse("Not authorized to delete this blog");
        blog.IsDeleted = true;
        blog.UpdatedAt = DateTime.UtcNow;
        _blogRepo.Update(blog);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Blog deleted");
    }

    public async Task<ApiResponse<BlogCommentDto>> AddCommentAsync(Guid userId, CreateBlogCommentDto dto, CancellationToken ct)
    {
        var blog = await _blogRepo.GetByIdAsync(dto.BlogPostId, ct);
        if (blog == null) return ApiResponse<BlogCommentDto>.ErrorResponse("Blog not found");
        // Blog comments are stored in the BlogComment entity — for MVP return stub
        return ApiResponse<BlogCommentDto>.SuccessResponse(new BlogCommentDto
        {
            Id = Guid.NewGuid(), BlogPostId = dto.BlogPostId, UserName = "User", Content = dto.Content, CreatedAt = DateTime.UtcNow
        }, "Comment added");
    }

    public Task<ApiResponse<BlogCommentDto>> UpdateCommentAsync(Guid commentId, Guid userId, UpdateBlogCommentDto dto, CancellationToken ct)
    {
        return Task.FromResult(ApiResponse<BlogCommentDto>.SuccessResponse(new BlogCommentDto
        {
            Id = commentId, BlogPostId = Guid.Empty, UserName = "User", Content = dto.Content, CreatedAt = DateTime.UtcNow
        }, "Comment updated"));
    }

    public Task<ApiResponse> DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken ct)
    {
        return Task.FromResult(ApiResponse.SuccessResponse("Comment deleted"));
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
        var all = await _reviewRepo.GetAllAsync(ct);
        var reviews = all.Where(r => r.DoctorId == doctorProfileId && r.IsVisible).ToList();
        var total = reviews.Count;
        var items = reviews.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ApiResponse<List<ReviewDto>>.SuccessResponse(_mapper.Map<List<ReviewDto>>(items), pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }
}

public class ConsultationRecordService : IConsultationRecordService
{
    private readonly IRepository<ConsultationRecord> _recordRepo;
    private readonly IRepository<Appointment> _apptRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ConsultationRecordService(IRepository<ConsultationRecord> recordRepo, IRepository<Appointment> apptRepo, IRepository<DoctorProfile> doctorRepo, IRepository<PatientProfile> patientRepo, IUnitOfWork uow, IMapper mapper)
    { _recordRepo = recordRepo; _apptRepo = apptRepo; _doctorRepo = doctorRepo; _patientRepo = patientRepo; _uow = uow; _mapper = mapper; }

    public async Task<ApiResponse<ConsultationRecordDto>> CreateAsync(Guid doctorUserId, CreateConsultationRecordDto dto, CancellationToken ct)
    {
        var appointment = await _apptRepo.GetByIdAsync(dto.AppointmentId, ct);
        if (appointment == null) return ApiResponse<ConsultationRecordDto>.ErrorResponse("Appointment not found");
        if (appointment.Status != AppointmentStatus.Completed && appointment.Status != AppointmentStatus.Approved)
            return ApiResponse<ConsultationRecordDto>.ErrorResponse("Appointment must be approved or completed");

        var allRecords = await _recordRepo.GetAllAsync(ct);
        if (allRecords.Any(r => r.AppointmentId == dto.AppointmentId))
            return ApiResponse<ConsultationRecordDto>.ErrorResponse("Consultation record already exists for this appointment");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null || appointment.DoctorId != doctor.Id)
            return ApiResponse<ConsultationRecordDto>.ErrorResponse("Not authorized");

        if (appointment.PatientId == null)
            return ApiResponse<ConsultationRecordDto>.ErrorResponse("Cannot create record for guest appointment");

        var record = new ConsultationRecord
        {
            AppointmentId = dto.AppointmentId,
            DoctorId = doctor.Id,
            PatientId = appointment.PatientId.Value,
            ConsultationSummary = dto.ConsultationSummary,
            Diagnosis = dto.Diagnosis,
            Recommendation = dto.Recommendation,
            FollowUpNotes = dto.FollowUpNotes,
            Prescription = dto.Prescription,
            NextAppointmentRecommendedDate = dto.NextAppointmentRecommendedDate,
            Appointment = appointment,
            Doctor = doctor,
            Patient = appointment.Patient!
        };
        await _recordRepo.AddAsync(record, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<ConsultationRecordDto>.SuccessResponse(_mapper.Map<ConsultationRecordDto>(record), "Record created");
    }

    public async Task<ApiResponse<ConsultationRecordDto>> UpdateAsync(Guid recordId, Guid doctorUserId, UpdateConsultationRecordDto dto, CancellationToken ct)
    {
        var record = await _recordRepo.GetByIdAsync(recordId, ct);
        if (record == null) return ApiResponse<ConsultationRecordDto>.ErrorResponse("Record not found");
        record.ConsultationSummary = dto.ConsultationSummary;
        record.Diagnosis = dto.Diagnosis;
        record.Recommendation = dto.Recommendation;
        record.FollowUpNotes = dto.FollowUpNotes;
        record.Prescription = dto.Prescription;
        record.UpdatedAt = DateTime.UtcNow;
        _recordRepo.Update(record);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<ConsultationRecordDto>.SuccessResponse(_mapper.Map<ConsultationRecordDto>(record), "Record updated");
    }

    public async Task<ApiResponse<List<ConsultationRecordDto>>> GetByPatientAsync(Guid patientUserId, int page, int pageSize, CancellationToken ct)
    {
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        if (patient == null) return ApiResponse<List<ConsultationRecordDto>>.ErrorResponse("Patient not found");
        var all = await _recordRepo.GetAllAsync(ct);
        var records = all.Where(r => r.PatientId == patient.Id).ToList();
        var total = records.Count;
        var items = records.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ApiResponse<List<ConsultationRecordDto>>.SuccessResponse(_mapper.Map<List<ConsultationRecordDto>>(items), pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<ConsultationRecordDto>> GetByIdAsync(Guid recordId, Guid userId, CancellationToken ct)
    {
        var record = await _recordRepo.GetByIdAsync(recordId, ct);
        if (record == null) return ApiResponse<ConsultationRecordDto>.ErrorResponse("Record not found");
        return ApiResponse<ConsultationRecordDto>.SuccessResponse(_mapper.Map<ConsultationRecordDto>(record));
    }

    public async Task<ApiResponse<List<ConsultationRecordDto>>> GetByAppointmentAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null)
            return ApiResponse<List<ConsultationRecordDto>>.ErrorResponse("Doctor not found");

        var all = await _recordRepo.GetAllAsync(ct);
        var records = all.Where(r => r.AppointmentId == appointmentId && r.DoctorId == doctor.Id).ToList();
        return ApiResponse<List<ConsultationRecordDto>>.SuccessResponse(
            _mapper.Map<List<ConsultationRecordDto>>(records));
    }
}

public class VerificationService : IVerificationService
{
    private readonly IRepository<VerificationRequest> _verRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public VerificationService(IRepository<VerificationRequest> verRepo, IRepository<DoctorProfile> doctorRepo, IUnitOfWork uow, IMapper mapper)
    { _verRepo = verRepo; _doctorRepo = doctorRepo; _uow = uow; _mapper = mapper; }

    public async Task<ApiResponse<VerificationRequestDto>> SubmitVerificationAsync(Guid doctorUserId, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<VerificationRequestDto>.ErrorResponse("Doctor not found");
        if (doctor.VerificationStatus == VerificationStatus.Approved)
            return ApiResponse<VerificationRequestDto>.ErrorResponse("Already verified");

        doctor.VerificationStatus = VerificationStatus.Submitted;
        _doctorRepo.Update(doctor);

        var request = new VerificationRequest { DoctorProfileId = doctor.Id, Status = VerificationStatus.Submitted, DoctorProfile = doctor };
        await _verRepo.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<VerificationRequestDto>.SuccessResponse(_mapper.Map<VerificationRequestDto>(request), "Verification submitted");
    }

    public async Task<ApiResponse<VerificationRequestDto>> GetVerificationStatusAsync(Guid doctorUserId, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null) return ApiResponse<VerificationRequestDto>.ErrorResponse("Doctor not found");
        var all = await _verRepo.GetAllAsync(ct);
        var request = all.Where(v => v.DoctorProfileId == doctor.Id).OrderByDescending(v => v.CreatedAt).FirstOrDefault();
        if (request == null) return ApiResponse<VerificationRequestDto>.ErrorResponse("No verification request found");
        return ApiResponse<VerificationRequestDto>.SuccessResponse(_mapper.Map<VerificationRequestDto>(request));
    }

    public async Task<ApiResponse<VerificationRequestDto>> GetVerificationByIdAsync(Guid requestId, CancellationToken ct)
    {
        var request = await _verRepo.GetByIdAsync(requestId, ct);
        if (request == null) return ApiResponse<VerificationRequestDto>.ErrorResponse("Verification request not found");
        return ApiResponse<VerificationRequestDto>.SuccessResponse(_mapper.Map<VerificationRequestDto>(request));
    }

    public async Task<ApiResponse<List<VerificationRequestDto>>> GetPendingVerificationsAsync(int page, int pageSize, CancellationToken ct)
    {
        var all = await _verRepo.GetAllAsync(ct);
        var pending = all.Where(v => v.Status == VerificationStatus.Submitted).ToList();
        var total = pending.Count;
        var items = pending.OrderBy(v => v.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ApiResponse<List<VerificationRequestDto>>.SuccessResponse(_mapper.Map<List<VerificationRequestDto>>(items), pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
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
        if (doctor != null) { doctor.VerificationStatus = VerificationStatus.Approved; _doctorRepo.Update(doctor); }

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
        return ApiResponse<List<NotificationDto>>.SuccessResponse(_mapper.Map<List<NotificationDto>>(items), pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
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

    public async Task<ApiResponse<List<ServicePackageDto>>> GetActivePackagesAsync(CancellationToken ct)
    {
        var all = await _pkgRepo.GetAllAsync(ct);
        var active = all.Where(p => p.IsActive && !p.IsDeleted).OrderBy(p => p.DisplayOrder).ToList();
        return ApiResponse<List<ServicePackageDto>>.SuccessResponse(_mapper.Map<List<ServicePackageDto>>(active));
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
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AdminService(IRepository<User> userRepo, IRepository<DoctorProfile> doctorRepo, IRepository<PatientProfile> patientRepo, IRepository<Appointment> apptRepo, IRepository<AuditLog> auditRepo, IRepository<Specialization> specRepo, IRepository<VerificationRequest> verRepo, IRepository<BlogPost> blogRepo, IUnitOfWork uow, IMapper mapper)
    { _userRepo = userRepo; _doctorRepo = doctorRepo; _patientRepo = patientRepo; _apptRepo = apptRepo; _auditRepo = auditRepo; _specRepo = specRepo; _verRepo = verRepo; _blogRepo = blogRepo; _uow = uow; _mapper = mapper; }

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
}

public class TreatmentPackageService : ITreatmentPackageService
{
    private readonly IRepository<TreatmentPackage> _packageRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public TreatmentPackageService(
        IRepository<TreatmentPackage> packageRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<PatientProfile> patientRepo,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _packageRepo = packageRepo;
        _doctorRepo = doctorRepo;
        _patientRepo = patientRepo;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponse<TreatmentPackageDto>> CreateAsync(Guid doctorUserId, CreateTreatmentPackageDto dto, CancellationToken ct)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctor == null)
            return ApiResponse<TreatmentPackageDto>.ErrorResponse("Doctor not found");

        var patient = await _patientRepo.GetByIdAsync(dto.PatientId, ct);
        if (patient == null)
            return ApiResponse<TreatmentPackageDto>.ErrorResponse("Patient not found");

        var package = new TreatmentPackage
        {
            DoctorId = doctor.Id,
            PatientId = dto.PatientId,
            Name = dto.Name,
            Description = dto.Description,
            SessionQuantity = dto.SessionQuantity,
            RemainingSessions = dto.SessionQuantity,
            ValidityDays = dto.ValidityDays,
            ExpirationDate = DateTime.UtcNow.AddDays(dto.ValidityDays),
            Price = dto.Price,
            Status = TreatmentPackageStatus.Assigned,
            AssignedDate = DateTime.UtcNow,
            Doctor = doctor,
            Patient = patient
        };

        await _packageRepo.AddAsync(package, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<TreatmentPackageDto>.SuccessResponse(_mapper.Map<TreatmentPackageDto>(package), "Treatment package created and assigned to patient");
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
        return ApiResponse<List<TreatmentPackageDto>>.SuccessResponse(_mapper.Map<List<TreatmentPackageDto>>(items), pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<List<TreatmentPackageDto>>> GetByPatientAsync(Guid patientUserId, int page, int pageSize, CancellationToken ct)
    {
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        if (patient == null)
            return ApiResponse<List<TreatmentPackageDto>>.ErrorResponse("Patient not found");

        var all = await _packageRepo.GetAllAsync(ct);
        var packages = all.Where(p => p.PatientId == patient.Id && !p.IsDeleted).ToList();
        var total = packages.Count;
        var items = packages.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ApiResponse<List<TreatmentPackageDto>>.SuccessResponse(_mapper.Map<List<TreatmentPackageDto>>(items), pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }

    public async Task<ApiResponse<TreatmentPackageDto>> GetByIdAsync(Guid packageId, Guid userId, CancellationToken ct)
    {
        var package = await _packageRepo.GetByIdAsync(packageId, ct);
        if (package == null)
            return ApiResponse<TreatmentPackageDto>.ErrorResponse("Treatment package not found");
        return ApiResponse<TreatmentPackageDto>.SuccessResponse(_mapper.Map<TreatmentPackageDto>(package));
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
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Treatment package accepted and is now active");
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
}

public class SubscriptionService : ISubscriptionService
{
    private readonly IRepository<DoctorSubscription> _subRepo;
    private readonly IRepository<ServicePackage> _pkgRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<PaymentTransaction> _paymentRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public SubscriptionService(
        IRepository<DoctorSubscription> subRepo,
        IRepository<ServicePackage> pkgRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<PaymentTransaction> paymentRepo,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _subRepo = subRepo;
        _pkgRepo = pkgRepo;
        _doctorRepo = doctorRepo;
        _paymentRepo = paymentRepo;
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

        // In production, generate VNPay redirect URL here
        return ApiResponse<SubscriptionDto>.SuccessResponse(new SubscriptionDto
        {
            Id = subscription.Id,
            PackageName = package.Name,
            Status = subscription.Status.ToString(),
            StartDate = subscription.StartDate,
            ExpirationDate = subscription.ExpirationDate,
            CreatedAt = subscription.CreatedAt
        }, "Subscription created. Redirect to VNPay for payment.");
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
        // In production, verify VNPay hash, extract transaction info, update subscription status
        // For MVP: auto-activate subscription
        if (queryParams.TryGetValue("vnp_TxnRef", out var txnRef) &&
            Guid.TryParse(txnRef, out var subscriptionId))
        {
            var sub = await _subRepo.GetByIdAsync(subscriptionId, ct);
            if (sub != null && sub.Status == SubscriptionStatus.PendingPayment)
            {
                sub.Status = SubscriptionStatus.Active;
                sub.UpdatedAt = DateTime.UtcNow;
                _subRepo.Update(sub);
                await _uow.SaveChangesAsync(ct);
                return ApiResponse.SuccessResponse("Payment processed, subscription activated");
            }
        }

        return ApiResponse.ErrorResponse("Invalid payment callback");
    }
}
