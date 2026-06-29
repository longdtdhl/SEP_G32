namespace OPCBS.Web.DTOs;

public class DoctorDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? Specialization { get; set; }
    public List<string> Specializations { get; set; } = new();
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public bool IsVerified { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
}

public class DoctorListItemDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Specialization { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public bool IsVerified { get; set; }
}

public class DoctorFilterDto
{
    public string? Search { get; set; }
    public string? Specialization { get; set; }
    public double? MinRating { get; set; }
    public decimal? MaxFee { get; set; }
    public string? Gender { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
