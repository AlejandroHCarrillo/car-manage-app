using PersonalCarGarage.Domain.Enums;

namespace PersonalCarGarage.Domain.Entities;

public class VehicleProcedure
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProcedureFrequency Frequency { get; set; }
    public DateTime DueDate { get; set; }
    public decimal? LastCost { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ProcedureCompletion> Completions { get; set; } = new List<ProcedureCompletion>();

    public bool IsPeriodic => Frequency != ProcedureFrequency.Once;

    public DateTime NextDueDate(DateTime fromDueDate) => Frequency switch
    {
        ProcedureFrequency.Monthly => fromDueDate.AddMonths(1),
        ProcedureFrequency.Semiannual => fromDueDate.AddMonths(6),
        ProcedureFrequency.Annual => fromDueDate.AddYears(1),
        _ => fromDueDate
    };
}
