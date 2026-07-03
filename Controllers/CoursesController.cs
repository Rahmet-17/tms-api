using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Interfaces;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService courseService;
    private readonly TmsDbContext _context;

    public CoursesController(
        ICourseService courseService,
        TmsDbContext context)
    {
        this.courseService = courseService;
        _context = context;
    }

    // GET: api/courses/{id}
    [HttpGet("{id:int}", Name = nameof(GetById))]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);

        return course is not null
            ? Ok(course)
            : NotFound();
    }

    // POST: api/courses
   [HttpPost]
public async Task<IActionResult> Create(
    CreateCourseRequest request,
    CancellationToken ct)
{
    var exists = await courseService
     .CodeExistsAsync(request.Code, ct);

    if (exists)
    {
        return Conflict(new ProblemDetails
        {
            Title = "Course code already exists",
            Detail = $"A course with code '{request.Code}' is already registered.",
            Status = StatusCodes.Status409Conflict
        });
    }

    var result =
        await courseService.CreateAsync(request, ct);

    return CreatedAtAction(
        nameof(GetById),
        new { id = result.Id },
        result);
}


    // N+1 BAD VERSION
    
    [HttpGet("nplus1")]
    public async Task<IActionResult> NPlusOne()
    {
        var students = await _context.Students
            .ToListAsync();

        var result = new List<object>();

        foreach (var s in students)
        {
            var count = await _context.Enrollments
                .CountAsync(e => e.StudentId == s.Id);

            result.Add(new
            {
                s.Name,
                EnrollmentCount = count
            });
        }

        return Ok(result);
    }

   
    //N+1 FIXED VERSION
   
    [HttpGet("nplus1-fixed")]
    public async Task<IActionResult> NPlusOneFixed()
    {
        var result = await _context.Students
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