using Microsoft.AspNetCore.Mvc;
using TmsCore.Interfaces;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _service;

    public CoursesController(ICourseService service)
    {
        _service = service;
    }

    [HttpPost]
public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
{
    var course = await _service.CreateAsync(
        request.Code,
        request.Title,
        request.Capacity);

    return Ok(course);
}

public record CreateCourseRequest(string Code, string Title, int Capacity);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await _service.GetAllAsync();
        return Ok(courses);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetById(string code)
    {
        var course = await _service.GetByIdAsync(code);
        return course is null ? NotFound() : Ok(course);
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        var deleted = await _service.DeleteAsync(code);
        return deleted ? NoContent() : NotFound();
    }
}