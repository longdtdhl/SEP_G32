using OPCBS.Application.DTOs.Favorites;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Entities;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Services;

public class FavoriteDoctorService : IFavoriteDoctorService
{
    private readonly IRepository<FavoriteDoctor> _favRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _uow;

    public FavoriteDoctorService(
        IRepository<FavoriteDoctor> favRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<User> userRepo,
        IUnitOfWork uow)
    {
        _favRepo = favRepo;
        _patientRepo = patientRepo;
        _doctorRepo = doctorRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<ApiResponse<List<FavoriteDoctorDto>>> GetFavoritesAsync(Guid patientUserId, CancellationToken ct)
    {
        var allFavs = await _favRepo.GetAllAsync(ct);
        var myFavorites = allFavs.Where(f => f.PatientId == patientUserId).OrderByDescending(f => f.CreatedAt).ToList();

        if (!myFavorites.Any())
            return ApiResponse<List<FavoriteDoctorDto>>.SuccessResponse(new List<FavoriteDoctorDto>());

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var allUsers = await _userRepo.GetAllAsync(ct);

        var favorites = new List<FavoriteDoctorDto>();
        foreach (var f in myFavorites)
        {
            // Match doctor either by UserId or DoctorProfile.Id
            var doc = allDoctors.FirstOrDefault(d => d.UserId == f.DoctorId || d.Id == f.DoctorId);
            var doctorUserId = doc?.UserId ?? f.DoctorId;
            var user = allUsers.FirstOrDefault(u => u.Id == doctorUserId);

            favorites.Add(new FavoriteDoctorDto
            {
                Id = f.Id,
                DoctorId = doc != null ? doc.Id : f.DoctorId,
                DoctorName = user?.FullName ?? "N/A",
                AvatarUrl = user?.AvatarUrl,
                ProfessionalTitle = doc?.ProfessionalTitle,
                Specializations = new List<string>(),
                AverageRating = doc?.AverageRating ?? 0,
                ReviewCount = doc?.ReviewCount ?? 0,
                CreatedAt = f.CreatedAt
            });
        }

        return ApiResponse<List<FavoriteDoctorDto>>.SuccessResponse(favorites);
    }

    public async Task<ApiResponse<FavoriteDoctorDto>> AddFavoriteAsync(Guid patientUserId, Guid doctorId, CancellationToken ct)
    {
        // Check patient exists
        var patients = await _patientRepo.GetAllAsync(ct);
        var patient = patients.FirstOrDefault(p => p.UserId == patientUserId);
        if (patient == null)
            return ApiResponse<FavoriteDoctorDto>.ErrorResponse("Patient profile not found.");

        // Check doctor exists (match by UserId or DoctorProfile.Id)
        var doctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = doctors.FirstOrDefault(d => d.UserId == doctorId || d.Id == doctorId);
        if (doctor == null)
            return ApiResponse<FavoriteDoctorDto>.ErrorResponse("Doctor not found.");

        var targetDoctorUserId = doctor.UserId;

        // Check duplicate
        var all = await _favRepo.GetAllAsync(ct);
        var existing = all.FirstOrDefault(f => f.PatientId == patientUserId && (f.DoctorId == targetDoctorUserId || f.DoctorId == doctor.Id));
        if (existing != null)
            return ApiResponse<FavoriteDoctorDto>.ErrorResponse("Doctor is already in your favorites.");

        var favorite = new FavoriteDoctor
        {
            PatientId = patientUserId,
            DoctorId = targetDoctorUserId,
            Patient = patient,
            Doctor = doctor
        };

        await _favRepo.AddAsync(favorite, ct);
        await _uow.SaveChangesAsync(ct);

        var allUsers = await _userRepo.GetAllAsync(ct);
        var user = allUsers.FirstOrDefault(u => u.Id == targetDoctorUserId);

        var dto = new FavoriteDoctorDto
        {
            Id = favorite.Id,
            DoctorId = doctor.Id,
            DoctorName = user?.FullName ?? "N/A",
            AvatarUrl = user?.AvatarUrl,
            ProfessionalTitle = doctor.ProfessionalTitle,
            Specializations = new List<string>(),
            AverageRating = doctor.AverageRating,
            ReviewCount = doctor.ReviewCount,
            CreatedAt = favorite.CreatedAt
        };

        return ApiResponse<FavoriteDoctorDto>.SuccessResponse(dto, "Doctor added to favorites.");
    }

    public async Task<ApiResponse<bool>> RemoveFavoriteAsync(Guid patientUserId, Guid doctorId, CancellationToken ct)
    {
        var doctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = doctors.FirstOrDefault(d => d.UserId == doctorId || d.Id == doctorId);
        var targetUserId = doctor?.UserId ?? doctorId;

        var all = await _favRepo.GetAllAsync(ct);
        var favorite = all.FirstOrDefault(f => f.PatientId == patientUserId && (f.DoctorId == targetUserId || (doctor != null && f.DoctorId == doctor.Id)));
        if (favorite == null)
            return ApiResponse<bool>.ErrorResponse("Favorite not found.");

        _favRepo.Delete(favorite);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<bool>.SuccessResponse(true, "Doctor removed from favorites.");
    }

    public async Task<ApiResponse<bool>> IsFavoriteAsync(Guid patientUserId, Guid doctorId, CancellationToken ct)
    {
        var doctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = doctors.FirstOrDefault(d => d.UserId == doctorId || d.Id == doctorId);
        var targetUserId = doctor?.UserId ?? doctorId;

        var all = await _favRepo.GetAllAsync(ct);
        var exists = all.Any(f => f.PatientId == patientUserId && (f.DoctorId == targetUserId || (doctor != null && f.DoctorId == doctor.Id)));
        return ApiResponse<bool>.SuccessResponse(exists);
    }

    private static FavoriteDoctorDto MapToDto(FavoriteDoctor f) => new()
    {
        Id = f.Id,
        DoctorId = f.DoctorId,
        DoctorName = f.Doctor?.User?.FullName ?? "N/A",
        AvatarUrl = f.Doctor?.User?.AvatarUrl,
        ProfessionalTitle = f.Doctor?.ProfessionalTitle,
        Specializations = f.Doctor?.DoctorSpecializations?
            .Select(ds => ds.Specialization.Name).ToList() ?? new List<string>(),
        AverageRating = f.Doctor?.AverageRating ?? 0,
        ReviewCount = f.Doctor?.ReviewCount ?? 0,
        CreatedAt = f.CreatedAt
    };
}
