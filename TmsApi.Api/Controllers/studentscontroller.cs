using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence.Data; // IMPORTANT: needed for TmsDbContext
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class StudentsController : ControllerBase
{
   private readonly IStudentService studentService;
private readonly LinkGenerator linkGenerator;

public StudentsController(
    IStudentService studentService,
    LinkGenerator linkGenerator)
{
    this.studentService = studentService;
    this.linkGenerator = linkGenerator;
}

  [HttpGet(Name = nameof(GetAll))]
[ProducesResponseType(
    typeof(IReadOnlyList<StudentResponseDto>),
    StatusCodes.Status200OK)]
[EndpointSummary("List all students")]
[EndpointDescription(
    "Returns all students registered in the system.")]
public async Task<IActionResult> GetAll(
    CancellationToken ct)
{
    var students = await studentService
        .GetAllAsync(ct);

    return Ok(students);
}


[HttpGet("{id:int}", Name = nameof(GetStudentById))]
[ProducesResponseType(
    typeof(StudentDetailDto),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
[EndpointSummary("Get a student by ID")]
[EndpointDescription(
    "Returns student details with HATEOAS links. Returns 404 if the student does not exist.")]
public async Task<IActionResult> GetStudentById(
    int id,
    CancellationToken ct)
{
    var student = await studentService
        .GetByIdAsync(id, ct);

    if (student is null)
        return NotFound();


    var studentUrl = linkGenerator.GetPathByName(
        HttpContext,
        nameof(GetStudentById),
        new { id });


    var links = new List<LinkDto>
    {
        new(
            studentUrl!,
            "self",
            "GET"
        ),

        new(
            studentUrl!,
            "delete",
            "DELETE"
        )
    };


    var detailDto = new StudentDetailDto
    {
        Id = student.Id,
        RegistrationNumber = student.RegistrationNumber,
        Name = student.Name,
        Links = links
    };

    return Ok(detailDto);
}

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateStudentRequest request,
        CancellationToken ct)
    {
        var student = await studentService
            .CreateAsync(request, ct);

        return CreatedAtAction(
            nameof(GetStudentById),
            new { id = student.Id },
            student);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct)
    {
        var deleted = await studentService
            .DeleteAsync(id, ct);

        return deleted
            ? NoContent()
            : NotFound();
    }
}