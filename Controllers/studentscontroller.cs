using Microsoft.AspNetCore.Mvc;
using TmsApi.Interfaces;
using TmsApi.Entities;
using TmsApi.Data; // IMPORTANT: needed for TmsDbContext
using Microsoft.EntityFrameworkCore;
using TmsApi.Dtos;
namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService studentService;

    public StudentsController(IStudentService studentService)
    {
        this.studentService = studentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct)
    {
        var students = await studentService
            .GetAllAsync(ct);

        return Ok(students);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken ct)
    {
        var student = await studentService
            .GetByIdAsync(id, ct);

        return student is null
            ? NotFound()
            : Ok(student);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateStudentRequest request,
        CancellationToken ct)
    {
        var student = await studentService
            .CreateAsync(request, ct);

        return CreatedAtAction(
            nameof(GetById),
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