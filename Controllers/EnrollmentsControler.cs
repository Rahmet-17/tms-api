using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Interfaces;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly ICourseService courseService;
    private readonly IEnrollmentService enrollmentService;

    public EnrollmentsController(
        ICourseService courseService,
        IEnrollmentService enrollmentService)
    {
        this.courseService = courseService;
        this.enrollmentService = enrollmentService;
    }

    // GET single enrollment
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var record = await enrollmentService.GetByIdAsync(courseId, id, ct);

        return record is not null ? Ok(record) : NotFound();
    }

    // POST enroll student
   [HttpPost]
public async Task<IActionResult> Create(
    int courseId,
    EnrollStudentRequest request,
    CancellationToken ct)
{
    try
    {
        var result = await enrollmentService.CreateAsync(courseId, request, ct);
        return CreatedAtAction(nameof(GetById),
            new { courseId, id = result.Id },
            result);
    }
    catch (InvalidOperationException ex)
    {
        return Conflict(new
        {
            title = "Enrollment failed",
            detail = ex.Message
        });
    }
}
}