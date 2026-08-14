using OPCBS.Domain.Common;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Reusable custom clinical field attached to appointments, consultation notes, or treatment packages.
/// </summary>
public class CustomClinicalField : BaseEntity
{
    /// <summary>Owner type: "PreAppointmentEvaluation", "ConsultationNote", "TreatmentPackage"</summary>
    public required string OwnerType { get; set; }

    /// <summary>Foreign key / ID of the owner entity (AppointmentSlotId / AppointmentId / ConsultationNoteId / TreatmentPackageId)</summary>
    public Guid OwnerId { get; set; }

    /// <summary>Section key: "BasicInformation", "ClinicalGuidelines", "PreAppointmentEvaluation", "ConsultationNote"</summary>
    public required string SectionKey { get; set; }

    /// <summary>Field title or label</summary>
    public required string Title { get; set; }

    /// <summary>Field content, clinical text, doctor instructions, or patient response</summary>
    public string? Content { get; set; }

    /// <summary>Field type: "Text", "LongText", "Instruction"</summary>
    public string FieldType { get; set; } = "Text";

    /// <summary>Display order index</summary>
    public int OrderIndex { get; set; } = 0;

    /// <summary>Doctor user ID or doctor profile ID who created or configured the field</summary>
    public Guid? CreatedByDoctorId { get; set; }
}
