using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalCarGarage.Application.Procedures;

namespace PersonalCarGarage.Api.Controllers;

[ApiController]
[Authorize]
public class ProceduresController(IProcedureService procedureService) : ControllerBase
{
    [HttpGet("api/procedures")]
    public async Task<ActionResult<IReadOnlyList<ProcedureResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await procedureService.GetAllAsync(cancellationToken));
    }

    [HttpGet("api/vehicles/{vehicleId:guid}/procedures")]
    public async Task<ActionResult<IReadOnlyList<ProcedureResponse>>> GetByVehicle(Guid vehicleId, CancellationToken cancellationToken)
    {
        return Ok(await procedureService.GetByVehicleAsync(vehicleId, cancellationToken));
    }

    [HttpPost("api/vehicles/{vehicleId:guid}/procedures")]
    public async Task<ActionResult<ProcedureResponse>> Create(Guid vehicleId, [FromBody] UpsertProcedureRequest request, CancellationToken cancellationToken)
    {
        var created = await procedureService.CreateAsync(vehicleId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("api/procedures/{id:guid}")]
    public async Task<ActionResult<ProcedureResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await procedureService.GetByIdAsync(id, cancellationToken));
    }

    [HttpGet("api/procedures/{id:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<ProcedureCompletionResponse>>> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await procedureService.GetHistoryAsync(id, cancellationToken));
    }

    [HttpPut("api/procedures/{id:guid}")]
    public async Task<ActionResult<ProcedureResponse>> Update(Guid id, [FromBody] UpsertProcedureRequest request, CancellationToken cancellationToken)
    {
        return Ok(await procedureService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpPost("api/procedures/{id:guid}/complete")]
    public async Task<ActionResult<ProcedureResponse>> Complete(Guid id, [FromBody] CompleteProcedureRequest request, CancellationToken cancellationToken)
    {
        return Ok(await procedureService.CompleteAsync(id, request, cancellationToken));
    }

    [HttpDelete("api/procedures/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await procedureService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
