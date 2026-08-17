namespace OPCBS.Web.DTOs;

public class TreatmentPackageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TargetOutcome { get; set; }
    public string? RecommendedExercises { get; set; }
    public string? Instructions { get; set; }
    public Guid DoctorId { get; set; }
    public Guid DoctorProfileId { get; set; }
    public string? DoctorName { get; set; }
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public int SessionQuantity { get; set; }
    public int RemainingSessions { get; set; }
    public int ValidityDays { get; set; }
    public int RecommendedSessionsPerWeek { get; set; } = 1;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AssignedDate { get; set; }
    public DateTime? AcceptedDate { get; set; }
    public DateTime? ActiveDate { get; set; }
    public Guid? CancellationRequestedByUserId { get; set; }
    public string? CancellationRequestedByName { get; set; }
    public DateTime? CancellationRequestedAt { get; set; }
    public string? CancellationReason { get; set; }

    public List<CustomClinicalFieldDto>? CustomFields { get; set; }
    public List<CustomClinicalFieldDto> BasicInformationFields => CustomFields?.Where(f => f.SectionKey == "BasicInformation").ToList() ?? new();
    public List<CustomClinicalFieldDto> ClinicalGuidelinesFields => CustomFields?.Where(f => f.SectionKey == "ClinicalGuidelines").ToList() ?? new();

    // Aliases for views
    public string Title => Name;
    public int TotalSessions => SessionQuantity;
    public int CompletedSessions => SessionQuantity - RemainingSessions;
    public bool IsExpired => ExpirationDate < DateTime.Now;
    public string DisplayPatientName => PatientName ?? "Template (Not assigned)";
    public bool IsTemplate => PatientId == null;
    public bool IsCancellationPending => Status == "CancellationPending";
}

public class CreateTreatmentPackageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TargetOutcome { get; set; }
    public string? RecommendedExercises { get; set; }
    public string? Instructions { get; set; }
    public int SessionQuantity { get; set; } = 8;
    public decimal Price { get; set; }
    public Guid? PatientId { get; set; }
    public int ValidityDays { get; set; } = 90;
    public int RecommendedSessionsPerWeek { get; set; } = 1;

    public List<CreateCustomClinicalFieldDto> BasicInformationFields { get; set; } = new();
    public List<CreateCustomClinicalFieldDto> ClinicalGuidelinesFields { get; set; } = new();

    public List<CreateCustomClinicalFieldDto>? CustomFields
    {
        get
        {
            var list = new List<CreateCustomClinicalFieldDto>();
            if (BasicInformationFields != null)
            {
                foreach (var f in BasicInformationFields)
                {
                    f.SectionKey = "BasicInformation";
                    list.Add(f);
                }
            }
            if (ClinicalGuidelinesFields != null)
            {
                foreach (var f in ClinicalGuidelinesFields)
                {
                    f.SectionKey = "ClinicalGuidelines";
                    list.Add(f);
                }
            }
            return list;
        }
        set
        {
            if (value != null)
            {
                BasicInformationFields = value.Where(f => f.SectionKey == "BasicInformation").ToList();
                ClinicalGuidelinesFields = value.Where(f => f.SectionKey == "ClinicalGuidelines").ToList();
            }
        }
    }

    // Read-write aliases for Razor form binding
    public string Title { get => Name; set => Name = value; }
    public int TotalSessions { get => SessionQuantity; set => SessionQuantity = value; }
}

public class UpdateTreatmentPackageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TargetOutcome { get; set; }
    public string? RecommendedExercises { get; set; }
    public string? Instructions { get; set; }
    public int SessionQuantity { get; set; }
    public int ValidityDays { get; set; } = 90;
    public int RecommendedSessionsPerWeek { get; set; } = 1;
    public decimal Price { get; set; }

    public List<CreateCustomClinicalFieldDto> BasicInformationFields { get; set; } = new();
    public List<CreateCustomClinicalFieldDto> ClinicalGuidelinesFields { get; set; } = new();

    public List<CreateCustomClinicalFieldDto>? CustomFields
    {
        get
        {
            var list = new List<CreateCustomClinicalFieldDto>();
            if (BasicInformationFields != null)
            {
                foreach (var f in BasicInformationFields)
                {
                    f.SectionKey = "BasicInformation";
                    list.Add(f);
                }
            }
            if (ClinicalGuidelinesFields != null)
            {
                foreach (var f in ClinicalGuidelinesFields)
                {
                    f.SectionKey = "ClinicalGuidelines";
                    list.Add(f);
                }
            }
            return list;
        }
        set
        {
            if (value != null)
            {
                BasicInformationFields = value.Where(f => f.SectionKey == "BasicInformation").ToList();
                ClinicalGuidelinesFields = value.Where(f => f.SectionKey == "ClinicalGuidelines").ToList();
            }
        }
    }

    // Read-write aliases for Razor form binding
    public string Title { get => Name; set => Name = value; }
    public int TotalSessions { get => SessionQuantity; set => SessionQuantity = value; }
}
