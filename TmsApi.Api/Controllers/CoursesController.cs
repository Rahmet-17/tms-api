using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence.Data;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICourseService courseService,
    TmsDbContext context,
    LinkGenerator linkGenerator) : ControllerBase
{
    // GET: api/courses/{id}
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(
        typeof(CourseDetailDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription(
        "Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetCourseById(
        int id,
        CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);

        if (course is null)
            return NotFound();

        var courseUrl = linkGenerator.GetPathByName(
            HttpContext,
            nameof(GetCourseById),
            new { id });

        var enrollmentsUrl = linkGenerator.GetPathByAction(
            HttpContext,
            action: "GetEnrollments",
            controller: "Enrollments",
            values: new { courseId = id });

        var links = new List<LinkDto>
        {
            new(courseUrl!, "self", "GET"),
            new(courseUrl!, "update", "PUT"),
            new(courseUrl!, "delete", "DELETE"),
            new(enrollmentsUrl!, "enrollments", "GET")
        };

        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(
                new LinkDto(
                    enrollmentsUrl!,
                    "enroll",
                    "POST"));
        }

        var detailDto = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links
        };

        return Ok(detailDto);
    }

    // GET: api/courses
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResponse<CourseResponseDto>),
        StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription(
        "Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var result = await courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }

    // POST: api/courses
    [HttpPost]
    [ProducesResponseType(
        typeof(CourseResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription(
        "Creates a course with a unique code. Returns 409 if the course code already exists.")]
    public async Task<IActionResult> Create(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        var exists = await courseService.CodeExistsAsync(
            request.Code,
            ct);

        if (exists)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var result = await courseService.CreateAsync(
            request,
            ct);

        return CreatedAtAction(
            nameof(GetCourseById),
            new { id = result.Id },
            result);
    }

    // N+1 BAD VERSION
    [HttpGet("nplus1")]
    public async Task<IActionResult> NPlusOne()
    {
        var students = await context.Students.ToListAsync();

        var result = new List<object>();

        foreach (var s in students)
        {
            var count = await context.Enrollments
                .CountAsync(e => e.StudentId == s.Id);

            result.Add(new
            {
                s.Name,
                EnrollmentCount = count
            });
        }

        return Ok(result);
    }

    // N+1 FIXED VERSION
    [HttpGet("nplus1-fixed")]
    public async Task<IActionResult> NPlusOneFixed()
    {
        var result = await context.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync();

        return Ok(result);
    }
}