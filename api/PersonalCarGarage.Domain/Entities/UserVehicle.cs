namespace PersonalCarGarage.Domain.Entities;

public class UserVehicle
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public DateTime AssignedAt { get; set; }
}
