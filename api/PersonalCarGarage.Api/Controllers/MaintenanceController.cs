using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalCarGarage.Application.Maintenance;

namespace PersonalCarGarage.Api.Controllers;

[ApiController]
[Authorize]
public class MaintenanceController(IMaintenanceService maintenanceService) : ControllerBase
{
    [HttpGet("api/vehicles/{vehicleId:guid}/maintenance")]
    public async Task<ActionResult<IReadOnlyList<MaintenanceResponse>>> GetByVehicle(Guid vehicleId, CancellationToken cancellationToken)
    {
        return Ok(await maintenanceService.GetByVehicleAsync(vehicleId, cancellationToken));
    }

    [HttpPost("api/vehicles/{vehicleId:guid}/maintenance")]
    public async Task<ActionResult<MaintenanceResponse>> Create(Guid vehicleId, [FromBody] UpsertMaintenanceRequest request, CancellationToken cancellationToken)
    {
        var created = await maintenanceService.CreateAsync(vehicleId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("api/maintenance/{id:guid}")]
    public async Task<ActionResult<MaintenanceResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await maintenanceService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPut("api/maintenance/{id:guid}")]
    public async Task<ActionResult<MaintenanceResponse>> Update(Guid id, [FromBody] UpsertMaintenanceRequest request, CancellationToken cancellationToken)
    {
        return Ok(await maintenanceService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("api/maintenance/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await maintenanceService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
