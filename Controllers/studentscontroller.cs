using Microsoft.AspNetCore.Mvc;
using TmsCore.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService studentService) : ControllerBase
{
    // GET: api/students
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await studentService.GetAllAsync();
        return Ok(students);
    }

    // GET: api/students/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var student = await studentService.GetByIdAsync(id);
        return student is not null ? Ok(student) : NotFound();
    }

    // POST: api/students
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var student = await studentService.CreateAsync(
            request.Name,
            request.Gpa);

        return CreatedAtAction(
            nameof(GetById),
            new { id = student.Id },
            student);
    }

    // DELETE: api/students/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    public record CreateStudentRequest(
        string Name,
        double? Gpa);
}