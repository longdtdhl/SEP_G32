namespace OPCBS.Web.DTOs;

public class TreatmentPackageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DoctorName { get; set; }
    public string? PatientName { get; set; }
    public int SessionQuantity { get; set; }
    public int RemainingSessions { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }

    // Aliases for views
    public string Title => Name;
    public int TotalSessions => SessionQuantity;
    public int CompletedSessions => SessionQuantity - RemainingSessions;
}

public class CreateTreatmentPackageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SessionQuantity { get; set; }
    public decimal Price { get; set; }
    public Guid? PatientId { get; set; }
    public int ValidityDays { get; set; } = 90;

    // Read-write aliases for Razor form binding
    public string Title { get => Name; set => Name = value; }
    public int TotalSessions { get => SessionQuantity; set => SessionQuantity = value; }
}

public class UpdateTreatmentPackageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SessionQuantity { get; set; }
    public decimal Price { get; set; }

    // Read-write aliases for Razor form binding
    public string Title { get => Name; set => Name = value; }
    public int TotalSessions { get => SessionQuantity; set => SessionQuantity = value; }
}
