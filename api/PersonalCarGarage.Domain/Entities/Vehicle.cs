namespace PersonalCarGarage.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string? Vin { get; set; }
    public string Color { get; set; } = string.Empty;
    public int CurrentMileage { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<UserVehicle> UserVehicles { get; set; } = new List<UserVehicle>();
    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
    public ICollection<VehicleProcedure> Procedures { get; set; } = new List<VehicleProcedure>();
    public ICollection<VehiclePhoto> Photos { get; set; } = new List<VehiclePhoto>();
    public ICollection<VehicleDocument> Documents { get; set; } = new List<VehicleDocument>();

    public string DisplayName => $"{Brand} {Model} ({Year})";
}
