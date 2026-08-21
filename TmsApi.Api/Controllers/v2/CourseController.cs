using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
[ApiExplorerSettings(GroupName = "v2")]
public class CoursesController(
    ICachedCourseService cachedCourseService,
    ICourseService courseService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        CancellationToken ct = default)
    {
        var result =
            await cachedCourseService.GetAllCoursesAsync(ct);

        return Ok(result);
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        UpdateCourseRequest request,
        CancellationToken ct)
    {
        var result =
            await courseService.UpdateAsync(
                id,
                request,
                ct);


        if (result is null)
            return NotFound();


        await cachedCourseService
            .InvalidateCourseCacheAsync(ct);


        return Ok(result);
    }

    [HttpGet("search")]
[EnableRateLimiting("search")]
public IActionResult SearchCourses(
    [FromQuery] string? term)
{
    return Ok(new
    {
        message = $"Searching courses for {term}"
    });
}
}