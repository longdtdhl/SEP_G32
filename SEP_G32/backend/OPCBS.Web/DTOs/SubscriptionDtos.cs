namespace OPCBS.Web.DTOs;

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid ServicePackageId { get; set; }
    public string? PackageName { get; set; }
    public string? Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSubscriptionDto
{
    public Guid ServicePackageId { get; set; }
}

public class SpecializationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DoctorCount { get; set; }
}

public class CreateSpecializationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
