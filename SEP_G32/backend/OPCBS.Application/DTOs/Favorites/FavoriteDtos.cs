namespace OPCBS.Application.DTOs.Favorites;

/// <summary>DTO returned when listing favorite doctors</summary>
public class FavoriteDoctorDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? ProfessionalTitle { get; set; }
    public List<string> Specializations { get; set; } = new();
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
