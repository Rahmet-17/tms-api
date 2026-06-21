using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Interfaces;

namespace TmsApi.Services
{
    public class CourseService : ICourseService
    {
        private readonly TmsDbContext _context;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            TmsDbContext context,
            ILogger<CourseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Course> CreateAsync(
            string code,
            string title,
            int capacity)
        {
            var existing = await _context.Courses
                .FirstOrDefaultAsync(
                    c => c.Code == code || c.Title == title);

            if (existing != null)
            {
                _logger.LogWarning(
                    "Duplicate course {CourseCode} or {CourseTitle} already exists",
                    code,
                    title);

                return existing;
            }

            var course = new Course
            {
                Code = code,
                Title = title,
                Capacity = capacity
            };

            _context.Courses.Add(course);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Created course {CourseCode} with id {CourseId}",
                course.Code,
                course.Id);

            return course;
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                _logger.LogWarning(
                    "Course {CourseId} not found",
                    id);
            }

            return course;
        }

        public async Task<IReadOnlyList<Course>> GetAllAsync()
        {
            return await _context.Courses.ToListAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                _logger.LogWarning(
                    "Course {CourseId} not found",
                    id);

                return false;
            }

            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Deleted course {CourseId}",
                id);

            return true;
        }
    }
}