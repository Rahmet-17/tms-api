using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence.Data;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace TmsApi.Api.Controllers;

[Authorize(Roles = "Instructor,Admin")]
[ApiController]
[Route("api/[controller]")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICourseService courseService,
    ICachedCourseService cachedCourseService,
    TmsDbContext context,
    LinkGenerator linkGenerator,
    IAuthorizationService authorizationService) : ControllerBase
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
        "Returns a cached paginated list of courses.")]
    public async Task<IActionResult> GetCourses(
        CancellationToken ct)
    {
        var result =
            await cachedCourseService.GetAllCoursesAsync(ct);

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
        "Creates a course and invalidates course cache.")]
    public async Task<IActionResult> Create(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        var exists =
            await courseService.CodeExistsAsync(
                request.Code,
                ct);

        if (exists)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail =
                    $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var result =
            await courseService.CreateAsync(
                request,
                ct);

        await cachedCourseService
            .InvalidateCourseCacheAsync(ct);

        return CreatedAtAction(
            nameof(GetCourseById),
            new { id = result.Id },
            result);
    }


    // PUT: api/courses/{id}
    // Resource-Based Authorization

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourse(
        int id,
        [FromBody] UpdateCourseRequestDto dto,
        CancellationToken ct)
    {
        // Find the actual Course entity.
        // The authorization handler needs the Course resource.

        var course =
            await context.Courses
                .FirstOrDefaultAsync(
                    c => c.Id == id,
                    ct);

        if (course is null)
        {
            return NotFound();
        }


        // Resource-based authorization.
        //
        // Admin:
        //     Can edit any course.
        //
        // Instructor:
        //     Can edit only a course where
        //     InstructorId matches their user ID.

        var authorizationResult =
            await authorizationService.AuthorizeAsync(
                User,
                course,
                "CanEditCourse");


        if (!authorizationResult.Succeeded)
        {
            // Instructor does not own this course.
            // Return 403 Forbidden.

            return Forbid();
        }


        // User is authorized to modify the course.

        course.Title = dto.Title;


        await context.SaveChangesAsync(ct);


        // Clear cached course data because
        // the course has changed.

        await cachedCourseService
            .InvalidateCourseCacheAsync(ct);


        return NoContent();
    }


    // N+1 BAD VERSION

    [HttpGet("nplus1")]
    public async Task<IActionResult> NPlusOne()
    {
        var students =
            await context.Students.ToListAsync();

        var result = new List<object>();

        foreach (var s in students)
        {
            var count =
                await context.Enrollments
                    .CountAsync(
                        e => e.StudentId == s.Id);

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
        var result =
            await context.Students
                .AsNoTracking()
                .Select(s => new
                {
                    s.Name,
                    EnrollmentCount =
                        s.Enrollments.Count
                })
                .ToListAsync();

        return Ok(result);
    }
}