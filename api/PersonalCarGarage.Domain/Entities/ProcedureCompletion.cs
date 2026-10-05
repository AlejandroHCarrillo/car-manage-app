namespace PersonalCarGarage.Domain.Entities;

public class ProcedureCompletion
{
    public Guid Id { get; set; }
    public Guid ProcedureId { get; set; }
    public VehicleProcedure Procedure { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public DateTime CompletedAt { get; set; }
    public DateTime DueDateCovered { get; set; }
    public decimal Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
