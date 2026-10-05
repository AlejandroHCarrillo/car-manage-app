using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalCarGarage.Application.Vehicles;

namespace PersonalCarGarage.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiclesController(IVehicleService vehicleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VehicleResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await vehicleService.GetAllAsync(cancellationToken));
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatsResponse>> Dashboard(CancellationToken cancellationToken)
    {
        return Ok(await vehicleService.GetDashboardAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await vehicleService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<VehicleResponse>> Create([FromBody] UpsertVehicleRequest request, CancellationToken cancellationToken)
    {
        var created = await vehicleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VehicleResponse>> Update(Guid id, [FromBody] UpsertVehicleRequest request, CancellationToken cancellationToken)
    {
        return Ok(await vehicleService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await vehicleService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/managers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignManager(Guid id, [FromBody] AssignManagerRequest request, CancellationToken cancellationToken)
    {
        await vehicleService.AssignManagerAsync(id, request.UserId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/managers/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnassignManager(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await vehicleService.UnassignManagerAsync(id, userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/photos")]
    [RequestSizeLimit(6_291_456)]
    public async Task<ActionResult<VehiclePhotoResponse>> AddPhoto(Guid id, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Selecciona una imagen." });
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { message = "Cada foto debe pesar máximo 5 MB." });
        }

        await using var stream = file.OpenReadStream();
        var photo = await vehicleService.AddPhotoAsync(id, file.FileName, file.ContentType, stream, cancellationToken);
        return Ok(photo);
    }

    [HttpDelete("{id:guid}/photos/{photoId:guid}")]
    public async Task<IActionResult> DeletePhoto(Guid id, Guid photoId, CancellationToken cancellationToken)
    {
        await vehicleService.DeletePhotoAsync(id, photoId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/documents")]
    [RequestSizeLimit(11_534_336)]
    public async Task<ActionResult<VehicleDocumentResponse>> AddDocument(
        Guid id,
        [FromForm] IFormFile file,
        [FromForm] string title,
        [FromForm] string kind,
        [FromForm] string? notes,
        [FromForm] string? issuedOn,
        [FromForm] string? expiresOn,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Selecciona un archivo." });
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new { message = "Cada documento debe pesar máximo 10 MB." });
        }

        if (!Enum.TryParse<PersonalCarGarage.Domain.Enums.DocumentKind>(kind, true, out var documentKind))
        {
            return BadRequest(new { message = "El tipo de documento no es válido." });
        }

        DateTime? issued = DateTime.TryParse(issuedOn, out var issuedOnValue) ? issuedOnValue : null;
        DateTime? expires = DateTime.TryParse(expiresOn, out var expiresOnValue) ? expiresOnValue : null;

        await using var stream = file.OpenReadStream();
        var document = await vehicleService.AddDocumentAsync(
            id,
            title,
            documentKind,
            file.FileName,
            file.ContentType,
            stream,
            notes,
            issued,
            expires,
            cancellationToken);
        return Ok(document);
    }

    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid id, Guid documentId, CancellationToken cancellationToken)
    {
        await vehicleService.DeleteDocumentAsync(id, documentId, cancellationToken);
        return NoContent();
    }
}
