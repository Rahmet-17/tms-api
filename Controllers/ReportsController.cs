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

        // 1. Active students with GPA >= 3.0
        [HttpGet("active-count")]
        public async Task<IActionResult> GetActiveStudentCount()
        {
            var count = await _context.Students
                .Where(s => s.IsActive && s.GPA >= 3.0m)
                .CountAsync();

            return Ok(count);
        }

        // 2. Courses with most enrollments
        [HttpGet("course-enrollments")]
        public async Task<IActionResult> GetCourseEnrollments()
        {
            var list = await _context.Courses
                .Select(c => new
                {
                    c.Title,
                    EnrollmentCount = _context.Enrollments.Count(e => e.CourseId == c.Id)
                })
                .OrderByDescending(x => x.EnrollmentCount)
                .ToListAsync();

            return Ok(list);
        }

        // 3. Average GPA per course
        [HttpGet("average-gpa")]
        public async Task<IActionResult> GetAverageGpa()
        {
            var list = await _context.Enrollments
                .GroupBy(e => e.Course.Title)
                .Select(g => new
                {
                    Course = g.Key,
                    AverageGPA = g.Average(e => e.Student.GPA)
                })
                .ToListAsync();

            return Ok(list);
        }

        // 4A. Students with no enrollments (Subquery)
        [HttpGet("no-enrollments-a")]
        public async Task<IActionResult> GetNoEnrollmentsA()
        {
            var list = await _context.Students
                .Where(s => !_context.Enrollments.Any(e => e.StudentId == s.Id))
                .Select(s => s.Name)
                .ToListAsync();

            return Ok(list);
        }

        // 4B. Students with no enrollments (LEFT JOIN style, EF-safe)
        [HttpGet("no-enrollments-b")]
        public async Task<IActionResult> GetNoEnrollmentsB()
        {
            var list = await _context.Students
                .GroupJoin(
                    _context.Enrollments,
                    s => s.Id,
                    e => e.StudentId,
                    (s, eGroup) => new { s, eGroup }
                )
                .SelectMany(
                    x => x.eGroup.DefaultIfEmpty(),
                    (x, e) => new { x.s, e }
                )
                .Where(x => x.e == null)
                .Select(x => x.s.Name)
                .ToListAsync();

            return Ok(list);
        }
    }
}