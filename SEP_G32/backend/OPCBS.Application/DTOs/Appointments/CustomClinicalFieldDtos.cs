namespace OPCBS.Application.DTOs.Appointments;

public class CustomClinicalFieldDto
{
    public Guid Id { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string SectionKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string FieldType { get; set; } = "Text"; // Text, LongText, Instruction
    public int OrderIndex { get; set; } = 0;
    public Guid? CreatedByDoctorId { get; set; }
}

public class CreateCustomClinicalFieldDto
{
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string SectionKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string FieldType { get; set; } = "Text";
    public int OrderIndex { get; set; } = 0;
}

public class UpdateCustomClinicalFieldDto
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string FieldType { get; set; } = "Text";
    public int OrderIndex { get; set; } = 0;
}
