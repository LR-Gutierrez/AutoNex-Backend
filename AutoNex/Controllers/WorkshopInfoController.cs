using AutoNex.DTOs;
using AutoNex.DTOs.WorkshopInfo;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/workshop-info")]
[Authorize(Roles = "Admin")]
public class WorkshopInfoController : ControllerBase
{
    private readonly IWorkshopInfoService _workshopInfoService;

    public WorkshopInfoController(IWorkshopInfoService workshopInfoService)
    {
        _workshopInfoService = workshopInfoService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var info = await _workshopInfoService.GetAsync(cancellationToken);
        if (info is null)
            return NotFound(ApiResponse<WorkshopInfoResponse>.Fail("No hay información del taller configurada"));

        return Ok(ApiResponse<WorkshopInfoResponse>.Ok(info));
    }

    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpdateWorkshopInfoRequest request, CancellationToken cancellationToken)
    {
        var info = await _workshopInfoService.UpsertAsync(request, cancellationToken);
        return Ok(ApiResponse<WorkshopInfoResponse>.Ok(info, "Información del taller guardada exitosamente"));
    }
}
