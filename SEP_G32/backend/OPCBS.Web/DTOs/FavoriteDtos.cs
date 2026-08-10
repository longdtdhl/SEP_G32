namespace OPCBS.Web.DTOs;

/// <summary>DTO for favorite doctor display in Web layer</summary>
public class FavoriteDoctorWebDto
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
