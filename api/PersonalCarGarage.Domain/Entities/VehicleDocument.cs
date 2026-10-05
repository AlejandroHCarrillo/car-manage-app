using PersonalCarGarage.Domain.Enums;

namespace PersonalCarGarage.Domain.Entities;

public class VehicleDocument
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public DocumentKind Kind { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? IssuedOn { get; set; }
    public DateTime? ExpiresOn { get; set; }
    public DateTime CreatedAt { get; set; }
}
