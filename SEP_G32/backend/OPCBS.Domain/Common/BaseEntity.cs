namespace OPCBS.Domain.Common;

/// <summary>
/// Base entity class with audit fields and soft delete support.
/// All aggregate entities inherit from this class.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity (GUID)
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when entity was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User ID who created this entity
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// User ID who last updated this entity
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete flag - true if entity is logically deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// Base entity without soft delete support for purely historical records
/// </summary>
public abstract class ImmutableEntity
{
    /// <summary>
    /// Unique identifier for the entity (GUID)
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID who created this entity
    /// </summary>
    public Guid? CreatedBy { get; set; }
}
