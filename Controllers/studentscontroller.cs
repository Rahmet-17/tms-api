using Microsoft.AspNetCore.Mvc;
using TmsApi.Interfaces;
using TmsApi.Entities;
using TmsApi.Data; // IMPORTANT: needed for TmsDbContext
using Microsoft.EntityFrameworkCore;
namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;
    private readonly TmsDbContext _context;

    public StudentsController(IStudentService service, TmsDbContext context)
    {
        _service = service;
        _context = context;
    }

    // GET: api/students
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    // GET: api/students/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var student = await _service.GetByIdAsync(id);
        return student is not null ? Ok(student) : NotFound();
    }

    // POST: api/students
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var student = await _service.CreateAsync(request.Name, request.GPA);
        return Ok(student);
    }

    // DELETE: api/students/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }

    // SOFT DELETE: api/students/soft/{id}
    [HttpDelete("soft/{id:int}")]
public async Task<IActionResult> SoftDelete(int id)
{
    var student = await _context.Students
        .FirstOrDefaultAsync(s => s.Id == id);

    if (student == null)
        return NotFound();

    _context.Entry(student).State = EntityState.Modified;

    student.IsDeleted = true;

    await _context.SaveChangesAsync();
    Console.WriteLine("SOFT DELETE HIT - CONTROLLER VERSION 1");

    return Ok("Student soft deleted");
    
}

[HttpPut("archive-all")]
public async Task<IActionResult> ArchiveAll()
{
    var affectedRows = await _context.Students
        .Where(s => !s.IsDeleted)
        .ExecuteUpdateAsync(s => s
            .SetProperty(x => x.IsDeleted, x => true));

    return Ok($"{affectedRows} students archived");
}

[HttpPut("restore/{id:int}")]
public async Task<IActionResult> Restore(int id)
{
    var student = await _context.Students
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(s => s.Id == id);

    if (student == null || student.IsDeleted == false)
        return NotFound("Student not found or not deleted");

    student.IsDeleted = false;

    await _context.SaveChangesAsync();

    return Ok("Student restored");
}
[HttpGet("debug-all")]
public async Task<IActionResult> DebugAll()
{
    var students = await _context.Students
        .IgnoreQueryFilters()
        .ToListAsync();

    return Ok(students);
}
    public record CreateStudentRequest(string Name, decimal? GPA);
}