namespace WMS.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// Concurrency token for optimistic concurrency control
    /// Automatically managed by EF Core to prevent lost updates
    /// </summary>
    public byte[] RowVersion { get; set; } = null!;
}
