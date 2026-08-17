using OPCBS.Domain.Common;
using OPCBS.Domain.Enums;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Conversation entity - represents a messaging thread between a doctor and a patient.
/// Auto-created when appointment is approved or treatment package is active.
/// </summary>
public class Conversation : BaseEntity
{
    /// <summary>Foreign key to PatientProfile (via UserId)</summary>
    public Guid PatientId { get; set; }

    /// <summary>Foreign key to DoctorProfile (via UserId)</summary>
    public Guid DoctorId { get; set; }

    /// <summary>Foreign key to Appointment that triggered this conversation (nullable)</summary>
    public Guid? AppointmentId { get; set; }

    /// <summary>Foreign key to TreatmentPackage that triggered this conversation (nullable)</summary>
    public Guid? TreatmentPackageId { get; set; }

    /// <summary>Conversation status (Open / Closed)</summary>
    public ConversationStatus Status { get; set; } = ConversationStatus.Open;

    /// <summary>Navigation property to PatientProfile</summary>
    public virtual required PatientProfile Patient { get; set; }

    /// <summary>Navigation property to DoctorProfile</summary>
    public virtual required DoctorProfile Doctor { get; set; }

    /// <summary>Navigation property to Appointment (optional)</summary>
    public virtual Appointment? Appointment { get; set; }

    /// <summary>Navigation property to TreatmentPackage (optional)</summary>
    public virtual TreatmentPackage? TreatmentPackage { get; set; }

    /// <summary>Navigation property: messages in this conversation</summary>
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

/// <summary>
/// Message entity - a single message within a conversation.
/// Uses ImmutableEntity because messages cannot be deleted (CHAT-05, CHAT-07).
/// </summary>
public class Message : ImmutableEntity
{
    /// <summary>Foreign key to Conversation</summary>
    public Guid ConversationId { get; set; }

    /// <summary>Sender's UserId (can be doctor or patient)</summary>
    public Guid SenderId { get; set; }

    /// <summary>Message text content</summary>
    public string? Content { get; set; }

    /// <summary>URL to attachment (image or PDF only)</summary>
    public string? AttachmentUrl { get; set; }

    /// <summary>Whether the message has been read by the recipient</summary>
    public bool IsRead { get; set; } = false;

    /// <summary>Navigation property to Conversation</summary>
    public virtual required Conversation Conversation { get; set; }
}
