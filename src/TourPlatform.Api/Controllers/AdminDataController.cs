using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourPlatform.Application.Contracts;

namespace TourPlatform.API.Controllers;

[ApiController]
[Route("api/data")]
[Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
public class AdminDataController : ControllerBase
{
    private readonly IAdminDataService _service;

    public AdminDataController(IAdminDataService service)
    {
        _service = service;
    }

    [HttpGet("{tourOperatorId}")]
    public async Task<IActionResult> GetPricingData(
        int tourOperatorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 5000) pageSize = 100; 

        var result = await _service.GetToursDataPerOperatorAsync(tourOperatorId, page, pageSize, ct);

        return Ok(result);
    }
}
