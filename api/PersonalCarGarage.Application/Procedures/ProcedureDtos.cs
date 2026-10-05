using PersonalCarGarage.Domain.Enums;

namespace PersonalCarGarage.Application.Procedures;

public record ProcedureResponse(
    Guid Id,
    Guid VehicleId,
    string VehicleName,
    string LicensePlate,
    string Title,
    string? Description,
    ProcedureFrequency Frequency,
    DateTime DueDate,
    decimal? LastCost,
    bool IsCompleted,
    string Status,
    int CompletionCount,
    DateTime CreatedAt);

public record ProcedureCompletionResponse(
    Guid Id,
    Guid ProcedureId,
    DateTime CompletedAt,
    DateTime DueDateCovered,
    decimal Cost,
    string? Notes,
    string CreatedByName,
    DateTime CreatedAt);

public record UpsertProcedureRequest(
    string Title,
    string? Description,
    ProcedureFrequency Frequency,
    DateTime DueDate);

public record CompleteProcedureRequest(
    DateTime CompletedAt,
    decimal Cost,
    string? Notes);
