using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Interfaces;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(
    ICourseService courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    // GET: api/courses/{courseId}/enrollments
    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(
        typeof(IReadOnlyList<EnrollmentResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("List enrolments for a course")]
    [EndpointDescription(
        "Returns all enrolments for the specified course. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetEnrollments(
        int courseId,
        CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);

        if (course is null)
            return NotFound();

        var result = await enrollmentService.GetByCourseAsync(courseId, ct);

        return Ok(result);
    }

    // GET: api/courses/{courseId}/enrollments/{id}
    [HttpGet("{id:int}", Name = nameof(GetById))]
    [ProducesResponseType(
        typeof(EnrollmentResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one enrolment for a course")]
    [EndpointDescription(
        "Returns a single enrolment for the specified course.")]
    public async Task<IActionResult> GetById(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var record = await enrollmentService.GetByIdAsync(courseId, id, ct);

        return record is not null
            ? Ok(record)
            : NotFound();
    }

    // POST: api/courses/{courseId}/enrollments
    [HttpPost]
    [ProducesResponseType(
        typeof(EnrollmentResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    [EndpointSummary("Enrol a student in a course")]
    [EndpointDescription(
        "Returns 404 if the course does not exist and 409 if the course has reached maximum capacity.")]
    public async Task<IActionResult> Create(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await enrollmentService.CreateAsync(
                courseId,
                request,
                ct);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    courseId,
                    id = result.Id
                },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Enrollment failed",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }
}