namespace OPCBS.Web.DTOs;

public class ServicePackageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public int MaxAppointments { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Features { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CreateServicePackageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public int MaxAppointments { get; set; }
    public List<string> Features { get; set; } = new();
}

public class UpdateServicePackageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public int MaxAppointments { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Features { get; set; } = new();
}
