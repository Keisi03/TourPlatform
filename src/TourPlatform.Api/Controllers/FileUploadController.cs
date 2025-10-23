using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TourPlatform.Application.Contracts;
using TourPlatform.Infrastructure.Entities;
using TourPlatform.Infrastructure.Hubs;

namespace TourPlatform.API.Controllers;

[ApiController]
[Route("api/touroperators/{tourOperatorId}/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = "TourOperator")]
public class FileUploadController : ControllerBase
{
    private readonly IFileProcessorService _fileProcessor;
    private readonly AppDbContext _db;
    private readonly IHubContext<UploadProgressHub> _hub;

    public FileUploadController(
        IFileProcessorService fileProcessor,
        AppDbContext db,
        IHubContext<UploadProgressHub> hub)
    {
        _fileProcessor = fileProcessor;
        _db = db;
        _hub = hub;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadPricingCsv(
        int tourOperatorId,
        IFormFile file,
        [FromQuery] string connectionId,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is missing.");

        //var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //if (!int.TryParse(userIdStr, out var userId))
        //    return Unauthorized("Invalid token: user id missing.");

        //var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        //if (user == null)
        //    return Unauthorized("User not found.");

        //if (user.Role != "TourOperator")
        //    return Forbid("Only TourOperator role can upload files.");

        //if (user.Touroperatorid != tourOperatorId)
        //    return Forbid("You can upload only for your own tourOperatorId.");

        try
        {
            await _fileProcessor.ProcessCsvAsync(file.OpenReadStream(), tourOperatorId, connectionId, ct);
            return Ok(new { message = "Upload Done" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error processing file", detail = ex.Message });
        }
    }
}
