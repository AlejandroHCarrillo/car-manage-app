namespace PersonalCarGarage.Domain.Entities;

public class MaintenanceRecord
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ServiceDate { get; set; }
    public int Mileage { get; set; }
    public decimal Cost { get; set; }
    public DateTime CreatedAt { get; set; }
}
