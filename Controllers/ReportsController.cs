using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly TmsDbContext _context;

        public ReportsController(TmsDbContext context)
        {
            _context = context;
        }

        // Pagination (Exercise 3 - Task 1)
        [HttpGet("students")]
        public async Task<IActionResult> GetStudents(
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _context.Students
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return Ok(result);
        }

        // Top 5 courses by enrollment (Exercise 3 - Task 2)
        [HttpGet("top-courses")]
        public async Task<IActionResult> GetTopCourses()
        {
            var result = await _context.Enrollments
                .GroupBy(e => e.Course.Title)
                .Select(g => new
                {
                    CourseTitle = g.Key,
                    EnrollmentCount = g.Count()
                })
                .OrderByDescending(x => x.EnrollmentCount)
                .Take(5)
                .ToListAsync();

            return Ok(result);
        }
        [HttpGet("students-paged")]
public async Task<IActionResult> GetStudentsPaged(int page = 1)
{
    int pageSize = 20;

    var result = await _context.Students
        .OrderBy(s => s.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return Ok(result);
}
    }
}