using System.Text.Json.Serialization;

namespace OPCBS.Web.DTOs;

public class DoctorDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    // Backend returns "biography"
    [JsonPropertyName("biography")]
    public string? Bio { get; set; }

    // Backend returns "professionalTitle"
    [JsonPropertyName("professionalTitle")]
    public string? Specialization { get; set; }

    public List<string> Specializations { get; set; } = new();

    // Backend returns "averageRating"
    [JsonPropertyName("averageRating")]
    public double Rating { get; set; }

    public int ReviewCount { get; set; }
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }

    // Backend returns verificationStatus as int enum (0=Draft, 1=Submitted, 2=Approved, 3=Rejected)
    [JsonPropertyName("verificationStatus")]
    public int VerificationStatusRaw { get; set; }

    [JsonIgnore]
    public bool IsVerified => VerificationStatusRaw == 2;

    public bool IsVisible { get; set; }
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

    // Backend returns "professionalTitle" 
    [JsonPropertyName("professionalTitle")]
    public string? Specialization { get; set; }

    // Backend returns "averageRating"
    [JsonPropertyName("averageRating")]
    public double Rating { get; set; }

    public int ReviewCount { get; set; }
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }

    // Backend returns verificationStatus as int enum (0=Draft, 1=Submitted, 2=Approved, 3=Rejected)
    [JsonPropertyName("verificationStatus")]
    public int VerificationStatusRaw { get; set; }

    [JsonIgnore]
    public bool IsVerified => VerificationStatusRaw == 2;

    public List<string>? Specializations { get; set; }
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

public class UpdateDoctorProfileDto
{
    [JsonPropertyName("professionalTitle")]
    public string? ProfessionalTitle { get; set; }

    [JsonPropertyName("biography")]
    public string? Biography { get; set; }

    [JsonPropertyName("experienceYears")]
    public int? ExperienceYears { get; set; }

    [JsonPropertyName("specializationIds")]
    public List<Guid>? SpecializationIds { get; set; }

    [JsonPropertyName("isVisible")]
    public bool? IsVisible { get; set; }
}
