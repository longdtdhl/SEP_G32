using System.Text.Json.Serialization;

namespace OPCBS.Web.DTOs;

public class PatientRecordDto
{
    public Guid Id { get; set; }
    public Guid? PatientId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string? GuestPhone { get; set; }
    public string? GuestEmail { get; set; }
    public DateTime? GuestDateOfBirth { get; set; }
    public string? GuestGender { get; set; }
    public string? GuestAddress { get; set; }
    
    public string? PsychologicalHistory { get; set; }
    public string? CurrentSymptoms { get; set; }
    public string? StressFactors { get; set; }
    public string? GeneralNotes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // Registered-patient fields are supplied by the API; guest values remain the fallback.
    [JsonPropertyName("displayName")]
    public string? ApiDisplayName { get; set; }

    [JsonPropertyName("displayPhone")]
    public string? ApiDisplayPhone { get; set; }

    [JsonPropertyName("displayEmail")]
    public string? ApiDisplayEmail { get; set; }

    [JsonIgnore]
    public string ResolvedDisplayName => !string.IsNullOrWhiteSpace(ApiDisplayName)
        ? ApiDisplayName
        : (!string.IsNullOrWhiteSpace(GuestName) ? GuestName : "Not recorded");

    [JsonIgnore]
    public string? ResolvedDisplayPhone => !string.IsNullOrWhiteSpace(ApiDisplayPhone)
        ? ApiDisplayPhone
        : GuestPhone;

    [JsonIgnore]
    public string? ResolvedDisplayEmail => !string.IsNullOrWhiteSpace(ApiDisplayEmail)
        ? ApiDisplayEmail
        : GuestEmail;

    [JsonIgnore]
    public DateTime? ResolvedDateOfBirth => DateOfBirth ?? GuestDateOfBirth;
    [JsonIgnore]
    public string? ResolvedGender => Gender ?? GuestGender;
    [JsonIgnore]
    public string? ResolvedAddress => Address ?? GuestAddress;

    [JsonIgnore]
    public string DisplayName => string.IsNullOrEmpty(GuestName) ? "Not updated" : GuestName;
    [JsonIgnore]
    public string DisplayPhone => string.IsNullOrEmpty(GuestPhone) ? "Not updated" : GuestPhone;
    [JsonIgnore]
    public string DisplayEmail => string.IsNullOrEmpty(GuestEmail) ? "Not updated" : GuestEmail;
    [JsonIgnore]
    public bool IsGuest => !PatientId.HasValue;
}

public class CreatePatientRecordDto
{
    public Guid? PatientId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string? GuestPhone { get; set; }
    public string? GuestEmail { get; set; }
    public DateTime? GuestDateOfBirth { get; set; }
    public string? GuestGender { get; set; }
    public string? GuestAddress { get; set; }
    
    public string? PsychologicalHistory { get; set; }
    public string? CurrentSymptoms { get; set; }
    public string? StressFactors { get; set; }
    public string? GeneralNotes { get; set; }
}

public class UpdatePatientRecordDto : CreatePatientRecordDto { }
